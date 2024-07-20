using AutoMapper;
using Group5.Data;
using Group5.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Group5.ViewModels;
using Group5.Models;
using System.Data;

namespace Group5.Controllers

{
    public class EmployeeRolesController : BaseController
    {
        public EmployeeRolesController(ApplicationDbContext ctx, IMapper mapper, ICartService service, IRequestService request, ICustomRoleService customRoleService) : base(ctx, mapper, service, request, customRoleService)
        {
        }
        public async Task<IActionResult> Index()
        {

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewEmployee"))
            {
                await PrepareCommonDataAsync();
                var employeeRoles = await ctx.Roles
                    .Include(x => x!.EmployeeRoles!)
                    .Include(x => x!.RolePermissions!)
                      .ThenInclude(a => a.Permission)
                    .ToListAsync();

                return View(employeeRoles);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }


     

        }
        public async Task<IActionResult> Create()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewEmployee"))
            {
                await PrepareCommonDataAsync();
                var permision = await ctx.Permissions.ToListAsync();
                ViewBag.PermisionName = new SelectList(permision, "Id", "Name");
                return View();
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }


     
        }
        [HttpPost]
        public async Task<IActionResult> Create(Role role,List<int> ListPermision)
        {
            if(ModelState.IsValid)
            {
				ctx.Roles.Add(role);
				await ctx.SaveChangesAsync();

				foreach (var p in ListPermision)
				{
					RolePermission rolePermission = new RolePermission()
					{
						PermissionId = p,
						RoleId = role.Id
					};
					ctx.RolePermissions.Add(rolePermission);
					await ctx.SaveChangesAsync();
				}

                TempData["success"] = "Create successfully";


                return RedirectToAction("Index");

			}
			await PrepareCommonDataAsync();
			var permision = await ctx.Permissions.ToListAsync();
			ViewBag.PermisionName = new SelectList(permision, "Id", "Name");
			return View(role);  
     
        }
        public async Task<IActionResult> Update(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewEmployee"))
            {
                await PrepareCommonDataAsync();
                var permision = await ctx.Permissions.ToListAsync();
                ViewBag.PermisionName = new SelectList(permision, "Id", "Name");

                var roleUpdate = await ctx.Roles
                    .Include(x => x!.RolePermissions!)
                    .ThenInclude(a => a.Permission)
                    .SingleOrDefaultAsync(a => a.Id == id);
                return View(roleUpdate);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }



        }
        [HttpPost]
        public async Task<IActionResult> Update(Role role,List<int> ListPermision)
        {
            var oldPermision = await ctx.RolePermissions
                .Where(a => a.RoleId == role.Id)
                .ToListAsync();
            foreach(var p in oldPermision)
            {
                ctx.RolePermissions?.Remove(p); 
                await ctx.SaveChangesAsync();   
            }

            foreach(var p in ListPermision) 
            {
                RolePermission rp = new RolePermission()
                {
                    RoleId = role.Id,
                    PermissionId =p
                }; 
                ctx.RolePermissions.Add(rp);  
                await ctx.SaveChangesAsync();   
            }
            TempData["success"] = "Update successfully";


            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<ActionResult> Delete(int id)
        {

            var itemToDelete = ctx.Roles.Find(id);
            var usersRole = ctx.EmployeeRoles.Where(u => u.RoleId == id).ToList();

            if (usersRole.Count > 0)
            {
                // There are users with this role, show an error message or handle it as needed
                @TempData["error"] = "Cannot delete the role because it is being used by one or more users.";
                return RedirectToAction("Index"); // You should create an Error view to display the error message
            }


            ctx.Roles.Remove(itemToDelete);
            TempData["success"] = "Delete successfully ";
            ctx.SaveChanges();

            return RedirectToAction("Index");



        }
    }
}

