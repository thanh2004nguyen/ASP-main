using Group5.Atribute;
using System.ComponentModel.DataAnnotations;

namespace Group5.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]  
        [UniqueNameAttribute]
        public string? Name { get; set; }    
    }
}
