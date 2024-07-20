using Group5.Data;
using Group5.Models.Base;
using RestSharp;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Group5.Models
{
    public class StationeryRequest: RequestBase
    {
        [JsonIgnore]
        public virtual ICollection<RequestItem>? RequestItems { get; set;}
    
    }
}
