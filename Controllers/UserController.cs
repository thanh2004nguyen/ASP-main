using Group5.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Group5.Models;
using System.Runtime.CompilerServices;
using Group5.ViewModels;
using Microsoft.CodeAnalysis.CSharp;
using Group5.Service;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;
using Group5.Shared;

namespace Group5.Controllers
{

    public class UserController : BaseController
    {
        public ICommonMethod common;
        public UserController(ApplicationDbContext ctx, IMapper mapper, ICartService service, IRequestService request, ICommonMethod common, ICustomRoleService customRoleService) : base(ctx, mapper, service, request,customRoleService)
        {
            this.common = common;
        }

        [AllowAnonymous]
        [Route("unauthozied")]
        public async Task<IActionResult> UnAuthorize()
        {
            await PrepareCommonDataAsync();
            return View("unauthorize");
        }
        [Route("login")]
        [AllowAnonymous]
        public async  Task<IActionResult> Login()
        {
          
            string? message = TempData["passchange"] as string;

            if (message != null)
            {
                ViewData["ChangePassSuccessed"] = message;
            }

            string? success = TempData["success"] as string;

            if (message != null)
            {
                ViewData["success"] = success;
            }


            return View("Login");
        }

        [Route("login")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginDto data)
        {

            if (ModelState.IsValid)
            {

                var check = await ctx.Users
                        .Include(e => e.EmployeeRoles!)
                        .ThenInclude(er => er.Role)
                    .Where(u => u.Email == data.Email)
                    .FirstOrDefaultAsync();

                if (check == null)
                {
                    ViewData["Error"] = "Email Or Password Not Correct ! Please Try Again";
                    return View("Login");
                }
                else
                {

                    if (!BCrypt.Net.BCrypt.Verify(data.Password, check.Password))
                    {

                        ViewData["Error"] = "Email Or Password Not Correct ! Please Try Again";
                        return View("Login");
                    }

                    var claims = new List<Claim>()
                    {
                         new Claim(ClaimTypes.NameIdentifier,check!.Email!),
                         new Claim("id", check.Id.ToString()),
                    };

                    // Add roles to claims
                    claims.AddRange(check.EmployeeRoles!.Select(er => new Claim(ClaimTypes.Role, er.Role!.Name!)));
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var properties = new AuthenticationProperties()
                    {
                        AllowRefresh = true,
                        IsPersistent = true
                    };
                    await HttpContext
                        .SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);

                    HttpContext.Session.SetString("loginemail", check!.Email!);
                                  return RedirectToAction("Index", "Home");
                }

            }
            return View("Login");
        }

