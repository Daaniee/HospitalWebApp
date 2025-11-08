using System.ComponentModel.DataAnnotations;

public class CreateStaffDto
{

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; }

    [Phone]
    [MaxLength(20)]
    public string PhoneNumber { get; set; }

    [Required]
    [MaxLength(10)]
    public string Gender { get; set; }

    public string? Specialization { get; set; }

    [Required]
    [MaxLength(255)]
    public string ProfileImageUrl { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    [Required]
    public int RoleId { get; set; }
}
