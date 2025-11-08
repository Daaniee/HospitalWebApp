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
        public string FullName { get; set; }

        [Range(0, 120)]
        public int Age { get; set; }

        [MaxLength(255)]
        public string Address { get; set; }

        public string BloodType { get; set; }
    }
}