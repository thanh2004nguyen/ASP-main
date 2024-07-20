using AutoMapper;
using Group5.Data;
using Group5.Models;
using Group5.Service;
using Group5.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Group5.Controllers
{
    public class StockController : BaseController
    {
        public StockController(ApplicationDbContext ctx, IMapper mapper, ICartService service, IRequestService request, ICustomRoleService customRoleService) : base(ctx, mapper, service, request, customRoleService)
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> StockLevel()
        {
            await PrepareCommonDataAsync();
            var Stocks = await ctx.StockLevels.ToListAsync();

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewStock"))
            {
                return View(Stocks);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }

          
        }

        public async Task<IActionResult> CreateStockLevel()
        {
            await PrepareCommonDataAsync();

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewStock"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }

           
        }

        [HttpPost]
        public async Task<IActionResult> CreateStockLevel(StockLevel stock)
        {
            await PrepareCommonDataAsync();

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewStock"))
            {
                if (ModelState.IsValid)
                {
                    ctx.Entry(stock).State = EntityState.Added;
                    await ctx.SaveChangesAsync();
                    TempData["success"] = "Create Stock Level Success";
                    return RedirectToAction("StockLevel");
                }
                return View(stock);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }
        }


        public async Task<IActionResult> UpdateStockLevel(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewStock"))
            {
                await PrepareCommonDataAsync();
                var stock = await ctx.StockLevels.SingleOrDefaultAsync(a => a.Id == id);
                return View(stock);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }


         
        }
        [HttpPost]
        public async Task<IActionResult> UpdateStockLevel(int id,StockLevel stock)
        {
            await PrepareCommonDataAsync();
            var stocklv = await ctx.StockLevels.SingleOrDefaultAsync(a => a.Id == id);
            if (stocklv!.Level == stock.Level)
            {
                stocklv.MinQuantity = stock.MinQuantity;
                ctx.Entry(stocklv).State = EntityState.Modified; //update(modified)
                await ctx.SaveChangesAsync();
                TempData["success"] = "Update Stock Level Success";
                return RedirectToAction("StockLevel");
            }
            if (ModelState.IsValid)
            {
                stocklv.Level = stock.Level;
                stocklv.MinQuantity = stock.MinQuantity;
                ctx.Entry(stocklv).State = EntityState.Modified; //update(modified)
                await ctx.SaveChangesAsync();
                TempData["success"] = "Update Stock Level Success";
                return RedirectToAction("StockLevel");
            }
            return View(stock);
        }
        public async Task<IActionResult> Delete(int id)
        {
            await PrepareCommonDataAsync();
            var stock = await ctx.StockLevels.FindAsync(id);

            ctx.Entry(stock).State = EntityState.Deleted;
            await ctx.SaveChangesAsync();
            TempData["success"] = "Delete Stock Level Success";
            return RedirectToAction("StockLevel");

        }


        // [... UseLess...]

        public async Task<IActionResult> UseLessItem()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewStock"))
            {
                await PrepareCommonDataAsync();
                var UseLess = await ctx.UseLessItems.ToListAsync();
                return View(UseLess);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }

        }
        public async Task<IActionResult> CreateULessLevel()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewStock"))
            {
                await PrepareCommonDataAsync();
                // lọc ra list StationeryItem ktra quantity bé hơn 10 thì update lại StockLevel bằng với 1.
                return View();
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }

         
        }

        [HttpPost]
        public async Task<IActionResult> CreateULessLevel(UseLessItem stock)
        {
            await PrepareCommonDataAsync();
            if (ModelState.IsValid)
            {
                ctx.Entry(stock).State = EntityState.Added;
                await ctx.SaveChangesAsync();
                TempData["success"] = "Create UseLessItem Success";
                return RedirectToAction("UseLessItem");
            }
            return View(stock);
        }


        public async Task<IActionResult> UpdateUseLess(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewStock"))
            {
                await PrepareCommonDataAsync();
                var stock = await ctx.UseLessItems.SingleOrDefaultAsync(a => a.Id == id);
                return View(stock);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }

          
        }
        [HttpPost]
        public async Task<IActionResult> UpdateUseLess(int id,UseLessItem stock)
        {
            await PrepareCommonDataAsync();
            var stocklv = await ctx.UseLessItems.SingleOrDefaultAsync(a=>a.Id== id);  
            if(stocklv!.StockLevel == stock.StockLevel)
            {
                stocklv.MaxTime = stock.MaxTime;
                ctx.Entry(stocklv).State = EntityState.Modified; //update(modified)
                await ctx.SaveChangesAsync();
                TempData["success"] = "Update UseLessItem Success";
                return RedirectToAction("UseLessItem");
            }
            if (ModelState.IsValid)
            {
                stocklv.StockLevel = stock.StockLevel;
                stocklv.MaxTime =stock.MaxTime;
                ctx.Entry(stocklv).State = EntityState.Modified; //update(modified)
                await ctx.SaveChangesAsync();
                TempData["success"] = "Update UseLessItem Success";
                return RedirectToAction("UseLessItem");
            }
            return View(stock);
        }

        public async Task<IActionResult> DeleteUless(int id)
        {
            await PrepareCommonDataAsync();
            var stock = await ctx.UseLessItems.FindAsync(id);

            ctx.Entry(stock).State = EntityState.Deleted;
            await ctx.SaveChangesAsync();
            TempData["success"] = "Delete UseLessItem Success";
            return RedirectToAction("UseLessItem");

        }
        public async Task<IActionResult> DeleteRequest(int id)
        {
            await PrepareCommonDataAsync();
            var stock = await ctx.NewStationeryRequests.FindAsync(id);

            ctx.Entry(stock).State = EntityState.Deleted;
            await ctx.SaveChangesAsync();
            TempData["success"] = "Delete UseLessItem Success";
            return RedirectToAction("ListRequest");
        }
        public async Task<IActionResult> ListStationeryLevel()
        {
            var lvo = await ctx.UseLessItems.SingleOrDefaultAsync(a=>a.StockLevel == 0);    
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewStock"))
            {
                await PrepareCommonDataAsync();
                var items = await ctx.StationeryItems
                    .Include(a => a.Categories)
                    .Include(a => a.StockLevel).Where(a => a.StockLevel!.Level < 5)
                    .Include(a => a.Brand)
                    .ToListAsync();

                // Update Stocklv for all Item in Stock
                var uless = await ctx.StationeryItems
                    .ToListAsync();
                foreach (var i in uless)
                {
                    if (i.LastStockOut != null)
                    {
                        TimeSpan daysDifference = (TimeSpan)(DateTime.Now - i.LastStockOut!);
                        var days = (int)daysDifference.TotalDays;
                        if(days == 0)
                        {
                            await request.ApplyUselessLv(0, i.Id);
                        }
                        await request.ApplyUselessLv(days, i.Id);
                    }
                    await request.ApplyStockLv(i.Quantity, i.Id);
                    var currentYear = DateTime.Now.Year;
                    int? checkTimeYear = i.CheckForUseLessTime?.Year;
                    if (checkTimeYear != null)
                    {
                        if (currentYear > checkTimeYear)
                        {
                            i.isCheckUseLess = false;
                        }
                    }

                }
                var useless = await ctx.StationeryItems
                     .Include(a => a.Categories)
                     .Include(a => a.UseLessItem)
                     .Where(a => a.UseLessItem != null)
                     .Where(a => a.UseLessId != lvo!.Id)
                     .ToListAsync();

                ViewBag.uselessList = useless;

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
                return View(viewitems);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }
        }


        public async Task<IActionResult> OderMoreGood(int id)
        {

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewStock"))
            {
                await PrepareCommonDataAsync();

                var stock = await ctx.StationeryItems.FindAsync(id);
                return View(stock);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }
        }

        [HttpPost]
        public async Task<IActionResult> OderMoreGood(StationeryItem item)
        {
            await PrepareCommonDataAsync();
            if (ModelState.IsValid)
            {
                var i = await ctx.StationeryItems.FindAsync(item.Id);

                var LoginEmail = "";
                if (User!.Identity!.IsAuthenticated)
                {
                    LoginEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
                }
                var user = await ctx.Users
                     .Include(a => a.Departments)
                     .Include(a => a.EmployeePositions)
                    .FirstOrDefaultAsync(a => a.Email == LoginEmail);

                NewStationeryRequest request = new NewStationeryRequest()
                {
                    CreatedAt = DateTime.Now,
                    Quantity = item.Quantity,
                    StationeryItem = i,
                    RequestStatusId = 9,
                    RequestBy = user,
                    Type = "Restock",
                    ItemName = item.Name,
                   
                };
                ctx.NewStationeryRequests.Add(request);

                i.StatusRestock = "Ordering";
                ctx.StationeryItems.Update(i);
                await ctx.SaveChangesAsync();
                TempData["success"] = "Add NewStationeryRequest Success";
                return RedirectToAction("ListStationeryLevel");
            }

            return View(item);

        }

        public async Task<IActionResult> ListRequest()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewStock"))
            {
                await PrepareCommonDataAsync();

                var requests = await ctx.NewStationeryRequests
                    .Include(a => a.RequestStatus)
                    .Include(a => a.StationeryItem)
                    .Where(a => a.Type == "Restock")
                    .ToListAsync();
                return View(requests);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }
        }
        public async Task<IActionResult> RequestDetail(int id)
        {

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewStock"))
            {
                await PrepareCommonDataAsync();

                var request = await ctx.NewStationeryRequests
                    .Include(a => a.RequestStatus)
                    .Include(a => a.StationeryItem)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (request == null)
                {
                    return NotFound();
                }

                return View(request);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }



    
        }

 
        public async Task<IActionResult> Approve(int id)
        {
            var newstationeryrequest = await ctx.NewStationeryRequests.FirstOrDefaultAsync(a => a.Id == id);
            var request = await ctx.NewStationeryRequests
                .Include(a => a.StationeryItem)
                .ThenInclude(a => a.StockLevel)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            // Kiểm tra MinQuatity có lớn hơn Quantity trong Stationery hay không nếu có thì + vào trong Stationery.Quatity
            var listStocklv = await ctx.StockLevels
                .Where(a => a.MinQuantity > request.StationeryItem!.Quantity)
                .ToListAsync();

            request.StationeryItem!.Quantity += newstationeryrequest!.Quantity;
            await ctx.SaveChangesAsync();

            if (listStocklv == null || listStocklv.Count == 0)
            {
                var stationeryItem = request.StationeryItem;
                stationeryItem.StockLevel = null;
                stationeryItem.StatusRestock = null;
                ctx.StationeryItems.Update(stationeryItem);
                await ctx.SaveChangesAsync();

            }
            else
            {
                var minStockLv = listStocklv.OrderBy(a => a.MinQuantity).First();
                var stationeryItem = request.StationeryItem;
                stationeryItem.StockLvId = minStockLv.Id;
                stationeryItem.StatusRestock = null;
                ctx.StationeryItems.Update(stationeryItem);
                await ctx.SaveChangesAsync();
            }



            request.RequestStatusId = 7;
            await ctx.SaveChangesAsync();
            TempData["success"] = "Approve Success";
            return RedirectToAction("RequestDetail", new { Id = id });
        }

        public async Task<IActionResult> CheckUseLess(int id)
        {
           var item = await ctx.StationeryItems.SingleOrDefaultAsync(ctx => ctx.Id == id);
            item!.isCheckUseLess = true;
            item!.CheckForUseLessTime = DateTime.Now;

            ctx.StationeryItems.Update(item);
            await ctx.SaveChangesAsync();   
            return RedirectToAction("ListStationeryLevel");
        }
    }
}
