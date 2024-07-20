using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Group5.Data;
using Group5.ViewModels;

namespace Group5.Atribute
{
    public class TotalLessThanRequestAmountAttribute : ValidationAttribute
    {
        private readonly string _requestAmountPropertyName;
       

        public TotalLessThanRequestAmountAttribute(string requestAmountPropertyName)
        {

            _requestAmountPropertyName = requestAmountPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            

            var AmountPro = validationContext.ObjectType.GetProperty(_requestAmountPropertyName);
            var amount = AmountPro.GetValue(validationContext.ObjectInstance);
            int.TryParse(amount.ToString(), out int amountValue);
            var total = (float)value;
            if (total <= amountValue)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult($"{validationContext.DisplayName} must be less than or equal to {_requestAmountPropertyName}.");
        }
    }
}
