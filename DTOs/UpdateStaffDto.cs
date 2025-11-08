using System.ComponentModel.DataAnnotations;

public class UpdateStaffDto
{
    [MaxLength(100)]
    public string? FullName { get; set; }

    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(10)]
    public string? Gender { get; set; }

    public string? Specialization { get; set; }

    [MaxLength(255)]
    public string? ProfileImageUrl { get; set; }

    public bool? IsActive { get; set; }

    public int? RoleId { get; set; }
}
