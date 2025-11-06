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
        public string CardNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Range(0, 120)]
        public int Age { get; set; }

        [MaxLength(255)]
        public string Address { get; set; }

        [MaxLength(5)]
        public string BloodType { get; set; }

        public bool IsDeleted { get; set; } = false;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<MedicalRecord> MedicalRecords { get; set; }
    }
}
