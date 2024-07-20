using Group5.Data;
using Group5.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Group5.ViewModels
{
    public class RequestTest
    {
   
        public int Id { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Employee? RequestBy { get; set; }
        public float? Total { get; set; }
        public int RequestStatusId { get; set; }
        [ForeignKey(nameof(RequestStatusId))]
        public RequestStatus? RequestStatus { get; set; }

        public virtual ICollection<RequestItem>? RequestItems { get; set; }
    }
}
