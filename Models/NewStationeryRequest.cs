using Group5.Data;
using Group5.Models.Base;
using Microsoft.EntityFrameworkCore.Storage;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Group5.Models
{
    public class NewStationeryRequest : RequestBase
    {

        public string? ItemName { get; set; }
        public string? Description { get; set; }
        public int? Quantity { get; set; }
        public int? Price { get; set; }
        public string? Image { get; set; }

        public virtual StationeryItem? StationeryItem { get; set; }
    
        public string? Type { get; set; }

}
}