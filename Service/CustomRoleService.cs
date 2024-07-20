using Group5.Data;
using Microsoft.EntityFrameworkCore;

namespace Group5.Service
{
    public class CustomRoleService: ICustomRoleService
    {
        private readonly ApplicationDbContext ctx;

        public CustomRoleService(ApplicationDbContext ctx)
        {
            this.ctx = ctx;
        }

        public  bool UserHasPermission(int? employeeId, string permission)
        {
            var userRoles =  ctx.EmployeeRoles
                .Include(x => x.Role)
                  .ThenInclude(a=>a!.RolePermissions!)
                  .ThenInclude(a=>a.Permission)
                .Where(er => er.EmployeeId == employeeId)
                .ToList();

            return userRoles.Any(a => a != null && a!.Role!.RolePermissions!.Any(er => er.Permission!.Name == permission));
            // Check if any of the user's roles have the required permission
           
        }

    }
}
