using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hospitalwebapp.Models
{
    public class Staff
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public required string CustomId { get; set; }

    [Required]
    [MaxLength(100)]
    public required string FullName { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public required string Email { get; set; }

    [Phone]
    [MaxLength(20)]
    public required string PhoneNumber { get; set; }

    [Required]
    [MaxLength(10)]
    public required string Gender { get; set; }

    [Required]
    [MaxLength(255)]
    public string? ProfileImageUrl { get; set; }

    [Required]
    public required string PasswordHash { get; set; }
    
    public string? Specialization { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;

    public DateTime LastLogin { get; set; }

    [ForeignKey("Role")]
    public int RoleId { get; set; }
    public Role? Role { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
}

}