using Group5.Atribute;
using Group5.Data;
using Group5.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Group5.ViewModels
{
    public class NewStationeryRequestDto
    {
        public int Id { get; set; }
        [Required]
        public string? ItemName { get; set; }
        public string? Description { get; set; }
        [Required]
        [Range(1, 1000)]
        public int? Quantity { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int? Price { get; set; }
        public string? Image { get; set; }
        [TotalLessThanRequestAmount("Amount")]
        public float Total { get; set; }
        public DateTime? RequestTime { get; set; }

        public string? Amount { get; set; }

        public Employee? RequestBy { get; set; }
        public int RequestStatusId { get; set; }
        [ForeignKey(nameof(RequestStatusId))]
        public RequestStatus? RequestStatus { get; set; }
      
        public IFormFile? Photo { get; set; }

    }
}
