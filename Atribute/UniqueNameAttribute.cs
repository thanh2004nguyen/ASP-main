
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Group5.Data;
using Group5.Models;

namespace Group5.Atribute;
[AttributeUsage(AttributeTargets.Property)]
public class UniqueNameAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        

        var dbContext = (ApplicationDbContext)validationContext.GetService(typeof(ApplicationDbContext));

            var existingCate = dbContext!.Set<Category>()
            .FirstOrDefault(r => r.Name!.ToLower() == (string)value);

            var existingBrand = dbContext.Set<Brand>()
             .FirstOrDefault(r => r.Name!.ToLower() == (string)value);

            var existingRole = dbContext.Set<Role>()
             .FirstOrDefault(r => r.Name!.ToLower() == (string)value);

      
            var existingPosition = dbContext.Set<EmployeePosition>()
           .FirstOrDefault(r => r.Position!.ToLower() == (string)value);

            var existingDepartment = dbContext.Set<Department>()
                .FirstOrDefault(r => r.Name!.ToLower() == (string)value);


        if (existingCate != null)
        {
            return new ValidationResult("Category Name Already Have!");
        }
        else if (existingBrand != null)
        {
            return new ValidationResult("This Brand Alrady Have!");
        }
        else if (existingRole != null)
        {
            return new ValidationResult("This Role Already Have!");
        }
  
        else if (existingPosition != null)
        {
            return new ValidationResult("This Position Already Have!");
        }
        else if (existingDepartment != null)
        {
            return new ValidationResult("This Department Name Alrady Have!");
        }

        return ValidationResult.Success;
    }
}
