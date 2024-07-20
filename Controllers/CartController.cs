using AutoMapper;
using Group5.Data;
using Group5.Models;
using Group5.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Security.Claims;

namespace Group5.Controllers
{
    public class CartController : BaseController
    {
        public CartController(ApplicationDbContext ctx, IMapper mapper, ICartService service, IRequestService request, ICustomRoleService customRoleService) : base(ctx, mapper, service, request, customRoleService)
        {
        }

        public async Task<IActionResult> Index()
        {
            await PrepareCommonDataAsync();

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "CanRequest"))
            {
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
                var total = cart.CartItems!.Sum(ci => ci.Quantity * ci.StationeryItem!.Price);
                ViewData["total"] = total;
                ViewData["user"] = user;
                ViewData["loginCart"] = cart;
                ViewBag.loginCartItem = cartItem;
                return View(cartItem);

            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }


           
        }
        public async Task<IActionResult> CreateCartItem(int id,int quantity,string type="Normal")
        { 
            var LoginEmail = "";
            if (User!.Identity!.IsAuthenticated)
            {
                LoginEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            var user = await ctx.Users.FirstOrDefaultAsync(a => a.Email == LoginEmail);
            var cart = await service.CreateCart(user!);
            var cartItems = await service.CreateCartItem(cart, id, quantity);
            var items = cart.CartItems!.Count();
            HttpContext.Session.SetInt32("cartitems", items);

            if (type=="ajax")
            {
                return Json(new
                {
                    items = cart.CartItems!.Count(),
                    newItemImage = cartItems.StationeryItem!.ImageUrl,
                    newItemPrice = cartItems.StationeryItem.Price,
                    newItemName = cartItems.StationeryItem.Name,
                    newItemId = cartItems.Id,
                    newItemQuantity = cartItems.Quantity,
                    total = await service.total(user!)
                }) ;
            }

            return RedirectToAction("Index", "Stationery");
        }

        public async Task<IActionResult> DeleteCartItem(int id)
        {
            var LoginEmail = "";
            if (User!.Identity!.IsAuthenticated)
            {
                LoginEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            var user = await ctx.Users.FirstOrDefaultAsync(a => a.Email == LoginEmail);
            await service.DeleteCartItem(id);
            var cart = await service.CreateCart(user!);
            var items = cart.CartItems!.Count();
            HttpContext.Session.SetInt32("cartitems", items);
            return Json(new 
            {
                success = true ,
                items = cart.CartItems!.Count(),
                total = await service.total(user!)    
            });
        }

        public async Task<IActionResult> UpdateCartQuantity(int id,int quantity)
        {
             await service.UpdateCartQuantity(id, quantity);

            var LoginEmail = "";
            if (User!.Identity!.IsAuthenticated)
            {
                LoginEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            var user = await ctx.Users.FirstOrDefaultAsync(a => a.Email == LoginEmail);


            return Json(new 
            { 
                success = true,
                subtotal = await service.subtotal(id, quantity),
                quantity =quantity,
                total = await service.total(user!)
            });
        }
    }
}
