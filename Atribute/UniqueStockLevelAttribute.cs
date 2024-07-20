
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Group5.Data;
using Group5.Models;

namespace Group5.Atribute;
[AttributeUsage(AttributeTargets.Property)]
public class UniqueStockLevelAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        

        var dbContext = (ApplicationDbContext)validationContext.GetService(typeof(ApplicationDbContext));

        if (int.TryParse(value.ToString(), out var stockLevel))
        {
            var existingUserLess = dbContext!.Set<StockLevel>().FirstOrDefault(r => r.Level == stockLevel);
            if (existingUserLess != null) 
            {
                return new ValidationResult("This Stock Level Already Have!");
            }
        }
        return ValidationResult.Success;
    }
}
