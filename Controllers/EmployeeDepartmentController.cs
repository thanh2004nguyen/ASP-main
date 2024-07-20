using AutoMapper;
using Group5.Data;
using Group5.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Group5.Models;

namespace Group5.Controllers
{
    public class EmployeeDepartmentController : BaseController
    {
        public EmployeeDepartmentController(ApplicationDbContext ctx, IMapper mapper, ICartService service, IRequestService request, ICustomRoleService customRoleService) : base(ctx, mapper, service, request,customRoleService)
        {
        }
        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewEmployee"))
            {
                await PrepareCommonDataAsync();
                var employeeDepartment = await ctx.Departments.ToListAsync();
                return View(employeeDepartment);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }

      
        }
        public async Task<IActionResult> CreateAsync()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewEmployee"))
            {
                await PrepareCommonDataAsync();
                return View();
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }

         
        }

        [HttpPost]
        
        public async Task<IActionResult> Create( Department department)
        {
            if(ModelState.IsValid)
            {
				// Thêm mới bản ghi vào bảng Departments
				ctx.Departments.Add(department);
				await ctx.SaveChangesAsync();
                TempData["success"] = "Create successfully";

                return RedirectToAction("Index");
			}
			await PrepareCommonDataAsync();
			return View(department);
     

        }

        public async Task<IActionResult> Update(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewEmployee"))
            {
                await PrepareCommonDataAsync();
                var empDepar = await ctx.Departments.FindAsync(id);
                return View(empDepar);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }

      
        }
        [HttpPost]
        public async Task<IActionResult> Update(Department dep)
        {
            var existingPosition = await ctx.Departments
                 .FirstOrDefaultAsync(a => a.Id == dep.Id);

            if (existingPosition.Name == dep.Name)
            {

                return RedirectToAction("Index");
            }
            else
            {

                var isPositionExists = await ctx.Departments
                    .AnyAsync(a => a.Name == dep.Name);

                if (isPositionExists)
                {

                    ModelState.AddModelError(nameof(Department.Name), "Department already exists.");
                    await PrepareCommonDataAsync();
                    return View(dep);
                }
                existingPosition.Name = dep.Name;

                TempData["success"] = "Update successfully";

                await ctx.SaveChangesAsync();
                return RedirectToAction("Index");
            }
        }

        [HttpGet] // Use [HttpPost] attribute to handle POST requests
        public async Task<ActionResult> Delete(int id)
        {
            var usersDepartmeent = ctx.Users.Where(u => u.DepartmentId == id).ToList();

            if (usersDepartmeent.Count > 0)
            {

                TempData["error"] = "Cannot delete the role because it is being used by one or more users.";
                return RedirectToAction("Index");
            }
            var itemToDelete = ctx.Departments.Find(id);
            ctx.Departments.Remove(itemToDelete);
            TempData["success"] = "Delete successfully";

            ctx.SaveChanges();

            return RedirectToAction("Index");
        }
    }

}
