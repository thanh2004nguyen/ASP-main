using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Group5.Migrations
{
    public partial class seed : Migration
    {
        private string ManagerRoleId = Guid.NewGuid().ToString();
        private string UserRoleId = Guid.NewGuid().ToString();
        private string AdminRoleId = Guid.NewGuid().ToString();

        private string User1Id = Guid.NewGuid().ToString();
        private string User2Id = Guid.NewGuid().ToString();

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            SeedRolesSQL(migrationBuilder);
            SeedPermisionSQL(migrationBuilder);

            SeedPermisionRoleSQL(migrationBuilder);

            SeedPositionSQL(migrationBuilder);

            SeedRequestStatusSQL(migrationBuilder);

            SeedDepartment(migrationBuilder);

            SeedCategory(migrationBuilder);

            SeedBrand(migrationBuilder);

            SeedStockLvSQL(migrationBuilder);

            SeedUseLessSQL(migrationBuilder);

            SeedStationeryItem(migrationBuilder);
      
            SeedUser(migrationBuilder);

            SeedEmployeeRoleSQL(migrationBuilder);

          

        }
        private void SeedStockLvSQL(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[StockLevels] ([MinQuantity],[Level])
            VALUES (10,1);");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[StockLevels] ([MinQuantity],[Level])
            VALUES (20,2);");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[StockLevels] ([MinQuantity],[Level])
            VALUES (30,3);");

        }
        private void SeedPermisionSQL(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Permissions] ([Name])
            VALUES ('ViewStock');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Permissions] ([Name])
            VALUES ('ViewEmployee');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Permissions] ([Name])
            VALUES ('ViewAdminChat');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Permissions] ([Name])
            VALUES ('ViewStationery');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Permissions] ([Name])
            VALUES ('CanRequest');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Permissions] ([Name])
            VALUES ('ViewRequestManage');");
        }

        private void SeedPermisionRoleSQL(MigrationBuilder migrationBuilder)
        {
            // admin
        

            migrationBuilder.Sql(@$"INSERT INTO [dbo].[RolePermissions] ([RoleId],[PermissionId])
            VALUES ((SELECT TOP 1 Id FROM Roles WHERE Name='admin'),(SELECT TOP 1 Id FROM Permissions WHERE Name='ViewStock'));");

            migrationBuilder.Sql(@$"INSERT INTO [dbo].[RolePermissions] ([RoleId],[PermissionId])
            VALUES ((SELECT TOP 1 Id FROM Roles WHERE Name='admin'),(SELECT TOP 1 Id FROM Permissions WHERE Name='ViewEmployee'));");

            migrationBuilder.Sql(@$"INSERT INTO [dbo].[RolePermissions] ([RoleId],[PermissionId])
            VALUES ((SELECT TOP 1 Id FROM Roles WHERE Name='admin'),(SELECT TOP 1 Id FROM Permissions WHERE Name='ViewAdminChat'));");

            migrationBuilder.Sql(@$"INSERT INTO [dbo].[RolePermissions] ([RoleId],[PermissionId])
            VALUES ((SELECT TOP 1 Id FROM Roles WHERE Name='admin'),(SELECT TOP 1 Id FROM Permissions WHERE Name='ViewStationery'));");

            migrationBuilder.Sql(@$"INSERT INTO [dbo].[RolePermissions] ([RoleId],[PermissionId])
            VALUES ((SELECT TOP 1 Id FROM Roles WHERE Name='admin'),(SELECT TOP 1 Id FROM Permissions WHERE Name='CanRequest'));");

            migrationBuilder.Sql(@$"INSERT INTO [dbo].[RolePermissions] ([RoleId],[PermissionId])
            VALUES ((SELECT TOP 1 Id FROM Roles WHERE Name='admin'),(SELECT TOP 1 Id FROM Permissions WHERE Name='ViewRequestManage'));");

            //CEO
      

            migrationBuilder.Sql(@$"INSERT INTO [dbo].[RolePermissions] ([RoleId],[PermissionId])
            VALUES ((SELECT TOP 1 Id FROM Roles WHERE Name='CEO'),(SELECT TOP 1 Id FROM Permissions WHERE Name='ViewStock'));");

            migrationBuilder.Sql(@$"INSERT INTO [dbo].[RolePermissions] ([RoleId],[PermissionId])
            VALUES ((SELECT TOP 1 Id FROM Roles WHERE Name='CEO'),(SELECT TOP 1 Id FROM Permissions WHERE Name='ViewEmployee'));");

            migrationBuilder.Sql(@$"INSERT INTO [dbo].[RolePermissions] ([RoleId],[PermissionId])
            VALUES ((SELECT TOP 1 Id FROM Roles WHERE Name='CEO'),(SELECT TOP 1 Id FROM Permissions WHERE Name='ViewAdminChat'));");

            migrationBuilder.Sql(@$"INSERT INTO [dbo].[RolePermissions] ([RoleId],[PermissionId])
            VALUES ((SELECT TOP 1 Id FROM Roles WHERE Name='CEO'),(SELECT TOP 1 Id FROM Permissions WHERE Name='ViewStationery'));");

            migrationBuilder.Sql(@$"INSERT INTO [dbo].[RolePermissions] ([RoleId],[PermissionId])
            VALUES ((SELECT TOP 1 Id FROM Roles WHERE Name='CEO'),(SELECT TOP 1 Id FROM Permissions WHERE Name='CanRequest'));");

            migrationBuilder.Sql(@$"INSERT INTO [dbo].[RolePermissions] ([RoleId],[PermissionId])
            VALUES ((SELECT TOP 1 Id FROM Roles WHERE Name='CEO'),(SELECT TOP 1 Id FROM Permissions WHERE Name='ViewRequestManage'));");

            //enjineer
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[RolePermissions] ([RoleId],[PermissionId])
            VALUES ((SELECT TOP 1 Id FROM Roles WHERE Name='engineer'),(SELECT TOP 1 Id FROM Permissions WHERE Name='CanRequest'));"); 
        }



        private void SeedUseLessSQL(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[UseLessItems] ([StockLevel],[MaxTime])
            VALUES (1,730);");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[UseLessItems] ([StockLevel],[MaxTime])
            VALUES (2,365);");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[UseLessItems] ([StockLevel],[MaxTime])
            VALUES (3,180);");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[UseLessItems] ([StockLevel],[MaxTime])
            VALUES (0,0);");
        }


        private void SeedRolesSQL(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Roles] ([Name])
            VALUES ('admin');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Roles] ([Name])
            VALUES ('engineer');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Roles] ([Name])
            VALUES ('manage');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Roles] ([Name])
            VALUES ('staffmanage');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Roles] ([Name])
            VALUES ('CEO');");
        }

        private void SeedRequestStatusSQL(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[RequestStatus] ([Status])
            VALUES ('Wait For Approvement');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[RequestStatus] ([Status])
            VALUES ('Approved');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[RequestStatus] ([Status])
            VALUES ('Rejected');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[RequestStatus] ([Status])
            VALUES ('Canceled/WithDraw');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[RequestStatus] ([Status])
            VALUES ('Sending');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[RequestStatus] ([Status])
            VALUES ('Wait For Stock');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[RequestStatus] ([Status])
            VALUES ('Completed');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[RequestStatus] ([Status])
            VALUES ('Canceling! Wait For Approval');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[RequestStatus] ([Status])
            VALUES ('In Progress');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[RequestStatus] ([Status])
            VALUES ('Canceling');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[RequestStatus] ([Status])
            VALUES ('Cancel InProgress/Completed Request');");

        }


        private void SeedPositionSQL(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[EmployeePositions] ([Position],[MaxAmountPerMonth],[Level])
            VALUES ('Engineer',1000,1);");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[EmployeePositions] ([Position],[MaxAmountPerMonth],[Level])
            VALUES ('Manager',2000,2);");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[EmployeePositions] ([Position],[MaxAmountPerMonth],[Level])
            VALUES ('Staff Manager',3000,3);");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[EmployeePositions] ([Position],[MaxAmountPerMonth],[Level])
            VALUES ('CEO',5000,4);");

        }
        private void SeedDepartment(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Departments] ([Name])
            VALUES ('Sales');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Departments] ([Name])
            VALUES ('HR');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Departments] ([Name])
            VALUES ('Marketing');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Departments] ([Name])
            VALUES ('Technical Support');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Departments] ([Name])
            VALUES ('Customer Services');");
        }

        private void SeedCategory(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Categories] ([Name])
            VALUES ('PhotoPayper');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Categories] ([Name])
            VALUES ('Cutting');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Categories] ([Name])
            VALUES ('Print');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Categories] ([Name])
            VALUES ('Writer');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Categories] ([Name])
            VALUES ('Ink');");
        }


        private void SeedBrand(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Brands] ([ImageUrl],[Name])
            VALUES (NULL,'VN');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Brands] ([ImageUrl],[Name])
            VALUES (NULL,'VIETTIEN');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Brands] ([ImageUrl],[Name])
            VALUES (NULL,'AKITOS');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[Brands] ([ImageUrl],[Name])
            VALUES (NULL,'BLUERAY');");

        }


        private void SeedStationeryItem(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[StationeryItems] ([Name],[Description],[Price],[ImageUrl],[CategoryId],[BrandId],[Quantity],[TypeOfQuantity],[StockLvId],[CreatedAt],[LastStockOut])
            VALUES ('Desk Stationery Set','This is the go to stationery kit for anyone who loves to be organised and plan out every single detail ahead, for the weekly and monthly schedules.',15,'DeskStationerySet.png',(SELECT TOP 1 Id FROM Categories ORDER BY NEWID()),(SELECT TOP 1 Id FROM Brands ORDER BY NEWID()),1,'Item', (SELECT TOP 1 Id FROM StockLevels WHERE MinQuantity = 30),'1/1/2023','1/1/2021');");

            migrationBuilder.Sql(@$"INSERT INTO [dbo].[StationeryItems] ([Name],[Description],[Price],[ImageUrl],[CategoryId],[BrandId],[Quantity],[TypeOfQuantity],[StockLvId],[CreatedAt],[LastStockOut])
            VALUES ('Calculator','Specification: Plastic-12 digits,dual',20,'calculator.PNG',(SELECT TOP 1 Id FROM Categories ORDER BY NEWID()),(SELECT TOP 1 Id FROM Brands ORDER BY NEWID()),22,'Botle',(SELECT TOP 1 Id FROM StockLevels WHERE MinQuantity = 30),'1/1/2023','1/1/2023');");

            migrationBuilder.Sql(@$"INSERT INTO [dbo].[StationeryItems] ([Name],[Description],[Price],[ImageUrl],[CategoryId],[BrandId],[Quantity],[TypeOfQuantity],[StockLvId],[CreatedAt],[LastStockOut])
            VALUES ('Colourful Life Book','Delve into the pages of our Colourful Life Book part of the ‘Colourful Life’ series. Customise your softcover photo book with the colour of your choice as you pick from a variety of fine Italian papers. Wrapped neatly across the cover as it perfectly frames the die cut centre that enhances your chosen cover photo. Perfect for your own to keep or even great as a gift. ',5,'Photobook.jpg',(SELECT TOP 1 Id FROM Categories ORDER BY NEWID()),(SELECT TOP 1 Id FROM Brands ORDER BY NEWID()),22,'Botle',(SELECT TOP 1 Id FROM StockLevels WHERE MinQuantity = 30),'1/1/2023','1/1/2022');");

            migrationBuilder.Sql(@$"INSERT INTO [dbo].[StationeryItems] ([Name],[Description],[Price],[ImageUrl],[CategoryId],[BrandId],[Quantity],[TypeOfQuantity],[StockLvId],[CreatedAt],[LastStockOut])
            VALUES ('Pen','A pen is a common writing instrument that applies ink to a surface, usually paper, for writing or drawing',1,'Pen.PNG',(SELECT TOP 1 Id FROM Categories ORDER BY NEWID()),(SELECT TOP 1 Id FROM Brands ORDER BY NEWID()),22,'Item',(SELECT TOP 1 Id FROM StockLevels WHERE MinQuantity = 30),'1/1/2023','1/1/2023');");

            migrationBuilder.Sql(@$"INSERT INTO [dbo].[StationeryItems] ([Name],[Description],[Price],[ImageUrl],[CategoryId],[BrandId],[Quantity],[TypeOfQuantity],[StockLvId],[CreatedAt],[LastStockOut])
            VALUES ('Payper','A basic good quality paper that is ideal for printing every day documents such as emails and forms. ECF (Elemental Chlorine Free).',15,'Paper.PNG',(SELECT TOP 1 Id FROM Categories ORDER BY NEWID()),(SELECT TOP 1 Id FROM Brands ORDER BY NEWID()),22,'Item',(SELECT TOP 1 Id FROM StockLevels WHERE MinQuantity = 30),'1/1/2023','1/1/2023');");

            migrationBuilder.Sql(@$"INSERT INTO [dbo].[StationeryItems] ([Name],[Description],[Price],[ImageUrl],[CategoryId],[BrandId],[Quantity],[TypeOfQuantity],[StockLvId],[CreatedAt],[LastStockOut])
            VALUES ('Printer','The Workforce AL-M310DN delivers absolute performance and reliability despite their compact size. Equipped with network printing capabilities and blazing fast performance to support ultra-high volume printing, these printers are built for the most demanding office environments.',200,'Printer.jpg',(SELECT TOP 1 Id FROM Categories ORDER BY NEWID()),(SELECT TOP 1 Id FROM Brands ORDER BY NEWID()),22,'Item',(SELECT TOP 1 Id FROM StockLevels WHERE MinQuantity = 30),'1/1/2023','1/1/2023');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[StationeryItems] ([Name],[Description],[Price],[ImageUrl],[CategoryId],[BrandId],[Quantity],[TypeOfQuantity],[StockLvId],[CreatedAt],[LastStockOut])
            VALUES ('Box File','Box files and filing boxes are perfect for filling, storing, & archiving paperwork.',11,'BoxFile.JPG',(SELECT TOP 1 Id FROM Categories ORDER BY NEWID()),(SELECT TOP 1 Id FROM Brands ORDER BY NEWID()),9,'100Sheet',(SELECT TOP 1 Id FROM StockLevels WHERE MinQuantity = 30),'1/1/2023','1/1/2023');");
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[StationeryItems] ([Name],[Description],[Price],[ImageUrl],[CategoryId],[BrandId],[Quantity],[TypeOfQuantity],[StockLvId],[CreatedAt],[LastStockOut])
            VALUES ('NoteBook','A notebook is more than just a practical tool. It can be a source of joy, a covetable item that turns an ordinary, everyday task—note-taking, journaling, task-planning, brainstorming, or doodling—into a sublime experience.',4,'NoteBook.JPG',(SELECT TOP 1 Id FROM Categories ORDER BY NEWID()),(SELECT TOP 1 Id FROM Brands ORDER BY NEWID()),23,'100ml',(SELECT TOP 1 Id FROM StockLevels WHERE MinQuantity = 30),'1/1/2023','1/1/2022');");
        }


  

        private void SeedUser(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(
           @$"INSERT [dbo].[Users] ([FullName],[Email], 
                [Avatar], [Password], [DepartmentId],[EmployeePositionId],[AmountRequestPerMonth]) VALUES 
                (N'CEO', N'ceo@gmail.com', 'avatar3.jpg', N'$2a$12$fniRPQUe9G2AcNS8yuaLJu6aisxQ21GYVJmJIDT6zTpK2qpR2r0t.', 
                (SELECT TOP 1 Id FROM Departments ORDER BY NEWID()),4,5000)");

            migrationBuilder.Sql(
                @$"INSERT [dbo].[Users] ([FullName], [Email], 
                [Avatar], [Password], [DepartmentId],[EmployeePositionId],[AmountRequestPerMonth]) VALUES 
                (N'Admin', N'admin@gmail.com', 'avatar1.jpg', N'$2a$12$fniRPQUe9G2AcNS8yuaLJu6aisxQ21GYVJmJIDT6zTpK2qpR2r0t.', 
                (SELECT TOP 1 Id FROM Departments ORDER BY NEWID()),4,5000)");

            migrationBuilder.Sql(
           @$"INSERT [dbo].[Users] ([FullName], [Email], 
                [Avatar], [Password], [DepartmentId],[EmployeePositionId],[SupperVisorId],[AmountRequestPerMonth]) VALUES 
                (N'engineer', N'engineer@gmail.com', 'avatar2.jpg', N'$2a$12$fniRPQUe9G2AcNS8yuaLJu6aisxQ21GYVJmJIDT6zTpK2qpR2r0t.', 
                (SELECT TOP 1 Id FROM Departments ORDER BY NEWID()),2,1,5000)");


            migrationBuilder.Sql(
           @$"INSERT [dbo].[Users] ([FullName],[Email], 
                [Avatar], [Password], [DepartmentId],[EmployeePositionId],[SupperVisorId],[AmountRequestPerMonth]) VALUES 
                (N'staffmanage', N'staffmanage@gmail.com','avatar4.jpg', N'$2a$12$fniRPQUe9G2AcNS8yuaLJu6aisxQ21GYVJmJIDT6zTpK2qpR2r0t.', 
                (SELECT TOP 1 Id FROM Departments ORDER BY NEWID()),2,1,5000)");

            migrationBuilder.Sql(
           @$"INSERT [dbo].[Users] ([FullName],[Email], 
                [Avatar], [Password], [DepartmentId],[EmployeePositionId],[SupperVisorId],[AmountRequestPerMonth]) VALUES 
                (N'manage', N'manage@gmail.com','avatar5.jpg', N'$2a$12$fniRPQUe9G2AcNS8yuaLJu6aisxQ21GYVJmJIDT6zTpK2qpR2r0t.', 
                (SELECT TOP 1 Id FROM Departments ORDER BY NEWID()),2,1,5000)");

            migrationBuilder.Sql(
 @$"INSERT [dbo].[Users] ([FullName],[Email], 
                [Avatar], [Password], [DepartmentId],[EmployeePositionId],[SupperVisorId],[AmountRequestPerMonth]) VALUES 
                (N'Hulk', N'manage1@gmail.com','avatar6.jpg', N'$2a$12$fniRPQUe9G2AcNS8yuaLJu6aisxQ21GYVJmJIDT6zTpK2qpR2r0t.', 
                (SELECT TOP 1 Id FROM Departments ORDER BY NEWID()),2,1,5000)");



            migrationBuilder.Sql(
 @$"INSERT [dbo].[Users] ([FullName],[Email], 
                [Avatar], [Password], [DepartmentId],[EmployeePositionId],[SupperVisorId],[AmountRequestPerMonth]) VALUES 
                (N'Cap Tan', N'manage2@gmail.com','avatar7.jpg', N'$2a$12$fniRPQUe9G2AcNS8yuaLJu6aisxQ21GYVJmJIDT6zTpK2qpR2r0t.', 
                (SELECT TOP 1 Id FROM Departments ORDER BY NEWID()),2,1,5000)");



            migrationBuilder.Sql(
 @$"INSERT [dbo].[Users] ([FullName],[Email], 
                [Avatar], [Password], [DepartmentId],[EmployeePositionId],[SupperVisorId],[AmountRequestPerMonth]) VALUES 
                (N'David Ly', N'manage3@gmail.com','avatar8.jpg', N'$2a$12$fniRPQUe9G2AcNS8yuaLJu6aisxQ21GYVJmJIDT6zTpK2qpR2r0t.', 
                (SELECT TOP 1 Id FROM Departments ORDER BY NEWID()),2,1,5000)");


            migrationBuilder.Sql(
 @$"INSERT [dbo].[Users] ([FullName],[Email], 
                [Avatar], [Password], [DepartmentId],[EmployeePositionId],[SupperVisorId],[AmountRequestPerMonth]) VALUES 
                (N'David SI', N'manage4@gmail.com','avatar9.jpg', N'$2a$12$fniRPQUe9G2AcNS8yuaLJu6aisxQ21GYVJmJIDT6zTpK2qpR2r0t.', 
                (SELECT TOP 1 Id FROM Departments ORDER BY NEWID()),3,1,5000)");
        }

        private void SeedEmployeeRoleSQL(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@$"INSERT INTO [dbo].[EmployeeRoles] ([EmployeeId],[RoleId])
            VALUES ((SELECT TOP 1 Id FROM Users WHERE FullName='Admin'),(SELECT TOP 1 Id FROM Roles WHERE Name='admin'));");

            migrationBuilder.Sql(@$"INSERT INTO [dbo].[EmployeeRoles] ([EmployeeId],[RoleId])
            VALUES ((SELECT TOP 1 Id FROM Users WHERE FullName='CEO'),(SELECT TOP 1 Id FROM Roles WHERE Name='CEO'));");

            migrationBuilder.Sql(@$"INSERT INTO [dbo].[EmployeeRoles] ([EmployeeId],[RoleId])
            VALUES ((SELECT TOP 1 Id FROM Users WHERE FullName='engineer'),(SELECT TOP 1 Id FROM Roles WHERE Name='engineer'));");
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
