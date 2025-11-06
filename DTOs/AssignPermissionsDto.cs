namespace hospitalwebapp.DTOs
{
    public class AssignPermissionsDto
    {
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public List<int> PermissionIds { get; set; } = new();
    }
}
