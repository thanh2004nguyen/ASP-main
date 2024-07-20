using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Group5.ViewModels
{
    public class UploadViewModel
    {
        [Required]
        public int RoomId { get; set; }
        [Required]
        public IFormFile? File { get; set; }
    }
}
