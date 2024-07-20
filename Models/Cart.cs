using Group5.Data;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Group5.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Employee? RequestBy { get; set; }
        [JsonIgnore]
        public virtual ICollection<CartItem>? CartItems { get; set; }

    }
}
