using Group5.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Group5.Models.Base
{
    public class RequestBase
    {
        [Key]
        public int Id { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CreatedAt { get; set; }
        [JsonIgnore]
        public Employee? RequestBy { get; set; }
        public float? Total { get; set; }
        public int RequestStatusId { get; set; }
        [ForeignKey(nameof(RequestStatusId))]

        [JsonIgnore]
        public RequestStatus? RequestStatus { get; set; }

        public string? RejectReason { get; set; }    
    }
}
