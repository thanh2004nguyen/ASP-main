using AutoMapper;
using Group5.Data;
using Group5.Models;
using Group5.Service;
using Group5.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Cms.Ecc;
using System.Data;

namespace Group5.Controllers
{
    [AllowAnonymous]
    public class EmployeeController : BaseController
    {
        public IWebHostEnvironment env;
        public EmployeeController(ApplicationDbContext ctx, IMapper mapper, ICartService service, IRequestService request, IWebHostEnvironment env, ICustomRoleService customRoleService) : base(ctx, mapper, service, request, customRoleService)
        {
            this.env = env;
        }
        public async Task<IActionResult> Index()
        {
            await PrepareCommonDataAsync();

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewEmployee"))
            {
                var employee = await ctx.Users
                 .Include(a => a.EmployeePositions)
                 .Include(a => a.EmployeeRoles!)
                 .ThenInclude(a => a.Role)
                 .Include(a => a.Departments)
                .ToListAsync();

                var employeeView = mapper.Map<List<EmployeeViewModel>>(employee);

                return View(employeeView);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }



        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareCommonDataAsync();

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewEmployee"))
            {
                var ceo = await ctx.EmployeePositions
                       .SingleOrDefaultAsync(a => a.Position == "CEO");
                ViewBag.CEOId = ceo!.Id;
                var role = await ctx.Roles.ToListAsync();
                ViewBag.RoleName = new SelectList(role, "Id", "Name");
                var depart = await ctx.Departments.ToListAsync();
                var position = await ctx.EmployeePositions.ToListAsync();
                ViewBag.DepartName = new SelectList(depart, "Id", "Name");
                ViewBag.PositionName = new SelectList(position, "Id", "Position");
                return View();
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }



        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateEmployee emp, List<int> ListRoles, string Password, string ConfirmPassword)
        {
            if (ModelState.IsValid)
            {
                var existingUser = ctx.Users.FirstOrDefault(u => u.Email == emp.Email);

                if (existingUser != null)
                {
                    // Nếu đã tồn tại, báo lỗi và chuyển về trang Create
                    TempData["error"] = "Email đã tồn tại!";
                    return RedirectToAction("Create");
                }

                if (Password != ConfirmPassword)
                {
                    TempData["error"] = "Passwords do not match!";
                    return RedirectToAction("Create");
                }
                string fileName = "avatar.png";

                if (emp.Photo != null && emp.Photo.Length > 0)
                {
                    // Tạo đường dẫn tới thư mục lưu trữ hình ảnh
                    var imagesFolder = Path.Combine(env.WebRootPath, "images");
                    if (!Directory.Exists(imagesFolder))
                    {
                        Directory.CreateDirectory(imagesFolder);
                    }

                    // Tạo tên file hình ảnh (vd: guid.jpg)
                    fileName = Guid.NewGuid().ToString() + Path.GetExtension(emp.Photo.FileName);
                    string filePath = Path.Combine(imagesFolder, fileName);

                    // Lưu file hình ảnh
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await emp.Photo.CopyToAsync(fileStream);
                    }

                    // Lưu đường dẫn hình ảnh vào thuộc tính ImageUrl
                }
                var newPassword = BCrypt.Net.BCrypt.HashPassword(Password);
                var employeeView = mapper.Map<Employee>(emp);
                employeeView.Avatar = fileName;
                employeeView.Password = newPassword;
                // add Request Amount 
                var inputposition = await ctx.EmployeePositions.SingleOrDefaultAsync(ctx => ctx.Id == employeeView.EmployeePositionId);
                employeeView.AmountRequestPerMonth = inputposition!.MaxAmountPerMonth;
                employeeView.ResetAmountTime = DateTime.Now;

                ctx.Users.Add(employeeView);
                await ctx.SaveChangesAsync();
                foreach (var i in ListRoles)
                {
                    EmployeeRole employeeRole = new EmployeeRole()
                    {
                        RoleId = i,
                        EmployeeId = employeeView.Id
                    };
                    ctx.EmployeeRoles.Add(employeeRole);
                    await ctx.SaveChangesAsync();
                }
                TempData["success"] = "Create Employee Successfully!";
                return RedirectToAction("Index");

            }

