using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Group5.Models
{
    public class StationeryItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public float? Price { get; set; }   
        public string? ImageUrl { get; set; }
        public int? Quantity { get; set; }
        public string? TypeOfQuantity { get; set; }
     
        public int? CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        [JsonIgnore]
        public virtual Category? Categories { get; set; }
        public int? BrandId { get; set; }
        [ForeignKey("BrandId")]
        [JsonIgnore]
        public virtual Brand? Brand { get; set; }
        public int? StockLvId { get; set; }
        [ForeignKey("StockLvId")]
        public StockLevel? StockLevel { get; set; }
        public int? UseLessId { get; set; }
        [ForeignKey("UseLessId")]
        public UseLessItem? UseLessItem { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CreatedAt { get; set; }

        public DateTime? LastStockOut { get; set; }
        public string? Status { get; set; }

        public string? StatusRestock { get; set; }

        public bool? isCheckUseLess { get; set; }

        public DateTime? CheckForUseLessTime { get; set; }

        private bool? _isHide;

        public Boolean? IsHide
        {
            get => _isHide ?? false;
            set => _isHide = value;
        }

    }
}
