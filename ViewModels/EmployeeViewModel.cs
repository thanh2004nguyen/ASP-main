using Group5.Data;
using Group5.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Group5.ViewModels
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public float? AmountRequestPerMonth { get; set; }
        public string? Avatar { get; set; }
        public string? Role { get; set; }
        public Role? Roletable { get; set; }
        public int DepartmentId { get; set; }
        public virtual Department? Departments { get; set; }
        public int EmployeePositionId { get; set; }

        [ForeignKey("EmployeePositionId")]
        public virtual EmployeePosition? EmployeePositions { get; set; }

        public ICollection<EmployeeRole>? EmployeeRoles { get; set; }

        public IFormFile? Photo { get; set; }

        public int? SupperVisorId { get; set; }
        [ForeignKey("SupperVisorId")]
        public virtual Employee? Suppervisors { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public DateTime? ResetAmountTime { get; set; }

    }
}
