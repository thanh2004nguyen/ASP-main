using Group5.Atribute;
using System.ComponentModel.DataAnnotations;

namespace Group5.Models
{
    public class StockLevel
    {
        public int Id { get; set; }
        [Required]
        [Range(0, 1000)]    
        public int MinQuantity { get; set; }
        [Required]
        [Range(1, 100)]

       [UniqueStockLevelAttribute]
        public int Level { get; set; }    
    }
}
