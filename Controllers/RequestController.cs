using AutoMapper;
using Group5.Data;
using Group5.Models;
using Group5.Models.Base;
using Group5.Service;
using Group5.Shared;
using Group5.ViewModels;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Ocsp;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Channels;

namespace Group5.Controllers
{
    public class RequestController : BaseController
    {
        public IEmployeeService employeeService;
        public ICommonMethod common;
        public RequestController(ApplicationDbContext ctx, IMapper mapper, ICartService service, IRequestService request, ICommonMethod common, IEmployeeService employeeService, ICustomRoleService customRoleService) : base(ctx, mapper, service, request, customRoleService)
        {
            this.common = common;
            this.employeeService = employeeService;
        }

        public async Task<IActionResult> Index()
        {
            await PrepareCommonDataAsync();
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "CanRequest"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }

        }

        [HttpPost]
        public async Task<IActionResult> CreateRequet()
        {
            await PrepareCommonDataAsync();

            var roles = User.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();
            bool hasCEORole = roles.Any(r => string.Equals(r, "CEO", StringComparison.OrdinalIgnoreCase));
            var status = await ctx.RequestStatus.SingleOrDefaultAsync(a => a.Status == "Wait For Approvement"); 
            var statusAppro = await ctx.RequestStatus.SingleOrDefaultAsync(a => a.Status == "Approved");
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "CanRequest"))
            {
                var user = await ctx.Users.SingleOrDefaultAsync(a => a.Email == User.FindFirstValue(ClaimTypes.NameIdentifier));
                var cart = await service.CreateCart(user!);
                var cartItems = await service.ListCartItems(cart);
                var total = cart.CartItems!.Sum(ci => ci.Quantity * ci.StationeryItem!.Price);
                if (total == 0)
                {
                    TempData["message"] = "Request List Empty!!! Please Try Again";
                    return RedirectToAction("Index", "Home");
                }
                StationeryRequest stationeryRequest = new StationeryRequest();

                if (hasCEORole)
                {
                    stationeryRequest.RequestStatusId = statusAppro!.Id;
                }
                if(!hasCEORole)
                {
                    stationeryRequest.RequestStatusId = status!.Id;
                }
                stationeryRequest.CreatedAt = DateTime.Now;
                stationeryRequest.RequestBy = user;
                stationeryRequest.Total = total;
                ctx.StationeryRequests.Add(stationeryRequest);
                await ctx.SaveChangesAsync();
                user!.AmountRequestPerMonth -= total;
                ctx.Users.Update(user);
                await ctx.SaveChangesAsync();

                foreach (var item in cartItems)
                {
                    RequestItem items = new RequestItem();

                    if(hasCEORole)
                    {
                        items.Status = "Approved";
                    }
                    if(!hasCEORole)
                    {
                        items.Status = "Wait For Approvement";
                    }
                    items.StationeryRequestId = stationeryRequest.Id;
                    items.StationeryItem = item.StationeryItem;
                    items.Quantity = item.Quantity;
                    items.CreatedDate = DateTime.Now;
                    ctx.RequestItems.Add(items);
                    await ctx.SaveChangesAsync();
                }
                //empty cart
                await service.EmpltyCart(cart);
                HttpContext.Session.SetInt32("cartitems", 0);
                TempData["success"] = "Made Request Successfully!!!";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }

        }

        public async Task<IActionResult> ListReceivedRequest()
        {
            var user = await ctx.Users.SingleOrDefaultAsync(a => a.Email == User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = await ctx.EmployeeRoles
                .Where(a => a.EmployeeId == user!.Id)
                .Select(a => a.Role!.Name)
                .ToListAsync();
            if (userRole.Any(role => role!.ToLower() == "ceo"))
            {
                await PrepareCommonDataAsync();
                var requestss = await ctx.StationeryRequests
                      .Include(a => a.RequestBy)
                      .Include(a => a.RequestStatus)
                      .Where(a => a.RequestBy!.SupperVisorId == user!.Id ||  a.RequestBy.Id == user.Id)
                      .OrderByDescending(a => a.RequestStatus!.Status == "Wait For Approvement")
                      .ThenByDescending(a => a.CreatedAt)
                    .ToListAsync();
                var newStationeryrequests = await ctx.NewStationeryRequests
                   .Include(a => a.RequestBy)
                   .Include(a => a.RequestStatus)
                   .Where(a => a.RequestBy!.SupperVisorId == user!.Id || a.RequestBy.Id == user.Id)
                .ToListAsync();
                var combinedRequestss = requestss.Cast<RequestBase>()
                        .Concat(newStationeryrequests.Cast<RequestBase>())
                        .ToList();
                return View(combinedRequestss);
            }

            await PrepareCommonDataAsync();
            var requests = await ctx.StationeryRequests
                  .Include(a=>a.RequestBy)
                  .Include(a=>a.RequestStatus)
                  .Where(a=>a.RequestBy!.SupperVisorId == user!.Id)
                  .OrderByDescending(a => a.RequestStatus!.Status == "Wait For Approvement")
                  .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();
            var newStationeryrequest = await ctx.NewStationeryRequests
              .Include(a => a.RequestBy)
              .Include(a => a.RequestStatus)
              .Where(a => a.RequestBy!.SupperVisorId == user!.Id)
            .ToListAsync();
            var combinedRequests = requests.Cast<RequestBase>()
                    .Concat(newStationeryrequest.Cast<RequestBase>())
                    .ToList();
            ViewBag.newStationeryrequest = newStationeryrequest;
            return View(combinedRequests);
        }


        public async Task<IActionResult> SubmitedRequest()
        {
            var user = await ctx.Users.SingleOrDefaultAsync(a => a.Email == User.FindFirstValue(ClaimTypes.NameIdentifier));
            await PrepareCommonDataAsync();

            await PrepareCommonDataAsync();
            var requests = await ctx.StationeryRequests
                  .Include(a => a.RequestBy)
                  .Include(a => a.RequestStatus)
                     .Where(a => a.RequestBy!.Id == user!.Id)
                .ToListAsync();
            var newStationeryrequest = await ctx.NewStationeryRequests
              .Include(a => a.RequestBy)
              .Include(a => a.RequestStatus)
              .Where(a=>a.Type != "Restock")
             .Where(a => a.RequestBy!.Id == user!.Id)
            .ToListAsync();
            var combinedRequests = requests.Cast<RequestBase>()
         .Concat(newStationeryrequest.Cast<RequestBase>())
         .ToList();

            return View(combinedRequests);
        }

        public async Task<IActionResult> ApprovedRequest()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewRequestManage"))
            {
                var user = await ctx.Users.SingleOrDefaultAsync(a => a.Email == User.FindFirstValue(ClaimTypes.NameIdentifier));

                await PrepareCommonDataAsync();
                var requests = await ctx.StationeryRequests
                      .Include(a => a.RequestBy)
                      .Include(a => a.RequestStatus)
                     .Where(a => a.RequestStatus!.Status == "Approved" || a.RequestStatus!.Status == "Sending" || a.RequestStatus!.Status == "In Progress" || a.RequestStatus!.Status == "Completed")
                    .ToListAsync();


                var newStationeryrequest = await ctx.NewStationeryRequests
                  .Include(a => a.RequestBy)
                  .Include(a => a.RequestStatus)
                  .Where(a => a.RequestStatus!.Status == "Approved" && a.RequestStatus!.Status != "Sending" && a.RequestStatus!.Status != "In Progress")
                  .ToListAsync();

                var combinedRequests = requests.Cast<RequestBase>()
                        .Concat(newStationeryrequest.Cast<RequestBase>())
                        .ToList();

                return View(combinedRequests);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }
         
        }
        public async Task<IActionResult> RequestDetail(int id)
        {
            await PrepareCommonDataAsync();
            var requests = await request.FindRequestById(id);  
            return View(requests);  
        }
        public async Task<IActionResult> NewRequestDetail(int id)
        {
            await PrepareCommonDataAsync();
            var requests = await request.FindNewRequestById(id);
            return View(requests);
        }

        public async Task<IActionResult> OwnRequestDetail(int id)
        {
            await PrepareCommonDataAsync();
            var requests = await request.FindRequestById(id);
            return View(requests);
        }

        public async Task<IActionResult> ApprovedRequestDetail(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewRequestManage"))
            {
                await PrepareCommonDataAsync();
                var requests = await request.FindRequestById(id);
                return View(requests);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }
        }

        public async Task<IActionResult> reject(int id ,string rejectReason)
        {
            var status = await ctx.RequestStatus.SingleOrDefaultAsync(a => a.Status == "Rejected");
         
            if (id<999990)
            {
                var req = await request.FindRequestById(id);
                var user = await ctx.Users.SingleOrDefaultAsync(a => a.Id == req!.RequestBy!.Id);
                if (req != null && status != null)
                {
                    req.RequestStatusId = status!.Id;
                    req.RejectReason = rejectReason;
                    ctx.Entry(req).State = EntityState.Modified;
                    await ctx.SaveChangesAsync();
                    await employeeService.UpdateAmount(req);

                    var requestItems = await request.ListRequestItems(req.Id);
                    foreach(var i in requestItems)
                    {
                        i.Status = status.Status;
                        ctx.RequestItems.Update(i);
                        await ctx.SaveChangesAsync();
                    }
                
                }
                TempData["message"] = "This Request Status Changed To 'Rejected'";
                return RedirectToAction("RequestDetail", new { id = id });
            }
            else
            {
                var req = await request.FindNewRequestById(id);
               
                if (req != null && status != null)
                {

                    req.RequestStatusId = status!.Id;
                    req.RejectReason= rejectReason;
                    ctx.Entry(req).State = EntityState.Modified;
                    await ctx.SaveChangesAsync();

                    await employeeService.NewUpdateAmount(req);
                }
                TempData["message"] = "This Request Status Changed To 'Rejected'";
                return RedirectToAction("NewRequestDetail", new { id = id });
            }
        }

        public async Task<IActionResult> rejectcancel(int id)
        {
            var status = await ctx.RequestStatus.SingleOrDefaultAsync(a => a.Status == "Approved");

            if (id < 999990)
            {
                var req = await request.FindRequestById(id);
                if (req != null && status != null)
                {
                    req.RequestStatusId = status!.Id;
                    ctx.Entry(req).State = EntityState.Modified;
                    await ctx.SaveChangesAsync();
                }
                TempData["message"] = "This Request Status Changed To 'Approved'";
                return RedirectToAction("RequestDetail", new { id = id });

            }
            else
            {
                var req = await request.FindNewRequestById(id);
                if (req != null && status != null)
                {
                    req.RequestStatusId = status!.Id;
                    ctx.Entry(req).State = EntityState.Modified;
                    await ctx.SaveChangesAsync();
                }
                TempData["message"] = "This Request Status Changed To 'Approved'";
                return RedirectToAction("NewRequestDetail", new { id = id });
            }
        }


        public async Task<IActionResult> approve(int id)
        {
            var status = await ctx.RequestStatus.SingleOrDefaultAsync(a => a.Status == "Approved");

            if (id < 999990)
            {
                var req = await request.FindRequestById(id);
                if (req != null && status != null)
                {
                    req.RequestStatusId = status!.Id;
                    ctx.Entry(req).State = EntityState.Modified;
                    await ctx.SaveChangesAsync();

                    var requestItems = await request.ListRequestItems(req.Id);
                    foreach (var i in requestItems)
                    {
                        i.Status = status.Status;
                        ctx.RequestItems.Update(i);
                        await ctx.SaveChangesAsync();
                    }

                }
                TempData["message"] = "This Request Status Changed To 'Approved'";
                return RedirectToAction("RequestDetail", new { id = id });
            }
            else
            {
                var req = await request.FindNewRequestById(id);
                if (req != null && status != null)
                {
                    req.RequestStatusId = status!.Id;
                    ctx.Entry(req).State = EntityState.Modified;
                    await ctx.SaveChangesAsync();
                }
                TempData["message"] = "This Request Status Changed To 'Approved'";
                return RedirectToAction("NewRequestDetail", new { id = id });

            }
        }

        public async Task<IActionResult> approvecancel(int id)
        {
            var ApproStatus = await ctx.RequestStatus.SingleOrDefaultAsync(a => a.Status == "Approved");
            var status = await ctx.RequestStatus.SingleOrDefaultAsync(a => a.Status == "Canceled/WithDraw");
            var CancelingStatus = await ctx.RequestStatus.SingleOrDefaultAsync(a => a.Status == "Canceling");
            var WaitApproCancelStatus = await ctx.RequestStatus.SingleOrDefaultAsync(a => a.Status == "Canceling! Wait For Approval");
            if (id < 999990)
            {
              
                var req = await request.FindRequestById(id);
                if (req!.RequestStatusId == WaitApproCancelStatus!.Id)
                {
                    req.RequestStatusId = status!.Id;
                    ctx.Entry(req).State = EntityState.Modified;
                    await ctx.SaveChangesAsync();


                    await employeeService.UpdateAmount(req);
                    await ctx.SaveChangesAsync();
                    var requestItems = await request.ListRequestItems(req.Id);
                    foreach (var i in requestItems)
                    {
                        i.Status = status.Status;
                        ctx.RequestItems.Update(i);
                        await ctx.SaveChangesAsync();
                    }

                    TempData["message"] = "This Request Status Changed To 'Canceled/WithDraw'";
                    return RedirectToAction("RequestDetail", new { id = id });
                }
                // if Request status = In progress , Sending , Completed  => change status to sending
                else
                {
                    var requestItems = await request.ListRequestItems(req.Id);
                    await employeeService.UpdateAmount(req);
                    req.RequestStatusId = CancelingStatus!.Id;
                    ctx.StationeryRequests.Update(req);
                    foreach (var i in requestItems)
                    {
                        if(i.Status == "Sending")
                        {
                            i.Status = "Stop Send";
                        }
                        else if (i.Status == "Completed")
                        {
                            i.Status = "Return Stock";
                        }
                        else if (i.Status == "Approved")
                        {
                            i.Status = "Canceling";
                        }
                        ctx.RequestItems.Update(i);
                        await ctx.SaveChangesAsync();
                    }
                    TempData["message"] = "This Request Status Changed To 'Canceling'";
                    return RedirectToAction("RequestDetail", new { id = id });
                }
             
                  
            }
            else
            {
                var req = await request.FindNewRequestById(id);
                if (req != null && status != null)
                {
                    req.RequestStatusId = status!.Id;
                    ctx.Entry(req).State = EntityState.Modified;
                    await ctx.SaveChangesAsync();

                    await employeeService.NewUpdateAmount(req);

                }
                TempData["message"] = "This Request Status Changed To 'Canceled/WithDraw'";
                return RedirectToAction("NewRequestDetail", new { id = id });
            }
  
        }

        public async Task<IActionResult> withdraw(int id)
        {
            var status = await ctx.RequestStatus.SingleOrDefaultAsync(a => a.Status == "Canceled/WithDraw");


            if (id < 999990)
            {
                var req = await request.FindRequestById(id);
            
                    req!.RequestStatusId = status!.Id;
                    ctx.Entry(req).State = EntityState.Modified;
                    await ctx.SaveChangesAsync();
                    await employeeService.UpdateAmount(req);


                var requestItems = await request.ListRequestItems(req.Id);
                foreach (var i in requestItems)
                {
                    i.Status = status.Status;
                    ctx.RequestItems.Update(i);
                    await ctx.SaveChangesAsync();
                }

                return Json(new
                    {
                        success = true
                    });
                
           
            }
            else
            {
                var req = await request.FindNewRequestById(id);
                    req!.RequestStatusId = status!.Id;
                    ctx.Entry(req).State = EntityState.Modified;
                    await ctx.SaveChangesAsync();
                    await employeeService.NewUpdateAmount(req);
                    return Json(new
                    {
                        success = true
                    });
            }
      
        }

        public async Task<IActionResult> cancel(int id)
        {
            var statusApproved = await ctx.RequestStatus.SingleOrDefaultAsync(a => a.Status == "Approved");
            var status = await ctx.RequestStatus.SingleOrDefaultAsync(a => a.Status == "Canceling! Wait For Approval");
            var statusCancelInprogress = await ctx.RequestStatus.SingleOrDefaultAsync(a => a.Status == "Cancel InProgress/Completed Request");
            if (id < 999990)
            {
                var req = await request.FindRequestById(id);
                if (req!.RequestStatusId == statusApproved.Id)
                {
                    req.RequestStatusId = status!.Id;
                    ctx.Entry(req).State = EntityState.Modified;
                    await ctx.SaveChangesAsync();

                }
                else
                {
                    req.RequestStatusId = statusCancelInprogress!.Id;
                    ctx.Entry(req).State = EntityState.Modified;
                    await ctx.SaveChangesAsync();
                }
                return Json(new
                {
                    success = true
                });

            }
            else
            {
                var req = await request.FindNewRequestById(id);


                if (req != null && status != null)
                {
                    req.RequestStatusId = status!.Id;
                    ctx.Entry(req).State = EntityState.Modified;
                    await ctx.SaveChangesAsync();
                }
                return Json(new
                {
                    success = true
                });
            }
   
        }
        public async Task<IActionResult> Sending(int id, string type = "Normal")
        {
            var req = await request.FindRequestById(id);
            var status = await ctx.RequestStatus.SingleOrDefaultAsync(a => a.Status == "Sending");
            var statusCompleted = await ctx.RequestStatus.SingleOrDefaultAsync(a => a.Status == "Completed");
            var statusProgress = await ctx.RequestStatus.SingleOrDefaultAsync(a => a.Status == "In Progress");
            var statusApproved = await ctx.RequestStatus.SingleOrDefaultAsync(a => a.Status == "Approved");
            var statusCanceling = await ctx.RequestStatus.SingleOrDefaultAsync(a => a.Status == "Canceling");
            var statusCanceled = await ctx.RequestStatus.SingleOrDefaultAsync(a => a.Status == "Canceled/WithDraw");

            // if Request for Item not available In Stock
            if (id>999989)
            {
                var newreq = await request.FindNewRequestById(id);

                // if Request Status = In Progress => Sending
                if (newreq!.RequestStatusId == statusProgress!.Id)
                {
                    newreq!.RequestStatusId = status!.Id;
                    ctx.Entry(newreq).State = EntityState.Modified;
                    await ctx.SaveChangesAsync();
                }
                // if Request Status is Sending => Completed
                else if (newreq!.RequestStatusId == status!.Id)
                {
                    newreq!.RequestStatusId = statusCompleted!.Id;
                    ctx.Entry(newreq).State = EntityState.Modified;
                    await ctx.SaveChangesAsync();
                }
                // if Request Status is Approved => In Progress
                else if (newreq!.RequestStatusId == statusApproved!.Id)
                {
                    newreq!.RequestStatusId = statusProgress!.Id;
                    ctx.Entry(newreq).State = EntityState.Modified;
                    await ctx.SaveChangesAsync();
                }
                return Json(new
                {
                    success = true
                });

            }
            // case Request item in Stock
            var items = await ctx.RequestItems
                .Where(a=>a.StationeryRequestId ==req!.Id)
                .Include(a=>a.StationeryItem)
                .ToListAsync();
            // if request status = canceling
            if (req!.RequestStatusId == statusCanceling!.Id)
            {

                // change request item status and , update quantity after back stock item
                foreach (var item in items)
                {
                    if(item.Status == "Return Stock" || item.Status == "Stop Send")
                    {
                
                        item.Status = "Canceled/WithDraw";
                        var stationeryItem = item!.StationeryItem;
                        stationeryItem!.Quantity += item.Quantity;
                        ctx.Entry(stationeryItem).State = EntityState.Modified;
                        ctx.Entry(item).State = EntityState.Modified;
                        await ctx.SaveChangesAsync();
                    }
                    if (item.Status == "Canceling")
                    {
                   
                        item.Status = "Canceled/WithDraw";
                        ctx.Entry(item).State = EntityState.Modified;
                        await ctx.SaveChangesAsync();
                    }
                }
                // change status of Request
                        req.RequestStatusId = statusCanceled!.Id;
                        ctx.StationeryRequests.Update(req);
                        await ctx.SaveChangesAsync();
                    if (type == "ajax")
                    {
                        return Json(new
                        {
                            success = true
                        });
                    }
            }


                // if Request current status is "Sending"
                if (req!.RequestStatusId == status!.Id)
            {
                foreach (var item in items)
                {
                    item.Status = "Completed";
                    ctx.RequestItems.Update(item);
                    await ctx.SaveChangesAsync();
                    // Set Item LastStock-out time = now
                    var stationeryItem = item!.StationeryItem;
                    stationeryItem!.LastStockOut = DateTime.Now;
                    ctx.Entry(stationeryItem).State = EntityState.Modified;
                    await ctx.SaveChangesAsync();
                }
                req.RequestStatusId = statusCompleted!.Id;
                ctx.StationeryRequests.Update(req);
                await ctx.SaveChangesAsync();
                if (type == "ajax")
                {
                    return Json(new
                    {
                        success = true
                    });
                }
            }


            // Case Request Current Status "Approved"
            foreach (var item in items)
            {
            
                var stationeryitem = await ctx.StationeryItems
                    .SingleOrDefaultAsync(a => a.Id == item.StationeryItem!.Id);
                if (stationeryitem!.Quantity == 0 || stationeryitem!.Quantity < item.Quantity)
                {
                    if (type == "ajax")
                    {
                        return Json(new
                        {
                            error = true,
                
                        });
                    }

                }
                item.Status = status.Status;
                ctx.RequestItems.Update(item);
                await ctx.SaveChangesAsync();

                // update quantity in stock
                stationeryitem!.Quantity -= item.Quantity;
                ctx.Entry(stationeryitem).State = EntityState.Modified; 
                await ctx.SaveChangesAsync();
            }
            req!.RequestStatusId = status!.Id;
            await ctx.SaveChangesAsync();
            if (type == "ajax")
            {
                return Json(new
                {
                    success = true
                });
            }
            return View();
        }
        public async Task<IActionResult> SendingItem(int id, string type = "Normal")
        {
          // take status "Sending"
              var status = await ctx.RequestStatus.SingleOrDefaultAsync(a => a.Status == "Sending");
              var statusProgress = await ctx.RequestStatus.SingleOrDefaultAsync(a => a.Status == "In Progress");

            // take request item
            var items = await ctx.RequestItems
                .Include(a => a.StationeryItem)
                .SingleOrDefaultAsync(a=>a.Id == id);
        
            //Set status of curent request item == Sending
                 items!.Status = status!.Status;
                 ctx.Entry(items).State = EntityState.Modified;
                 await ctx.SaveChangesAsync();


            // update stock quantity of Item relate with  requestitem id = id
            var stationeryitem = await ctx.StationeryItems
                .SingleOrDefaultAsync(a => a.Id == items!.StationeryItem!.Id);
 
            stationeryitem!.Quantity -= items!.Quantity;
            ctx.Entry(stationeryitem).State = EntityState.Modified;
            await ctx.SaveChangesAsync();


            // get all requestItem in curent Request
            var request = await ctx.StationeryRequests
                 .Include(a=>a.RequestItems)
                 .SingleOrDefaultAsync(a=>a.Id == items.StationeryRequestId);
             var requestitem = await ctx.RequestItems
                .Where(a => a.StationeryRequestId == request!.Id)
                .ToListAsync();
            // check all request items status "Pending" or not
             int count = 0;
             foreach(var i in requestitem)
            {
                if(i.Status == "Sending")
                {
                    count += 1;
                }
            }
            // if all request items status == Sending -> change status of request == Sending
            if (requestitem.Count== count) 
            {
                request!.RequestStatusId = status.Id;
                ctx.Entry(request).State = EntityState.Modified;
                await ctx.SaveChangesAsync();
            }
            // if not , set Request status = "In Progress"
            else
            {
                request!.RequestStatusId = statusProgress!.Id;
                ctx.Entry(request).State = EntityState.Modified;
                await ctx.SaveChangesAsync();
            }

            if (type == "ajax")
            {
                return Json(new
                {
                    success = true
                });
            }
            return View();
        }

        public async Task<IActionResult> completeItem(int id, string type = "Normal")
        {
            // take status "Completed"
            var status = await ctx.RequestStatus.SingleOrDefaultAsync(a => a.Status == "Completed");

            // take request item
            var items = await ctx.RequestItems
                .Include(a => a.StationeryItem)
                .SingleOrDefaultAsync(a => a.Id == id);

            //Set status of curent Request-item == Completed
            items!.Status = status!.Status;
            ctx.Entry(items).State = EntityState.Modified;
            await ctx.SaveChangesAsync();

            // Set Item LastStock-out time = now
            var stationeryItem = items!.StationeryItem;
            stationeryItem!.LastStockOut = DateTime.Now;
            ctx.Entry(stationeryItem).State = EntityState.Modified;
            await ctx.SaveChangesAsync();

            // Set Status of Request
            // get all request-Item in curent Request
            var request = await ctx.StationeryRequests
              .Include(a => a.RequestItems)
              .SingleOrDefaultAsync(a => a.Id == items.StationeryRequestId);
            var requestitem = await ctx.RequestItems
               .Where(a => a.StationeryRequestId == request!.Id)
               .ToListAsync();
            // check all request items status "Pending" or not
            int count = 0;
            foreach (var i in requestitem)
            {
                if (i.Status == "Completed")
                {
                    count += 1;
                }
            }
            // if all request-items status == Completed -> change status of request == Completed
            if (requestitem.Count == count)
            {
                request!.RequestStatusId = status.Id;
                ctx.Entry(request).State = EntityState.Modified;
                await ctx.SaveChangesAsync();
            }
            if (type == "ajax")
            {
                return Json(new
                {
                    success = true
                });
            }

            return View();
        }


        // New Stationery Request

        [HttpGet]
        public async Task<IActionResult> CreateNewStationeryRequest()
        {
            await PrepareCommonDataAsync();
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "CanRequest"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateNewStationeryRequest(NewStationeryRequestDto dto)
        {
            await PrepareCommonDataAsync();

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "CanRequest"))
            {

                // Validate the model
                var validationContext = new ValidationContext(dto);
                var validationResults = new List<ValidationResult>();

                if (!Validator.TryValidateObject(dto, validationContext, validationResults, true))
                {
                    foreach (var validationResult in validationResults)
                    {
                        // Add validation errors to ModelState
                        ModelState.AddModelError(string.Empty, validationResult!.ErrorMessage!);
                    }

                    return View(dto);
                }

                // Continue processing if model validation succeeds

                var user = await ctx.Users.SingleOrDefaultAsync(a => a.Email == User.FindFirstValue(ClaimTypes.NameIdentifier));
                var roles = User.Claims
                     .Where(c => c.Type == ClaimTypes.Role)
                     .Select(c => c.Value)
                     .ToList();
                bool hasCEORole = roles.Any(r => string.Equals(r, "CEO", StringComparison.OrdinalIgnoreCase));
                var status = await ctx.RequestStatus.SingleOrDefaultAsync(a => a.Status == "Wait For Approvement");
                var statusAppro = await ctx.RequestStatus.SingleOrDefaultAsync(a => a.Status == "Approved");

                if (user != null && status != null)
                {
                    string imageName = await common.UploadImage(dto.Photo!);

                    var newRequest = mapper.Map<NewStationeryRequest>(dto);
                    newRequest.Image = imageName;
                    newRequest.CreatedAt = DateTime.Now;
                    // if user has role Ceo , change sttus to Approved
                    if (hasCEORole)
                    {
                        newRequest.RequestStatusId = statusAppro!.Id;
                    }
                    else
                    {
                        newRequest.RequestStatusId = status.Id;
                    }
                         
                    newRequest.RequestBy = user;

                    ctx.NewStationeryRequests.Add(newRequest);
                    await ctx.SaveChangesAsync();

                    user.AmountRequestPerMonth -= dto.Total;
                    ctx.Users.Update(user);
                    await ctx.SaveChangesAsync();
                    TempData["success"] = "Made Request Successfully!!!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Handle the case when user or status is not found
                    ModelState.AddModelError(string.Empty, "User or status not found.");
                }

                return View(dto);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }



        }


        public async Task<IActionResult> ApprovedNewRequestDetail(int id)
        {
            await PrepareCommonDataAsync();
            var requests = await request.FindNewRequestById(id);
            return View(requests);
        }

        public async Task<IActionResult> OwnNewRequestDetail(int id)
        {
            await PrepareCommonDataAsync();
            var requests = await request.FindNewRequestById(id);
            return View(requests);
        }


        // Request List Ajax

        [HttpGet]
        public IActionResult GetRequestsByType(string type)
        {
            List<RequestBase> filteredRequests;

            if (type == "Approved")
            {
               var req = ctx.StationeryRequests
                    .Include(a => a.RequestBy)
                    .Include(a => a.RequestStatus)
                    .Where(a => a.RequestStatus!.Status == "Approved")
            
                    .ToList();

                var newreq = ctx.NewStationeryRequests
                  .Include(a => a.RequestBy)
                  .Include(a => a.RequestStatus)
                  .Where(a => a.Type != "Restock")
                  .Where(a => a.RequestStatus!.Status == "Approved")
               
                  .ToList();

                 filteredRequests = req.Cast<RequestBase>()
                         .Concat(newreq.Cast<RequestBase>())
                    .OrderByDescending(a => a.CreatedAt)
                         .ToList();

            }
            else if (type == "InProgress")
            {
                var req = ctx.StationeryRequests
              .Include(a => a.RequestBy)
              .Include(a => a.RequestStatus)
              .Where(a => a.RequestStatus!.Status == "In Progress")
              .ToList();

                var newreq = ctx.NewStationeryRequests
                  .Include(a => a.RequestBy)
                  .Include(a => a.RequestStatus)
                   .Where(a => a.Type != "Restock")
                  .Where(a => a.RequestStatus!.Status == "In Progress")
                  .ToList();

                filteredRequests = req.Cast<RequestBase>()
                        .Concat(newreq.Cast<RequestBase>())
                                .OrderByDescending(a => a.CreatedAt)
                        .ToList();
            }
            else if (type == "Sending")
            {
                var req = ctx.StationeryRequests
             .Include(a => a.RequestBy)
             .Include(a => a.RequestStatus)
             .Where(a => a.RequestStatus!.Status == "Sending")
             .ToList();

                var newreq = ctx.NewStationeryRequests
                  .Include(a => a.RequestBy)
                  .Include(a => a.RequestStatus)
                  .Where(a => a.RequestStatus!.Status == "Sending")
                  .ToList();

                filteredRequests = req.Cast<RequestBase>()
                        .Concat(newreq.Cast<RequestBase>())
                      .OrderByDescending(a => a.CreatedAt)
                        .ToList();
            }
            else if (type == "Completed")
            {
                var req = ctx.StationeryRequests
               .Include(a => a.RequestBy)
               .Include(a => a.RequestStatus)
               .Where(a => a.RequestStatus!.Status == "Completed")
               .ToList();

                var newreq = ctx.NewStationeryRequests
                  .Include(a => a.RequestBy)
                  .Include(a => a.RequestStatus)
                      .Where(a => a.Type != "Restock")
                  .Where(a => a.RequestStatus!.Status == "Completed")
                  .ToList();

                filteredRequests = req.Cast<RequestBase>()
                        .Concat(newreq.Cast<RequestBase>())
                       .OrderByDescending(a => a.CreatedAt)
                        .ToList();
            }
            else if (type == "Cancel")
            {
                var req = ctx.StationeryRequests
               .Include(a => a.RequestBy)
               .Include(a => a.RequestStatus)
               .Where(a => a.RequestStatus!.Status == "Canceling")
               .ToList();
                filteredRequests = req.Cast<RequestBase>()
                        .OrderByDescending(a => a.CreatedAt)
                        .ToList();
            }
            else
            {
                // Handle unknown type
                return BadRequest("Invalid request type.");
            }

            return PartialView("ApprovedRequestPartial", filteredRequests);
        }

        [HttpGet]
        public IActionResult GetReceivedRequestsByType(string type)
        {
            List<RequestBase> filteredRequests;

            var user =  ctx.Users.SingleOrDefault(a => a.Email == User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole =  ctx.EmployeeRoles
                .Where(a => a.EmployeeId == user!.Id)
                .Select(a => a.Role!.Name)
                .ToList();
            if (userRole.Contains("CEO"))
            {
              
                if (type == "Other")
                {
                   
                    var req = ctx.StationeryRequests
                         .Include(a => a.RequestBy)
                         .Include(a => a.RequestStatus)
                         .Where(a => a.RequestBy!.Suppervisors!.Email == User.FindFirstValue(ClaimTypes.NameIdentifier) || a.RequestBy.Id == user.Id)
                         .Where(a => a.RequestStatus!.Status != "Wait For Approvement" && a.RequestStatus!.Status != "Canceling! Wait For Approval")
                         .ToList();

                    var newreq = ctx.NewStationeryRequests
                      .Include(a => a.RequestBy)
                      .Include(a => a.RequestStatus)
                      .Where(a=>a.Type !="Restock")
                      .Where(a => a.RequestBy!.Suppervisors!.Email! == User.FindFirstValue(ClaimTypes.NameIdentifier) || a.RequestBy.Id == user.Id)
                      .Where(a => a.RequestStatus!.Status != "Wait For Approvement" && a.RequestStatus!.Status != "Canceling! Wait For Approval")
                      .ToList();

                    filteredRequests = req.Cast<RequestBase>()
                            .Concat(newreq.Cast<RequestBase>())
                               .OrderByDescending(a => a.CreatedAt)
                            .ToList();
                    return PartialView("_ReceivedRequestTablePartial", filteredRequests);
                }
                else if (type == "NeedApproval")
                {
                    var req = ctx.StationeryRequests
                  .Include(a => a.RequestBy)
                  .Include(a => a.RequestStatus)
                  .Where(a => a.RequestBy!.Suppervisors!.Email! == User.FindFirstValue(ClaimTypes.NameIdentifier) || a.RequestBy.Id == user.Id)
                  .Where(a => a.RequestStatus!.Status == "Wait For Approvement" || a.RequestStatus!.Status == "Canceling! Wait For Approval")
                  .ToList();

                    var newreq = ctx.NewStationeryRequests
                      .Include(a => a.RequestBy)
                      .Include(a => a.RequestStatus)
                          .Where(a => a.Type != "Restock")
                      .Where(a => a.RequestBy!.Suppervisors!.Email! == User.FindFirstValue(ClaimTypes.NameIdentifier) || a.RequestBy.Id == user.Id)
                      .Where(a => a.RequestStatus!.Status == "Wait For Approvement" || a.RequestStatus!.Status == "Canceling! Wait For Approval")
                      .ToList();

                    filteredRequests = req.Cast<RequestBase>()
                            .Concat(newreq.Cast<RequestBase>())
                               .OrderByDescending(a => a.CreatedAt)
                            .ToList();
                    return PartialView("_ReceivedRequestTablePartial", filteredRequests);
                }
            }
            else
            {
                if (type == "Other")
                {
                    var req = ctx.StationeryRequests
                         .Include(a => a.RequestBy)
                         .Include(a => a.RequestStatus)
                         .Where(a => a.RequestBy!.Suppervisors!.Email == User.FindFirstValue(ClaimTypes.NameIdentifier))
                         .Where(a => a.RequestStatus!.Status != "Wait For Approvement" && a.RequestStatus!.Status != "Canceling! Wait For Approval")
                         .ToList();

                    var newreq = ctx.NewStationeryRequests
                      .Include(a => a.RequestBy)
                      .Include(a => a.RequestStatus)
                          .Where(a => a.Type != "Restock")
                      .Where(a => a.RequestBy!.Suppervisors!.Email! == User.FindFirstValue(ClaimTypes.NameIdentifier))
                      .Where(a => a.RequestStatus!.Status != "Wait For Approvement" && a.RequestStatus!.Status != "Canceling! Wait For Approval")
                      .ToList();

                    filteredRequests = req.Cast<RequestBase>()
                            .Concat(newreq.Cast<RequestBase>())
                               .OrderByDescending(a => a.CreatedAt)
                            .ToList();

                    return PartialView("_ReceivedRequestTablePartial", filteredRequests);

                }
                else if (type == "NeedApproval")
                {
                    var req = ctx.StationeryRequests
                  .Include(a => a.RequestBy)
                  .Include(a => a.RequestStatus)
                  .Where(a => a.RequestBy!.Suppervisors!.Email! == User.FindFirstValue(ClaimTypes.NameIdentifier))
                  .Where(a => a.RequestStatus!.Status == "Wait For Approvement" || a.RequestStatus!.Status == "Canceling! Wait For Approval")
                  .ToList();

                    var newreq = ctx.NewStationeryRequests
                      .Include(a => a.RequestBy)
                      .Include(a => a.RequestStatus)
                      .Where(a => a.Type != "Restock")
                      .Where(a => a.RequestBy!.Suppervisors!.Email! == User.FindFirstValue(ClaimTypes.NameIdentifier))
                      .Where(a => a.RequestStatus!.Status == "Wait For Approvement" || a.RequestStatus!.Status == "Canceling! Wait For Approval")
                      .ToList();

                    filteredRequests = req.Cast<RequestBase>()
                            .Concat(newreq.Cast<RequestBase>())
                               .OrderByDescending(a => a.CreatedAt)
                            .ToList();
                    return PartialView("_ReceivedRequestTablePartial", filteredRequests);
                }
            }
            return PartialView("_ReceivedRequestTablePartial", new List<RequestBase>());

        }
    }
}
