namespace hospitalwebapp.DTOs
{
    public class UpdateRoleDto
    {
        public int? Id { get; set; }
        public string? Name { get; set; } // current name
        public string? NewName { get; set; } // new name to update
    }
}
