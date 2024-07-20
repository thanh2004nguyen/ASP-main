using System.ComponentModel.DataAnnotations;

namespace Group5.Models
{
    public class RequestStatus
    {
        [Key]
        public int Id { get; set; } 
        public string? Status { get; set; }
 
        public ICollection<RequestItem>? RequestItems { get; }
        public ICollection<NewStationeryRequest>? NewStationeryRequests { get; }

    }
}
