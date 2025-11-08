using System.ComponentModel.DataAnnotations;
using hospitalwebapp.Models;

public class UpdateMedicalRecordDto
{
    [MaxLength(500)]
    public string? Symptoms { get; set; }

    [MaxLength(500)]
    public string? Diagnosis { get; set; }

    [MaxLength(500)]
    public string? Prescription { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime? FollowUpDate { get; set; }

    public VisitStatus? Status { get; set; }
}
