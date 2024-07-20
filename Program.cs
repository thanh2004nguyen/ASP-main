using Group5.Data;
using Group5.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Group5.Helpers;
using Group5.Hubs;
using Group5.Service;
using Group5.Shared;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(o =>
{
    o.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationDbContextConnection"));
});

builder.Services.AddHttpClient<IMessageService, MessageService>();
builder.Services.AddScoped<ICommonMethod, CommonMethod>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ICustomRoleService, CustomRoleService>();
builder.Services.AddScoped<CustomViewDataFilter>();
builder.Services.AddMvc(options =>
{
    options.Filters.AddService<CustomViewDataFilter>();
});

//session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(9900000);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();



builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddTransient<IFileValidator, FileValidator>();
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Login";
    options.AccessDeniedPath = "/unauthozied";
    options.ExpireTimeSpan = TimeSpan.FromDays(1);
});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
    options.AddPolicy("ManageOnly", policy => policy.RequireRole("manage"));
});


builder.Services.AddMvc(options =>
{
    
    options.Filters.Add(new AuthorizeFilter());
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<ChatHub>("/chatHub");
app.MapRazorPages();
app.Run();