            var role = await ctx.Roles.ToListAsync();
            ViewBag.RoleName = new SelectList(role, "Id", "Name");
            var depart = await ctx.Departments.ToListAsync();
            var position = await ctx.EmployeePositions.ToListAsync();
            ViewBag.DepartName = new SelectList(depart, "Id", "Name");
            ViewBag.PositionName = new SelectList(position, "Id", "Position");
            await PrepareCommonDataAsync();
            return View(emp);
        }
        public async Task<IActionResult> Update(int id)
        {
            await PrepareCommonDataAsync();

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewEmployee"))
            {
                var empUpdate = await ctx.Users!
           .Include(x => x.EmployeeRoles!)
             .ThenInclude(x => x.Role)
           .SingleOrDefaultAsync(ctx => ctx.Id == id);

                var supervisors = await ctx.Users.ToListAsync(); // Adjust this to get the supervisors based on your logic
                ViewBag.SupperVisorOptions = new SelectList(supervisors, "Id", "FullName");

                var Viewemp = mapper.Map<EmployeeViewModel>(empUpdate);

                var depart = await ctx.Departments.ToListAsync();
                var position = await ctx.EmployeePositions.ToListAsync();

                var role = await ctx.Roles.ToListAsync();
                var employeeRole = await ctx.EmployeeRoles
                    .Where(a => a.EmployeeId == id)
                    .ToListAsync();

                var roleIds = employeeRole.Select(er => er.RoleId).ToList();

                var currentRoles = await ctx.Roles
                    .Where(r => roleIds.Contains(r.Id))
                    .ToListAsync();

                ViewBag.CurrentRoleName = new SelectList(currentRoles, "Id", "Name");
                ViewBag.RoleName = new SelectList(role, "Id", "Name");
                ViewBag.DepartName = new SelectList(depart, "Id", "Name");
                ViewBag.PositionName = new SelectList(position, "Id", "Position");
                return View(Viewemp);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }



        }
        [HttpPost]
        public async Task<IActionResult> Update(CreateEmployee emp, List<int> ListRoles, string Password, string Avatar)
        {

            var employeeView = mapper.Map<Employee>(emp);
            //Delete old ROle
            var oldRole = await ctx.EmployeeRoles
                .Where(a => a.EmployeeId == employeeView.Id)
                .ToListAsync();
            foreach (var i in oldRole)
            {
                ctx.EmployeeRoles.Remove(i);
                await ctx.SaveChangesAsync();
            }
            foreach (var i in ListRoles)
            {

                EmployeeRole employeeRole = new EmployeeRole()
                {
                    RoleId = i,
                    EmployeeId = employeeView.Id
                };
                ctx.EmployeeRoles.Add(employeeRole);
                await ctx.SaveChangesAsync();
            }

            employeeView.Avatar = Avatar;
            employeeView.Password = Password;
            ctx.Users.Update(employeeView);
            TempData["success"] = "Update successfully";

            await ctx.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> GetSupervisors(int positionId)
        {
            var currentPosition = await ctx.EmployeePositions.SingleOrDefaultAsync(a => a.Id == positionId);
            var Suppervisors = await ctx.Users
                .Where(a => a.EmployeePositions!.Level > currentPosition!.Level)
                .ToListAsync();

            return Json(new
            {
                success = true,
                svisor = Suppervisors
            });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewEmployee"))
            {
                var itemToDelete = ctx.Users.Find(id);
                if (itemToDelete != null)
                {
                    ctx.Users.Remove(itemToDelete);
                    ctx.SaveChanges();
                }


                TempData["success"] = "Delete successfully";
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }



        }

        public async Task<IActionResult> Details(int? id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (customRoleService.UserHasPermission(userId, "ViewEmployee"))
            {

                if (id == null)
                {
                    return NotFound();
                }

                var employee = await ctx.Users
                    .Include(a => a.EmployeeRoles)
                        .ThenInclude(a => a.Role)
                    .Include(a => a.Departments)
                    .Include(a => a.EmployeePositions)


                    .FirstOrDefaultAsync(m => m.Id == id);

                if (employee == null)
                {
                    return NotFound();
                }

                var employeeViewModel = mapper.Map<EmployeeViewModel>(employee);
                await PrepareCommonDataAsync();

                // Tùy thuộc vào yêu cầu, bạn có thể muốn thêm các dòng code để chuẩn bị dữ liệu cho trang chi tiết.

                return View(employeeViewModel);
            }
            else
            {
                return RedirectToAction("unauthorize", "User");
            }


        }


        // Validate email
        [HttpGet]
        public ActionResult CheckEmail(string email)
        {
            var e = ctx.Users.Any(ctx => ctx.Email == email);
            if (e)
            {
                return Json(new { exists = true });
            }
            else
            {
                return Json(new { exists = false });
            }
        }
    }
}

