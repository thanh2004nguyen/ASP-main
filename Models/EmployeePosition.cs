using Group5.Atribute;
using System.ComponentModel.DataAnnotations;

namespace Group5.Models
{
    public class EmployeePosition
    {
        public int Id { get; set; }
        [Required]
	/*	[UniqueNameAttribute]*/
		public string? Position { get; set; }
		[Required]
		[Range(1, 1000000)]
		public float? MaxAmountPerMonth { get; set; }
		[Required]
		[Range(1,100)]
		public int? Level { get; set; }

    }
}
