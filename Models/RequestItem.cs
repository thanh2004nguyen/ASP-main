using System.ComponentModel.DataAnnotations.Schema;

namespace Group5.Models
{
    public class RequestItem
    {

        public int Id { get; set; }
        public int StationeryRequestId { get; set; }
        [ForeignKey("StationeryRequestId")]
        public virtual StationeryRequest? Stationery { get; set; }
        public virtual StationeryItem? StationeryItem { get; set; }
        public int Quantity { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? Status { get; set; }
 
    }
}
