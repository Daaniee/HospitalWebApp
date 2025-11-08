using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using hospitalwebapp.Models;

namespace hospitalwebapp.DTOs
{
    public class CreateMedicalRecordDto
    {
        public int PatientId { get; set; }
        public int DoctorId { get; set; }

        public DateTime VisitDate { get; set; }

        public DateTime? FollowUpDate { get; set; }

        [MaxLength(500)]
        public string? Symptoms { get; set; }

        [MaxLength(500)]
        public string? Diagnosis { get; set; }

        [MaxLength(500)]
        public string? Prescription { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public VisitStatus Status { get; set; } = VisitStatus.Pending;
    }


}