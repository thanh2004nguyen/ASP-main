using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Group5.Models;
using Microsoft.EntityFrameworkCore;

namespace Group5.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Employee> Users { get; set; }

    
        public DbSet<Role> Roles { get; set; }
        public DbSet<EmployeeRole> EmployeeRoles { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<StationeryItem> StationeryItems { get; set; }
        public DbSet<EmployeePosition> EmployeePositions { get; set; }
        public DbSet<NewStationeryRequest> NewStationeryRequests { get; set; }
        public DbSet<RequestStatus> RequestStatus { get; set; }
        public DbSet<StockLevel> StockLevels { get; set; }
        public DbSet<UseLessItem> UseLessItems { get; set; }

        public DbSet<Cart> Carts { get; set; }
        public  DbSet<CartItem> CartItems { get; set; }
        public DbSet<RequestItem> RequestItems { get; set; }
        public DbSet<StationeryRequest> StationeryRequests { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }





        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmployeeRole>()
                .HasKey(er => new { er.EmployeeId, er.RoleId });

            modelBuilder.Entity<EmployeeRole>()
                .HasOne(er => er.Employee)
                .WithMany(e => e.EmployeeRoles)
                .HasForeignKey(er => er.EmployeeId);

            modelBuilder.Entity<EmployeeRole>()
                .HasOne(er => er.Role)
                .WithMany(r => r.EmployeeRoles)
                .HasForeignKey(er => er.RoleId);

            modelBuilder.Entity<NewStationeryRequest>()
                 .Property(e => e.Id)
                 .UseIdentityColumn(seed: 999990, increment: 1);

        }
    }
}
