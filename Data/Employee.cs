using Group5.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Group5.Data
{
    public class Employee 
    {
        [Key]
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Avatar { get; set; }
        public string? Password { get; set; }
        public float? AmountRequestPerMonth { get; set; }   
        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public virtual Department? Departments { get; set; }
        public int? EmployeePositionId { get; set; }
        [ForeignKey("EmployeePositionId")]
        public virtual EmployeePosition? EmployeePositions { get; set; }

        public int? SupperVisorId { get; set; }
        [ForeignKey("SupperVisorId")]
        public virtual Employee? Suppervisors { get; set; }
        [JsonIgnore]
        public ICollection<EmployeeRole>? EmployeeRoles { get; set; }
        [JsonIgnore]
        public ICollection<Room>? Rooms { get; set; }
        [JsonIgnore]
        public ICollection<Message>? Messages { get; set; }
        [JsonIgnore]
        public ICollection<Cart>? Carts { get; set; }

        public DateTime? ResetAmountTime { get; set; }

    }
}

