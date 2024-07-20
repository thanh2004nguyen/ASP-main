using Group5.Data;
using Group5.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Group5.Service
{
    public class CartService: ICartService  
    {
        ApplicationDbContext ctx;

        public CartService(ApplicationDbContext ctx)
        {
            this.ctx = ctx;
        }

        public async Task<Cart> CreateCart(Employee user)
        {
          
            var exitscart = await ctx.Carts.SingleOrDefaultAsync(a=>a.RequestBy!.Id == user.Id);
            if (exitscart==null)
            {
                Cart cart = new Cart()
                {
                    CreatedAt = DateTime.Now,
                    RequestBy = user,
                 
                };
                ctx.Carts.Add(cart);
                await ctx.SaveChangesAsync();
                var carttt = await ctx.Carts
                    .Include(a=>a.CartItems)
                    .SingleOrDefaultAsync(a => a.Id == cart.Id);
                return cart;
            }
            else
            {
                var carttt = await ctx.Carts
                  .Include(a => a.CartItems)
                  .SingleOrDefaultAsync(a => a.Id == exitscart.Id);
                return carttt;
            }
            
        }

        public async Task<CartItem> CreateCartItem(Cart carts,int id,int quantity)
        {
            var sta = await ctx.StationeryItems.SingleOrDefaultAsync(a => a.Id == id);
            var exitscartitem = await ctx.CartItems
                .Include(a=>a.StationeryItem)
                .Where(a=>a.CartId==carts.Id)
                .SingleOrDefaultAsync(a => a.StationeryItem!.Id== id);
            if (sta!=null)
            {
                if(exitscartitem!=null) 
                {
                    exitscartitem.Quantity+= quantity;
                    ctx.Entry(exitscartitem).State = EntityState.Modified;
                    await ctx.SaveChangesAsync();

                    var cartItem = await ctx.CartItems
                   .Include(a => a.StationeryItem)
                   .SingleOrDefaultAsync(a => a.Id == exitscartitem.Id);
                    return cartItem!;
                }
                else
                {
                    CartItem cartI = new CartItem()
                    {
                        CartId = carts.Id,
                        StationeryItem = sta,
                        Quantity = quantity,
                        CreatedDate = DateTime.Now,
                       
                    };
                    ctx.CartItems.Add(cartI);
                    await ctx.SaveChangesAsync();
                    var cartItem = await ctx.CartItems
                        .Include (a=>a.StationeryItem)  
                        .SingleOrDefaultAsync(a => a.Id == cartI.Id);
                    return cartItem!;
                } 
            }
            return null!;
        }
        public async  Task<bool> DeleteCartItem(int id)
        {
            var cartitem = await ctx.CartItems.SingleOrDefaultAsync(a=>a.Id==id);
            if (cartitem != null) 
            {
              ctx.CartItems.Remove(cartitem);
                return (await ctx.SaveChangesAsync() > 0);
            }
            else
            { return false; }
        }

        public async Task<bool> UpdateCartQuantity(int id,int quantity)
        {
            var cartitem = await ctx.CartItems.SingleOrDefaultAsync(a => a.Id == id);
            if (cartitem != null)
            {
                cartitem.Quantity = quantity;
                ctx.CartItems.Update(cartitem);
                return (await ctx.SaveChangesAsync() > 0);
            }
            else
            { return false; }
        }

        public async Task<float> subtotal(int id,int quantity)
        {
            var cartitem = await ctx.CartItems
                .Include(a => a.StationeryItem)
                .SingleOrDefaultAsync(a=>a.Id==id);

            var cart = await ctx.Carts
                  .Include(c => c.CartItems)
                  .SingleOrDefaultAsync(c => c.Id == cartitem.CartId);
            var price = cartitem.StationeryItem.Price;
            return (float)price * quantity;
        }

        public async Task<float> total(Employee emp)
        {
            var cart = await ctx.Carts
                  .Include(c => c.CartItems)
                  .SingleOrDefaultAsync(c => c.RequestBy.Id == emp.Id);

            var cartItems = await ctx.CartItems
                    .Include(a => a.StationeryItem)
                    .Where(a => a.CartId == cart.Id)
                    .ToListAsync();
            return (float)cart.CartItems.Sum(ci => ci.Quantity * ci.StationeryItem.Price);
       
        }

        public async Task<List<CartItem>> ListCartItems(Cart cart)
        {
            return await ctx.CartItems
                .Where(a=>a.CartId == cart.Id)
                .Include(a=>a.StationeryItem)
                .ToListAsync();
        }

        public async Task EmpltyCart(Cart cart)
        {
            var listCartItem = await ctx.CartItems
                .Where(a => a.CartId == cart.Id)
                .Include(a => a.StationeryItem)
                .ToListAsync();
            if(listCartItem != null && listCartItem.Count !=0)
            {
                foreach (var item in listCartItem)
                {
                    ctx.CartItems.Remove(item);
                    await ctx.SaveChangesAsync();
                }
            }
    
        }



    }
}
