using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hospitalwebapp.Models
{
    public class Patient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string CardNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        [Range(0, 120)]
        public int Age { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        public string BloodType { get; set; } = string.Empty;
        public int PhoneNumber { get; set; } = 0;
        public string Gender { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public string? EmergencyContact { get; set; }
        public string Genotype { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;


        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    }
}
