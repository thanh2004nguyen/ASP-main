using AutoMapper;
using Group5.Data;
using Group5.Service;
using Microsoft.AspNetCore.Mvc;

namespace Group5.Controllers
{
    public class DetailsEmployeeController : BaseController
    {
        public DetailsEmployeeController(ApplicationDbContext ctx, IMapper mapper, ICartService service, IRequestService request, ICustomRoleService customRoleService) : base(ctx, mapper, service, request, customRoleService)
        {
        }

        public async Task<IActionResult> IndexAsync()
        {
            await PrepareCommonDataAsync();
            return View();
        }
    }
}