        [Route("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "User");

        }



        [Route("Register")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Register()
        {

            return View("Register");
        }
        public async Task<IActionResult> Detail()
        {
            await PrepareCommonDataAsync();
            var loginEmail = "";
            if (User?.Identity?.IsAuthenticated == true)
            {
                loginEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            var user = await ctx.Users
                .Include(a => a.Departments)
                .Include(a => a.EmployeePositions)
                .FirstOrDefaultAsync(a => a.Email == loginEmail);

            return View(user);
        }


        /*
                public async Task<IActionResult> UpdateAvatar(int id, IFormFile newPhoto)
                {
                    var employee = await ctx.Users.FindAsync(id);

                    if (employee == null)
                    {
                        return NotFound();
                    }

                    string imageUrl = employee.Avatar;
                    string fileName = null;

                    if (newPhoto != null && newPhoto.Length > 0)
                    {
                        // Tạo đường dẫn tới thư mục lưu trữ hình ảnh
                        var imagesFolder = Path.Combine(env.WebRootPath, "images");
                        if (!Directory.Exists(imagesFolder))
                        {
                            Directory.CreateDirectory(imagesFolder);
                        }

                        // Tạo tên file hình ảnh (vd: guid.jpg)
                        fileName = Guid.NewGuid().ToString() + Path.GetExtension(newPhoto.FileName);
                        string filePath = Path.Combine(imagesFolder, fileName);

                        // Lưu file hình ảnh
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await newPhoto.CopyToAsync(fileStream);
                        }

                        // Lưu đường dẫn hình ảnh vào thuộc tính ImageUrl
                        imageUrl = "/images/" + fileName;

                        // Xóa file ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(employee.Avatar))
                        {
                            var oldImagePath = Path.Combine(env.WebRootPath, employee.Avatar.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Cập nhật đường dẫn hình ảnh mới
                        employee.Avatar = imageUrl;
                        await ctx.SaveChangesAsync();
                    }

                    return RedirectToAction("Details", new { id = id });
                }*/

        /*   [Route("Register")]
           [Authorize(Policy = "AdminOnly")]
           [HttpPost]*/

        /*public async Task<IActionResult> Register(RegisterDto info)
        {
            if (ModelState.IsValid)
            {
                var data = new User()
                {
                    Username = info.Name,
                    Password = BCrypt.Net.BCrypt.HashPassword(info.Pass),
                    Role = info.Role
                };

                await _context.AddAsync(data);
                await _context.SaveChangesAsync();
                TempData["success"] = "Đăng ký thành công. vui lòng đăng nhập để sử dụng";
                return RedirectToAction("Login");
            }
            return View("Register");
        }*/
        [Route("changePassword")]
        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            await PrepareCommonDataAsync();
            return View("ChangePassword");
        }

        [Route("changePassword")]
        [HttpPost]



        public async Task<IActionResult> ChangePassword(ConfirmPassword confirm)
        {
            var loginEmail = "";
            if (User?.Identity?.IsAuthenticated == true)
            {
                loginEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            var user = await ctx.Users
                .Include(a => a.Departments)
                .Include(a => a.EmployeePositions)
                .FirstOrDefaultAsync(a => a.Email == loginEmail);





            if (ModelState.IsValid)
            {

                if (!BCrypt.Net.BCrypt.Verify(confirm.CurrentPassword, user.Password))
                {
                    ViewData["Error"] = "Current Password Not Correct";
                    return RedirectToAction("ChangePassword");
                }
                else
                {
                    var newPassword = BCrypt.Net.BCrypt.HashPassword(confirm.NewPassword);
                    user.Password = newPassword;
                    await ctx.SaveChangesAsync();
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    TempData["passchange"] = "Password changed successfully!!";
                    return RedirectToAction("Login");
                }
            }
            return RedirectToAction("Detail");
        }


        [Route("ChangeAvatar")]
        [HttpGet]
        public async Task<IActionResult> ChangeAvatar()
        {
            await PrepareCommonDataAsync();
            var loginEmail = "";
            if (User?.Identity?.IsAuthenticated == true)
            {
                loginEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            var user = await ctx.Users
                .Include(a => a.Departments)
                .Include(a => a.EmployeePositions)
                .FirstOrDefaultAsync(a => a.Email == loginEmail);

            var viewuser = mapper.Map<EmployeeViewModel>(user);
            return View(viewuser);
        }

        [Route("changeAvatar")]
        [HttpPost]
        public async Task<IActionResult> ChangeAvatar(IFormFile Photo)
        {
            if(Photo == null)
            {
                return RedirectToAction("Detail");
            }
            else
            {
                var loginEmail = "";
                if (User?.Identity?.IsAuthenticated == true)
                {
                    loginEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);
                }

                var user = await ctx.Users
                    .Include(a => a.Departments)
                    .Include(a => a.EmployeePositions)
                    .FirstOrDefaultAsync(a => a.Email == loginEmail);


                if (ModelState.IsValid)
                {
                    var photourl = await common.UploadImage(Photo);
                    user!.Avatar = photourl;
                    ctx.Users.Update(user);
                    await ctx.SaveChangesAsync();
                    TempData["success"] = "Update Avatar success";
                    return RedirectToAction("Detail");

                }
                return View();
            }
      
        }
    }
}