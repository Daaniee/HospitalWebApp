using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace hospitalwebapp.DTOs
{
    public class UpdatePatientDto
    {
        [MaxLength(100)]
        public string? FullName { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(100)]
        public string? PasswordHash { get; set; }

        [Range(0, 120)]
        public int? Age { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        public string? BloodType { get; set; }
        public int? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? EmergencyContact { get; set; }
        public string? Genotype { get; set; }
    }
}