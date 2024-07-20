using System.ComponentModel.DataAnnotations.Schema;

namespace Group5.Models
{
    public class CartItem
    {
        public int Id { get; set; } 
        public int CartId { get; set; }
        [ForeignKey("CartId")]
        public virtual Cart? Cart { get; set; }   
        public int StationeryItemId { get; set; }
        [ForeignKey("StationeryItemId")]
        public virtual StationeryItem? StationeryItem { get; set; }
        public int Quantity { get; set; }   
        public DateTime? CreatedDate { get; set; }  


    }
}
