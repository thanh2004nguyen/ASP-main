using Group5.Atribute;
using Group5.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Group5.ViewModels
{
    public class StationeryItemViewModel
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]

        public string? Name { get; set; }
        [Required]
        [StringLength(200)]
        public string? Description { get; set; }
        [Required]
        [Range(0, 200, ErrorMessage = ("Price must be greater than 0 and cannot reachout 200"))]
        public float? Price { get; set; }
        public string? ImageUrl { get; set; }

        public IFormFile? Photo { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be 0 or greater.")]
        public int? Quantity { get; set; }
        [Required]
        [StringLength(20)]
        public string? TypeOfQuantity { get; set; }
        public int? CategoryId { get; set; }
        [JsonIgnore]
        public virtual Category? Categories { get; set; }
        public int? BrandId { get; set; }
        [JsonIgnore]
        public virtual Brand? Brand { get; set; }
        public int? StockLvId { get; set; }
        [ForeignKey("StockLvId")]
        public StockLevel? StockLevel { get; set; }
        public int? UseLessId { get; set; }
        [ForeignKey("UseLessId")]
        public UseLessItem? UseLessItem { get; set; }

        public string? Status { get; set; }
        public string? StatusRestock { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CreatedAt { get; set; }
    }
}