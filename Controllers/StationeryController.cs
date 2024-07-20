using AutoMapper;
using Group5.Data;
using Group5.Models;
using Group5.Service;
using Group5.Shared;
using Group5.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Group5.Controllers
{

    public class StationeryController : BaseController
    {
        public ICommonMethod common;
        public StationeryController(ApplicationDbContext ctx, IMapper mapper, ICartService service, IRequestService request, ICommonMethod common, ICustomRoleService customRoleService) : base(ctx, mapper, service, request, customRoleService)
        {
            this.common = common;
        }

        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewStationery"))
            {
                await PrepareCommonDataAsync();
                var items = await ctx.StationeryItems

                    .Include(a => a.Categories)
                    .Include(a => a.Brand)
                    .Where(c => c.IsHide != true)
                    .ToListAsync();

                var viewitems = mapper.Map<List<StationeryItemViewModel>>(items);

                var LoginEmail = "";
                if (User!.Identity!.IsAuthenticated)
                {
                    LoginEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
                }
                var user = await ctx.Users
                     .Include(a => a.Departments)
                     .Include(a => a.EmployeePositions)
                    .FirstOrDefaultAsync(a => a.Email == LoginEmail);
                var cart = await service.CreateCart(user!);
                var cartItem = await ctx.CartItems
                               .Include(a => a.StationeryItem)
                               .Where(a => a.CartId == cart.Id)
                               .ToListAsync();
                ViewData["loginCart"] = cart;
                ViewData["loginCartItem"] = cartItem;

                var distinctCategories = await ctx.Categories
                    .Select(r => r.Name)
                    .Distinct()
                    .ToListAsync();

                ViewBag.Categories = distinctCategories;

                return View(viewitems);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }
        }

        public async Task<IActionResult> Hiddenindex()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewStationery"))
            {
                await PrepareCommonDataAsync();
                var item = await ctx.StationeryItems
                    .Include(b => b.Categories)
                    .Include(c => c.Brand)
                .Where(c => c.IsHide == true)

                    .ToListAsync();
                var viewitems = mapper.Map<List<StationeryItemViewModel>>(item);
                return View(viewitems);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }


          
        }

        public async Task<IActionResult> Chart()
        {
            await PrepareCommonDataAsync();
            return View();
        }

        public async Task<IActionResult> ChartData()
        {
            var employeeList = await ctx.Users.ToListAsync();
            var listproduct = await ctx.StationeryItems
                .ToListAsync();
            var totalCost = listproduct.Sum(a => a.Price);

            var dataPoints = new List<object>();

            foreach (var item in listproduct)
            {
                var totalquantity = item.Quantity;
                var totalemp = employeeList.Count;
                var percentQuantityAndEmp = (totalquantity / totalemp);
                var percent = (item.Price / totalCost) * 100;
                dataPoints.Add(new { ProductName = item.Name, PercentCost = percent, totalQuantity = totalquantity, quantityAndEmp = percentQuantityAndEmp });
            }

            return Json(new
            {
                success = true,
                data = dataPoints
            });
        }


        //Stationery
        public async Task<IActionResult> CreateStationery()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewStationery"))
            {
                var listcate = await ctx.Categories.ToListAsync();
                ViewBag.List = new SelectList(listcate, "Id", "Name");
                var listbr = await ctx.Brands.ToListAsync();
                ViewBag.brList = new SelectList(listbr, "Id", "Name");
                await PrepareCommonDataAsync();
                return View();
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }


    
        }

        [HttpPost]
        public async Task<IActionResult> CreateStationery(StationeryItemViewModel product)

        {
            //Console.WriteLine($"qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqq{product.CategoryId}");
            var photourl = await common.UploadImage(product!.Photo!);
            var viewitem = mapper.Map<StationeryItem>(product);
            viewitem.ImageUrl = photourl;
            await PrepareCommonDataAsync();
            if (ModelState.IsValid)
            {
                viewitem.CreatedAt = DateTime.Now;
                viewitem.LastStockOut = DateTime.Now;   
                ctx.StationeryItems.Add(viewitem);
                await ctx.SaveChangesAsync();
                TempData["success"] = "Create Stationery successfully";
                return RedirectToAction("Index");

            }

            var listcate = await ctx.Categories.ToListAsync();
            ViewBag.List = new SelectList(listcate, "Id", "Name");
            var listbr = await ctx.Brands.ToListAsync();
            ViewBag.brList = new SelectList(listbr, "Id", "Name");
            await PrepareCommonDataAsync();
            return View(product);
        }
        public async Task<IActionResult> DetailSta(int id)
        {
            await PrepareCommonDataAsync();

            var item = await ctx.StationeryItems.Include(a => a.Categories)
                .Include(b => b.Brand)
                .SingleOrDefaultAsync(c => c.Id == id);
            return View(item);
        }

        public async Task<IActionResult> EditStationery(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewStationery"))
            {
                await PrepareCommonDataAsync();


                var items = await ctx.StationeryItems
                    .Include(a => a.Categories)
                    .Include(a => a.Brand)
                    .SingleOrDefaultAsync(a => a.Id == id);
                var listcate = await ctx.Categories

                    .ToListAsync();


                ViewBag.List = new SelectList(listcate, "Id", "Name");

                var listbr = await ctx.Brands

                    .ToListAsync();
                ViewBag.brandList = new SelectList(listbr, "Id", "Name");
                return View(items);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditStationery(int id, StationeryItem editedStationery)
        {
            await PrepareCommonDataAsync();
            // Find the existing StationeryItem in the database
            var existingStationery = await ctx.StationeryItems
                .FirstOrDefaultAsync(s => s.Id == id);

            //if (existingStationery == null)
            if (ModelState.IsValid)
            {
                // Handle the case where the StationeryItem with the given id is not found
                // throw new InvalidOperationException("StationeryItem not found");

                // Update the properties of the existing StationeryItem
                existingStationery.Name = editedStationery.Name;
                existingStationery.Description = editedStationery.Description ?? existingStationery.Description;
                existingStationery.Price = editedStationery.Price ?? existingStationery.Price;

                existingStationery.ImageUrl = editedStationery.ImageUrl ?? existingStationery.ImageUrl;
                existingStationery.Quantity = editedStationery.Quantity ?? existingStationery.Quantity;
                existingStationery.TypeOfQuantity = editedStationery.TypeOfQuantity ?? existingStationery.TypeOfQuantity;

                // Update the foreign key relationships
                existingStationery.CategoryId = editedStationery.CategoryId ?? existingStationery.CategoryId;
                existingStationery.BrandId = editedStationery.BrandId ?? existingStationery.BrandId;
                existingStationery.StockLevel = editedStationery.StockLevel ?? existingStationery.StockLevel;

                // Set the entity state to Modified to indicate changes
                ctx.Entry(existingStationery).State = EntityState.Modified;
            }
            try
            {
                // Save changes to the database
                await ctx.SaveChangesAsync();
                TempData["success"] = "change Stationery success";
                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                // Handle concurrency issues if needed
                throw;
            }
        }
        public async Task<IActionResult> EditHiddenStationery(int id)
        {
            await PrepareCommonDataAsync();
            var existingStationery = await ctx.StationeryItems
                .FirstOrDefaultAsync(s => s.Id == id);
         
                existingStationery.IsHide = false;
                ctx.Entry(existingStationery).State = EntityState.Modified;
                // Save changes to the database
                await ctx.SaveChangesAsync();
                TempData["success"] = "Edit Stationery successfully";
                return RedirectToAction("Index");
          
        }

        public async Task<IActionResult> DeleteStationery(int id)
        {
            await PrepareCommonDataAsync();
            if (ModelState.IsValid)
            {
                var choseitem = await ctx.StationeryItems.
                    Where(c => c.Id == id).SingleOrDefaultAsync();
          /*      if (choseitem.Quantity > 0)
                {
                    TempData["error"] = "Cannot delete StationeryItem with a non-zero quantity.";
                    return RedirectToAction("Index");
                }*/
                if (choseitem != null)
                {
                    // Check if the item is already hidden
                    if (choseitem.IsHide == true)
                    {
                        TempData["error"] = "StationeryItem is already hidden.";
                        return RedirectToAction("Index");
                    }

                    // Update the Status property to indicate hiding
                    choseitem.IsHide = true;

                    try
                    {
                        ctx.Entry(choseitem).State = EntityState.Modified;
                        await ctx.SaveChangesAsync();
                        TempData["success"] = "Hide StationeryItem success";
                        return RedirectToAction("Index");
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        // Handle concurrency issues if needed
                        throw;
                    }
                }
                else
                {
                    TempData["error"] = "StationeryItem not found.";
                    return RedirectToAction("Index");
                }
            }
            return View();
        }



        //Caterogy

        public async Task<IActionResult> Category()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewStationery"))
            {
                await PrepareCommonDataAsync();
                var items = await ctx.Categories.ToListAsync();
                return View(items);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }

           
        }

        public async Task<IActionResult> CreateCate()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewStationery"))
            {
                await PrepareCommonDataAsync();
                return View();
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }


        }

        [HttpPost]
        public async Task<IActionResult> CreateCate(Category cate)
        {
            await PrepareCommonDataAsync();
            if (ModelState.IsValid)
            {
                ctx.Categories.Add(cate);
                await ctx.SaveChangesAsync();
                TempData["success"] = "Create Category Successfully";
                return RedirectToAction("Category");

            }
            return View(cate);
        }

        public async Task<IActionResult> DetailsCate(int Id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewStationery"))
            {
                await PrepareCommonDataAsync();

                var categoryWithItems = await ctx.StationeryItems
                .Where(c => c.CategoryId == Id)
                    .Where(c => c.IsHide != true)
                .Include(c => c.Categories)
                .ToListAsync();

                if (categoryWithItems == null)
                {
                    return NotFound();
                }

                // Pass the category and associated items to the view
                return View(categoryWithItems);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }


        
        }

        public async Task<IActionResult> EditCate(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewStationery"))
            {
                await PrepareCommonDataAsync();
                var items = await ctx.Categories
                    .SingleOrDefaultAsync(a => a.Id == id);
                return View(items);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }


          
        }


        [HttpPost]
        public async Task<IActionResult> EditCate(int id, Category editcate)
        {
            await PrepareCommonDataAsync();
            var existingCate = await ctx.Categories
                .FirstOrDefaultAsync(s => s.Id == id);
            if (existingCate!.Name == editcate.Name)
            {
                return RedirectToAction("Category");
            }


            if (ModelState.IsValid)
            {
              
                existingCate!.Name = editcate.Name;
                ctx.Entry(existingCate).State = EntityState.Modified;
                await ctx.SaveChangesAsync();
                TempData["success"] = "Edit Category Successfully";
                return RedirectToAction("Category");
            }
    
            return View(editcate);

        }

        public async Task<IActionResult> DeleteCate(int id)
        {
            await PrepareCommonDataAsync();
            var category = await ctx.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            // Ensure that no StationeryItem is referencing this category
            var associatedItems = await ctx.StationeryItems.Where(s => s.CategoryId == id).ToListAsync();

            if (associatedItems.Count > 0)
            {
                TempData["error"] = "Cant Delete";
                return RedirectToAction("Category");
            }

            ctx.Categories.Remove(category);
            await ctx.SaveChangesAsync();
            TempData["success"] = "Delete Category Sucessfully";
            return RedirectToAction("Category");
        }

        //brand
        public async Task<IActionResult> Brand()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewStationery"))
            {
                await PrepareCommonDataAsync();
                var items = await ctx.Brands.ToListAsync();
                var LoginEmail = "";
                if (User!.Identity!.IsAuthenticated)
                {
                    LoginEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
                }
                var user = await ctx.Users
                     .Include(a => a.Departments)
                     .Include(a => a.EmployeePositions)
                    .FirstOrDefaultAsync(a => a.Email == LoginEmail);
                var cart = await service.CreateCart(user!);
                var cartItem = await ctx.CartItems
                               .Include(a => a.StationeryItem)
                               .Where(a => a.CartId == cart.Id)
                               .ToListAsync();
                ViewData["loginCart"] = cart;
                ViewData["loginCartItem"] = cartItem;
                return View(items);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }
        }

        public async Task<IActionResult> CreateBrand()
        {

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewStationery"))
            {

                await PrepareCommonDataAsync();
                return View();
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }


        }

        [HttpPost]
        public async Task<IActionResult> CreateBrand(Brand bproduct)
        {
            await PrepareCommonDataAsync();
            if (ModelState.IsValid)
            {
                ctx.Brands.Add(bproduct);
                await ctx.SaveChangesAsync();
                TempData["success"] = "Create Brand successfully";
                return RedirectToAction("Brand");

            }
            return View(bproduct);
        }

        public async Task<IActionResult> EditBrand(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewStationery"))
            {
                await PrepareCommonDataAsync();
                var existingbrand = await ctx.Brands
                  .FirstOrDefaultAsync(s => s.Id == id);
                return View(existingbrand);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }

         
        }

        [HttpPost]
        public async Task<IActionResult> EditBrand(int id, Brand editbr)
        {
            await PrepareCommonDataAsync();


            var existingbrand = await ctx.Brands
                .FirstOrDefaultAsync(s => s.Id == id);
            if(existingbrand!.Name == editbr.Name)
            {
                return RedirectToAction("Brand");
            }
            if (ModelState.IsValid)
            {
                existingbrand.Name = editbr.Name;
                ctx.Entry(existingbrand).State = EntityState.Modified;
                await ctx.SaveChangesAsync();
                TempData["success"] = "Change brand successfully";
                return RedirectToAction("Brand");
            }
        
          return View(editbr);

        }
        public async Task<IActionResult> DetailsBrand(int Id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewStationery"))
            {
                await PrepareCommonDataAsync();

                var brandWithItems = await ctx.StationeryItems
                .Where(c => c.BrandId == Id)
                    .Where(c => c.IsHide != true)
                .Include(c => c.Brand)
                .ToListAsync();

                if (brandWithItems == null)
                {
                    return NotFound();
                }

                // Pass the category and associated items to the view
                return View(brandWithItems);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }
        }

        public async Task<IActionResult> DeleteBrand(int id)
        {
            await PrepareCommonDataAsync();
            var brand = await ctx.Brands.FindAsync(id);

            if (brand == null)
            {
                return NotFound();
            }

            // Ensure that no StationeryItem is referencing this category
            var associatedItems = await ctx.StationeryItems.Where(s => s.BrandId == id).ToListAsync();

            if (associatedItems.Count > 0)
            {
                /*TempData["error"] = "cannot delete brand has item";*/

                TempData["error"] = "Cant Delete";
                return RedirectToAction("Brand");
            }

            ctx.Brands.Remove(brand);
            await ctx.SaveChangesAsync();
            TempData["success"] = "Delete brand successfully";
            return RedirectToAction("Brand");
        }




        // Show Stationery by Category Ajax

        public IActionResult GetStationeryByCategory(string category)
        {

            var items = ctx.StationeryItems
               .Where(a => a.Categories!.Name == category)
                   .Where(c => c.IsHide != true)
               .Include(a => a.Categories)
               .Include(a => a.Brand)
               .ToList();
            Console.WriteLine($"aaaaaaaaaaaaaaaaa{items}");
            var viewitems = mapper.Map<List<StationeryItemViewModel>>(items);


            return PartialView("_StationeryPartial", viewitems);

        }
    }
}
