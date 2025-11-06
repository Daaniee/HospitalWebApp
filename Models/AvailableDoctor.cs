using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hospitalwebapp.Models
{
public class AvailableDoctor
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }
        public Staff Doctor { get; set; }

        public bool IsAvailable { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
