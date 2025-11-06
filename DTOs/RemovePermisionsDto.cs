namespace hospitalwebapp.DTOs
{
    public class RemovePermissionsDto
    {
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public List<int> PermissionIds { get; set; } = new();
    }
}
