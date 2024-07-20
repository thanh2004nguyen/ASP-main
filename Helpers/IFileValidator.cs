using Microsoft.AspNetCore.Http;

namespace Group5.Helpers
{
    public interface IFileValidator
    {
        bool IsValid(IFormFile file);
    }
}
