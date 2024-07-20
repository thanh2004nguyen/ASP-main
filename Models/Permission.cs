using System.ComponentModel.DataAnnotations;

namespace Group5.Models
{
    public class Permission
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }

        public ICollection<RolePermission>? RolePermissions { get; set; }
    }
}
