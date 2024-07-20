using AutoMapper.Execution;
using System.ComponentModel.DataAnnotations;

namespace Group5.ViewModels
{
    public class ConfirmPassword
    {
        [Required(ErrorMessage = "Current password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        [Key]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

     
    }
}