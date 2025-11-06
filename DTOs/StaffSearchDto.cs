using System.ComponentModel.DataAnnotations;

public class StaffSearchDto
{
    [Required]
    public int? Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public int? RoleId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool Descending { get; set; } = false;
}