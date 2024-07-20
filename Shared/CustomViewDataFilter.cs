using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Group5.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Group5.Models;
using Newtonsoft.Json;

namespace Group5.Shared
{
    public class CustomViewDataFilter: IActionFilter
    {
        ApplicationDbContext ctx;

        public CustomViewDataFilter(ApplicationDbContext ctx)
        {
            this.ctx = ctx;
        }

        public async void OnActionExecuting(ActionExecutingContext context)
        {

            string loginEmail = context.HttpContext.Session.GetString("loginemail");
         
            if (context.Controller is Controller controller)
            {
                controller.ViewData["allviewEmail"] = loginEmail;
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Do nothing on action executed
        }
    }
}
