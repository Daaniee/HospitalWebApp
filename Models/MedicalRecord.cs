using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hospitalwebapp.Models
{
    public class MedicalRecord
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Patient")]
        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }
        public Staff Doctor { get; set; }

        public DateTime VisitDate { get; set; }

        public DateTime CheckInTime { get; set; }
        public DateTime CheckOutTime { get; set; }

        [MaxLength(500)]
        public string Symptoms { get; set; }

        [MaxLength(500)]
        public string Diagnosis { get; set; }

        [MaxLength(500)]
        public string Prescription { get; set; }

        [MaxLength(1000)]
        public string Notes { get; set; }

        public DateTime? FollowUpDate { get; set; }

        [Required]
        public VisitStatus Status { get; set; } = VisitStatus.Pending;


        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}