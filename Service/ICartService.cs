using Group5.Data;
using Group5.Models;

namespace Group5.Service
{
    public interface ICartService
    {
        Task<Cart> CreateCart(Employee user);
        Task<CartItem> CreateCartItem(Cart carts, int id, int quantity);
        Task<bool> DeleteCartItem(int id);
        Task<bool> UpdateCartQuantity(int id, int quantity);
        Task<float> subtotal(int id, int quantity);
        Task<float> total(Employee emp);
        Task EmpltyCart(Cart cart);
        Task<List<CartItem>> ListCartItems(Cart cart);
    }
}
 