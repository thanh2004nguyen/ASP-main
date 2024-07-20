using AutoMapper;
using Group5.Data;
using Group5.Models;
using Group5.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Group5.Controllers
{
    public class EmployeePositionController : BaseController
    {
        public EmployeePositionController(ApplicationDbContext ctx, IMapper mapper, ICartService service, IRequestService request, ICustomRoleService customRoleService) : base(ctx, mapper, service, request, customRoleService)
        {
        }
        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewEmployee"))
            {
                await PrepareCommonDataAsync();
                var empPosition = await ctx.EmployeePositions.ToListAsync();
                return View(empPosition);
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
                return View();
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }

       
        }

        [HttpPost]

        public async Task<IActionResult> Create(EmployeePosition empPosition)
        {
            if(ModelState.IsValid)
            {
				// Thêm mới bản ghi vào bảng Departments
				ctx.EmployeePositions.Add(empPosition);
				await ctx.SaveChangesAsync();
                TempData["success"] = "Create successfully";

                return RedirectToAction("Index");
			}
   

			await PrepareCommonDataAsync();
            return View(empPosition) ; 



		}

        public async Task<IActionResult> Update(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewEmployee"))
            {
                await PrepareCommonDataAsync();
                var empPos = await ctx.EmployeePositions.SingleOrDefaultAsync(a => a.Id == id);
                return View(empPos);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }

      
        }
        [HttpPost]
        public async Task<IActionResult> Update(EmployeePosition empPos)
        {
            var existingPosition = await ctx.EmployeePositions
                 .FirstOrDefaultAsync(a => a.Id == empPos.Id);

            if (existingPosition.Position == empPos.Position)
            {
                existingPosition.MaxAmountPerMonth = empPos.MaxAmountPerMonth;
                existingPosition.Level = empPos.Level;  
                ctx.EmployeePositions.Update(existingPosition);
                await ctx.SaveChangesAsync();
                TempData["success"] = "Update Successfully";
                // The position is the same, so do nothing
                return RedirectToAction("Index");
            }
            else
            {
                // Check if the new position already exists in the database
                var isPositionExists = await ctx.EmployeePositions
                    .AnyAsync(a => a.Position == empPos.Position);

                if (isPositionExists)
                {
                    // The new position already exists, show an error
                    ModelState.AddModelError(nameof(EmployeePosition.Position), "Position already exists.");
                    await PrepareCommonDataAsync();
                    return View(empPos);
                }
                // Update only the properties you need
                existingPosition.Position = empPos.Position;
                existingPosition.Level = empPos.Level;
                // Update any other properties as needed
                ctx.EmployeePositions.Update(existingPosition);
                await ctx.SaveChangesAsync();
                TempData["success"] = "Update Successfully";
                return RedirectToAction("Index");
            }

 
        }
        [HttpGet] // Use [HttpPost] attribute to handle POST requests
        public async Task<ActionResult> Delete(int id)
        {


            var itemToDelete = ctx.EmployeePositions.Find(id);

            var usersDepartmeent = ctx.Users.Where(u => u.EmployeePositionId == id).ToList();

            if (usersDepartmeent.Count > 0)
            {

                TempData["success"] = "Cannot delete the role because it is being used by one or more users.";
                return RedirectToAction("Index");
            }
            TempData["success"] = "Delete successfully ";
            ctx.EmployeePositions.Remove(itemToDelete);
            ctx.SaveChanges();

            // Redirect to a different action or view after deletion
            return RedirectToAction("Index"); // Redirect to the Index action, change it as needed
        }
    }
}
