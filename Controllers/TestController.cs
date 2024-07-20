using AutoMapper;
using Group5.Data;
using Group5.Models;
using Group5.Service;
using Group5.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;


namespace Group5.Controllers
{
    [AllowAnonymous]
    public class TestController : Controller
    {
        public IWebHostEnvironment env;
        internal ApplicationDbContext ctx;
        internal IMapper mapper;
        internal ICartService service;
        internal IRequestService request;
        internal ICustomRoleService customRoleService;

        public TestController(ApplicationDbContext ctx, IMapper mapper, ICartService service, IRequestService request, ICustomRoleService customRoleService, IWebHostEnvironment env)
        {
            this.ctx = ctx;
            this.mapper = mapper;
            this.service = service;
            this.request = request;
            this.customRoleService = customRoleService;
            this.env = env;
        }

        public async Task PrepareCommonDataAsync()
        {

            var loginEmail = "";
            if (User?.Identity?.IsAuthenticated == true)
            {
                loginEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            var user = await ctx.Users
                .Include(a => a.Departments)
                .Include(a => a.EmployeePositions)
                .FirstOrDefaultAsync(a => a.Email == loginEmail);

            HttpContext.Session.SetInt32("UserId", user!.Id);

            var cart = await service.CreateCart(user!);

            var cartItems = await ctx.CartItems
                .Include(a => a.StationeryItem)
                .Where(a => a.CartId == cart.Id)
                .ToListAsync();

            var total = cart.CartItems!.Sum(ci => ci.Quantity * ci.StationeryItem!.Price);

            var message = TempData["message"] as string;
            var error = TempData["error"] as string;

            ViewBag.Message = message;
            ViewBag.Error = error;


            // get Permision of Login USer
            var permissions = await ctx.EmployeeRoles!
                  .Include(a => a.Role!)
                  .ThenInclude(a => a.RolePermissions!)
                    .ThenInclude(a => a.Permission)
                  .Where(a => a.EmployeeId == user.Id)
                  .ToListAsync();

            List<string> userPermissions = new List<string>();

            foreach (var p in permissions)
            {
                var rolePermissions = p.Role!.RolePermissions;

                foreach (var rolePermission in rolePermissions!)
                {
                    var permission = rolePermission!.Permission!.Name;
                    userPermissions.Add(permission!);
                }
            }


            ViewData["roles"] = userPermissions;
            ViewData["total"] = total;
            ViewData["user"] = user;
            ViewData["loginCart"] = cart;
            ViewBag.loginCartItem = cartItems;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTest([FromBody] CreateEmployee emp)
        {
          
           //  return Ok(ListRoles);
            var existingUser = ctx.Users.FirstOrDefault(u => u.Email == emp.Email);

            if (existingUser != null)
            {
                // Nếu đã tồn tại, báo lỗi và chuyển về trang Create
                TempData["error"] = "Email đã tồn tại!";
                return RedirectToAction("Create");
            }

            if (emp.Password != emp.ConfirmPassword)
            {
                TempData["error"] = "Passwords do not match!";
                return RedirectToAction("Create");
            }
            string fileName = "avatar.png";

            if (emp.Photo != null && emp.Photo.Length > 0)
            {
                // Tạo đường dẫn tới thư mục lưu trữ hình ảnh
                var imagesFolder = Path.Combine(env.WebRootPath, "images");
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                // Tạo tên file hình ảnh (vd: guid.jpg)
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(emp.Photo.FileName);
                string filePath = Path.Combine(imagesFolder, fileName);

                // Lưu file hình ảnh
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await emp.Photo.CopyToAsync(fileStream);
                }

                // Lưu đường dẫn hình ảnh vào thuộc tính ImageUrl
            }
            var newPassword = BCrypt.Net.BCrypt.HashPassword(emp.Password);
            var employeeView = mapper.Map<Employee>(emp);
            employeeView.Avatar = fileName;
            employeeView.Password = newPassword;
            // add Request Amount 
            var inputposition = await ctx.EmployeePositions.SingleOrDefaultAsync(ctx => ctx.Id == employeeView.EmployeePositionId);
            employeeView.AmountRequestPerMonth = inputposition!.MaxAmountPerMonth;
            employeeView.ResetAmountTime = DateTime.Now;

            ctx.Users.Add(employeeView);
            await ctx.SaveChangesAsync();
            /*     foreach (var i in ListRoles)
                 {
                     EmployeeRole employeeRole = new EmployeeRole()
                     {
                         RoleId = i,
                         EmployeeId = employeeView.Id
                     };
                     ctx.EmployeeRoles.Add(employeeRole);
                 }*/
              return Ok(employeeView);
        }



        [HttpPost]
        public async Task<IActionResult> CreateRequet([FromBody]RequestTest request)
        {
         
            var user = await ctx.Users
              .SingleOrDefaultAsync(a => a.Email == request!.RequestBy!.Email);
             
            var cart = await service.CreateCart(user!);
            var allproduct = await ctx.StationeryItems.ToListAsync();
          
            // simulate each user buy radom 3 stationery items
            foreach (var product in GetRandomProducts(allproduct, 1))
            {
                CartItem cartItem = new CartItem()
                {
                    CartId = cart.Id,
                    StationeryItem = product,
                    Quantity = 2,
                    CreatedDate = DateTime.Now, 
                };
                ctx.CartItems.Add(cartItem);   
                await ctx.SaveChangesAsync();
            }
       
            var cartItems = await service.ListCartItems(cart);

            var total = cart.CartItems!.Sum(ci => ci.Quantity * ci.StationeryItem!.Price);
                if (total == 0)
                {
                    TempData["message"] = "Request List Empty!!! Please Try Again";
                    return RedirectToAction("Index", "Home");
                }
                StationeryRequest stationeryRequest = new StationeryRequest()
                {
                    CreatedAt = DateTime.Now,
                    RequestBy = user,
                    Total = total,
                    RequestStatusId = 1

                };
                ctx.StationeryRequests.Add(stationeryRequest);
                await ctx.SaveChangesAsync();
          
            user!.AmountRequestPerMonth -= total;
                ctx.Users.Update(user);
                await ctx.SaveChangesAsync();

                foreach (var item in cartItems)
                {
                    RequestItem items = new RequestItem()
                    {
                        StationeryRequestId = stationeryRequest.Id,
                        StationeryItem = item.StationeryItem,
                        Quantity = item.Quantity,
                        CreatedDate = DateTime.Now,
                        Status = "Wait For Approvement"
                    };

                    ctx.RequestItems.Add(items);
                    await ctx.SaveChangesAsync();
                }
                //empty cart
                await service.EmpltyCart(cart);
                return Ok(stationeryRequest);

                HttpContext.Session.SetInt32("cartitems", 0);
                TempData["success"] = "Made Request Successfully!!!";
                return RedirectToAction("Index", "Home");

        }


        private List<StationeryItem> GetRandomProducts(List<StationeryItem> allProducts, int numberOfProductsToAdd)
        {
            var random = new Random();
            var selectedProducts = new List<StationeryItem>();

            for (int i = 0; i < numberOfProductsToAdd; i++)
            {
                var randomIndex = random.Next(allProducts.Count);
                selectedProducts.Add(allProducts[randomIndex]);
            }

            return selectedProducts;
        }


    }
    }

 
    

