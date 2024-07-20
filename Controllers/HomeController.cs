using AutoMapper;
using Group5.Data;
using Group5.Models;
using Group5.Service;
using Group5.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace Group5.Controllers
{
   /* [AllowAnonymous]*/
    public class HomeController : BaseController
    {
        public HomeController(ApplicationDbContext ctx, IMapper mapper, ICartService service, IRequestService request, ICustomRoleService customRoleService) : base(ctx, mapper, service, request, customRoleService)
        {
        }

        /*   [Authorize(Policy = "ManageOnly")]
           [Authorize(Policy = "AdminOnly")]*/
        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
          
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
                //check User Request Amount reset or not
                var currentMonth = DateTime.Now.Month;
                int? resetAmountTime = user!.ResetAmountTime?.Month;
                if (currentMonth > resetAmountTime)
                {
                    var amount = user!.EmployeePositions!.MaxAmountPerMonth;
                    user.AmountRequestPerMonth = amount;
                    user.ResetAmountTime = DateTime.Now;
                    ctx.Users.Update(user);
                    await ctx.SaveChangesAsync();
                }
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
                var distinctBrand = await ctx.Brands
                     .Select(r => r.Name)
                     .Distinct()
                     .ToListAsync();
                ViewBag.Brands = distinctBrand;
                ViewBag.Categories = distinctCategories;

                return View(viewitems);
            
          
     
        }
        public async Task<IActionResult> DetailSta(int id)
        {
            await PrepareCommonDataAsync();

            var item = await ctx.StationeryItems.Include(a => a.Categories)
                .Include(b => b.Brand)
                .SingleOrDefaultAsync(c => c.Id == id);
            return View(item);
        }

        // Show Stationery by Category Ajax

        public IActionResult GetStationeryByCategory(string category ,string brand)
        {
            List<StationeryItem> items = new List<StationeryItem>();
            Console.WriteLine($"aaaaassssssssssssabrand:{brand}");
            if(brand == "allbrand")
            {
                Console.WriteLine($"aaaaassssssssssssabrand:{brand}");
                items = ctx.StationeryItems
                  .Where(a => a.Categories!.Name == category)
                      .Where(c => c.IsHide != true)
                  .Include(a => a.Categories)
                  .Include(a => a.Brand)
                  .ToList();
              
            }
            else
            {
                items = ctx.StationeryItems
                   .Where(a => a.Categories!.Name == category)
                   .Where(a => a.Brand!.Name == brand)
                       .Where(c => c.IsHide != true)
                   .Include(a => a.Categories)
                   .Include(a => a.Brand)
                   .ToList();

            }
 
    
            var viewitems = mapper.Map<List<StationeryItemViewModel>>(items);


            return PartialView("PartialView", viewitems);

        }




        public IActionResult Privacy()
        {
            return View();
        }

     
    }
}