using Group5.Atribute;
using System.ComponentModel.DataAnnotations;

namespace Group5.Models
{
    public class UseLessItem
    {
        public int Id { get; set; }
		[Required]
		[UniqueUseLessLevelAttribute]
		public int StockLevel { get; set; }
		[Required]
		[Range(1, 2000)]
		public int MaxTime { get; set; }    
    }
}
