namespace Group5.Service
{
    public interface ICustomRoleService
    {
        bool UserHasPermission(int? employeeId, string permission);
    }
}
