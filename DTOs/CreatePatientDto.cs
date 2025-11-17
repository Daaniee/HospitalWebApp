using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace hospitalwebapp.DTOs
{
    public class CreatePatientDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string PasswordHash { get; set; } = string.Empty;

        [Range(0, 120)]
        public int Age { get; set; }

        [MaxLength(255)]
        public required string Address { get; set; }

        public required string BloodType { get; set; }
        public required int PhoneNumber { get; set; }
        public required string Gender { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? EmergencyContact { get; set; }
        public required string Genotype { get; set; }
        
    }
}