using AutoMapper;
using Group5.Data;
using Group5.Models;
using Group5.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Group5.Controllers
{
    public class BaseController : Controller
    {
        internal ApplicationDbContext ctx;
        internal IMapper mapper;
        internal ICartService service;
        internal IRequestService request;
        internal ICustomRoleService customRoleService;

        public BaseController(ApplicationDbContext ctx, IMapper mapper, ICartService service, IRequestService request, ICustomRoleService customRoleService)
        {
            this.ctx = ctx;
            this.mapper = mapper;
            this.service = service;
            this.request = request;
            this.customRoleService = customRoleService;
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
                  .Include(a=>a.Role!)
                  .ThenInclude(a=>a.RolePermissions!)
                    .ThenInclude(a=>a.Permission)
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
    }
}
