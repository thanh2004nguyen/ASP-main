using Group5.Atribute;
using System.ComponentModel.DataAnnotations;

namespace Group5.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set;}
        [Required]
	
		public string? Name { get; set; }
    }
}
