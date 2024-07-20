using Group5.Data;
using Group5.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Group5.ViewModels
{
    public class CreateEmployee
    {
        public int Id { get; set; }
        [Required]
        public string? FullName { get; set; }
        [Required]
       [EmailAddress]
        public string? Email { get; set; }
        public float? AmountRequestPerMonth { get; set; }
        public string? Avatar { get; set; }
       [Required]
        public int DepartmentId { get; set; }
        public virtual Department? Departments { get; set; }
       [Required]
        public int EmployeePositionId { get; set; }

        [ForeignKey("EmployeePositionId")]
        [JsonIgnore]
        public virtual EmployeePosition? EmployeePositions { get; set; }

        [JsonIgnore]
        public ICollection<EmployeeRole>? EmployeeRoles { get; set; }
       
        public IFormFile? Photo { get; set; }

       
        public int? SupperVisorId { get; set; }
        [ForeignKey("SupperVisorId")]
        [JsonIgnore]
        public virtual Employee? Suppervisors { get; set; }

        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public DateTime? ResetAmountTime { get; set; }

    }
}
