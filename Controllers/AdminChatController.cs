using Group5.Data;
using Group5.Controllers;
using Group5.Models;
using Group5.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Net.Http;
using AutoMapper;
using Group5.Service;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Group5.Controllers
{
    /* [Authorize]*/

 
    public class AdminChatController : BaseController
    {
        private readonly HttpClient client;
        string ApiPath = "http://localhost:5240/api/Rooms/list";

        public AdminChatController(ApplicationDbContext ctx, IMapper mapper, ICartService service, IRequestService request, HttpClient client, ICustomRoleService customRoleService) : base(ctx, mapper, service, request, customRoleService)
        {
            this.client = client;
        }

        public async Task<IActionResult> Index()
        {
            await PrepareCommonDataAsync();
            string url = ApiPath;
            string json = await client.GetStringAsync(url);
            var  r = JsonConvert.DeserializeObject<List<RoomViewModel>>(json);
             var mes = await ctx.Messages
                .Where(a=>a.Seen==false)
                .ToListAsync();

            var viewModel = new IndexViewModel
            { 
                Rooms = r,
                UnseenMessages = mes,
               
            };
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewAdminChat"))
            {
                return View(viewModel);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }
        }

        public async Task<IActionResult> CheckUnseenMessages()
        {
            await PrepareCommonDataAsync();
            string url = ApiPath;
            string json = await client.GetStringAsync(url);
            var r = JsonConvert.DeserializeObject<List<RoomViewModel>>(json);
            var mes = await ctx.Messages
               .Where(a => a.Seen == false)
               .ToListAsync();

            var viewModel = new IndexViewModel
            {
                Rooms = r,
                UnseenMessages = mes,
            };

            // Return the JSON data along with the partial view HTML
            return Json(viewModel);
        }
    }
}
