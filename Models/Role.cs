using Group5.Atribute;
using Group5.Data;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Group5.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        [Required]

		public string? Name { get; set; }

        public ICollection<RolePermission>? RolePermissions { get; set; }
        public ICollection<EmployeeRole>? EmployeeRoles { get; set; }

    }
}
