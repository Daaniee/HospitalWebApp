using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hospitalwebapp.Attributes;
using hospitalwebapp.DTOs;
using hospitalwebapp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hospitalwebapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicalRecordController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogger _audit;

        public MedicalRecordController(AppDbContext context, IAuditLogger audit)
        {
            _context = context;
            _audit = audit;
        }


        // [RequirePermission("AssignDoctor")]
        [HttpGet("available-doctors")]
        public async Task<IActionResult> GetAvailableDoctors()
        {
            var doctors = await _context.AvailableDoctors
                .Include(d => d.Doctor)
                .Where(d => d.IsAvailable)
                .Select(d => new
                {
                    d.Doctor.Id,
                    d.Doctor.FullName,
                    d.Doctor.Specialization,
                    d.UpdatedAt
                })
                .ToListAsync();

            return Ok(new ApiResponse<object>(true, 200, "Available doctors", doctors));
        }

        // [RequirePermission("ScheduleAppointment")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMedicalRecordDto dto)
        {
            var patient = await _context.Patients.FindAsync(dto.PatientId);
            if (patient == null || patient.IsDeleted)
                return NotFound(new ApiResponseNoData(false, 404, "Patient not found"));

            var doctor = await _context.Staff.FindAsync(dto.DoctorId);
            if (doctor == null)
                return NotFound(new ApiResponseNoData(false, 404, "Doctor not found"));

            var record = new MedicalRecord
            {
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                VisitDate = dto.VisitDate,
                Symptoms = dto.Symptoms ?? string.Empty,
                Diagnosis = dto.Diagnosis ?? string.Empty,
                Prescription = dto.Prescription ?? string.Empty,
                Notes = dto.Notes ?? string.Empty,
                FollowUpDate = dto.FollowUpDate,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.MedicalRecords.Add(record);
            await _context.SaveChangesAsync();

            var staffId = HttpContext.Session?.GetInt32("StaffId");

            _audit.Log(
                action: "CreateMedicalRecord",
                staffId: staffId,
                targetPatientId: dto.PatientId,
                targetStaffId: dto.DoctorId,
                details: $"Created medical record for patient {patient.CardNumber} by Dr. {doctor.FullName}"
            );

            return Ok(new ApiResponse<object>(true, 201, "Medical record created", new { record.Id }));
        }

        // [RequirePermission("RegisterPatient")]
        [HttpPatch("{id}/checkin")]
        public async Task<IActionResult> CheckIn(int id)
        {
            var record = await _context.MedicalRecords.FindAsync(id);
            if (record == null)
                return NotFound(new ApiResponseNoData(false, 404, "Medical record not found"));

            if (record.CheckInTime.HasValue)
                return BadRequest(new ApiResponseNoData(false, 400, "Patient already checked in"));

            record.CheckInTime = DateTime.UtcNow;
            record.Status = VisitStatus.Pending; // still pending until checkout
            record.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var staffId = HttpContext.Session?.GetInt32("StaffId");
            _audit.Log(
                action: "CheckIn",
                staffId: staffId,
                targetPatientId: record.PatientId,
                targetStaffId: record.DoctorId,
                details: $"Patient checked in for visit {record.Id}"
            );

            return Ok(new ApiResponseNoData(true, 200, "Patient checked in successfully"));
        }

        // [RequirePermission("RegisterPatient")]
        [HttpPatch("{id}/checkout")]
        public async Task<IActionResult> CheckOut(int id)
        {
            var record = await _context.MedicalRecords.FindAsync(id);
            if (record == null)
                return NotFound(new ApiResponseNoData(false, 404, "Medical record not found"));

            if (!record.CheckInTime.HasValue)
                return BadRequest(new ApiResponseNoData(false, 400, "Patient has not checked in"));

            if (record.CheckOutTime.HasValue)
                return BadRequest(new ApiResponseNoData(false, 400, "Patient already checked out"));

            record.CheckOutTime = DateTime.UtcNow;
            record.Status = VisitStatus.Completed;
            record.UpdatedAt = DateTime.UtcNow;

            // Free up the doctor
            var availability = await _context.AvailableDoctors
                .FirstOrDefaultAsync(d => d.DoctorId == record.DoctorId);

            if (availability != null)
            {
                availability.IsAvailable = true;
                availability.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var staffId = HttpContext.Session?.GetInt32("StaffId");
            _audit.Log(
                action: "CheckOut",
                staffId: staffId,
                targetPatientId: record.PatientId,
                targetStaffId: record.DoctorId,
                details: $"Patient checked out for visit {record.Id} at {record.CheckOutTime}"
            );

            return Ok(new ApiResponseNoData(true, 200, "Patient checked out successfully"));
        }

        // [RequirePermission("ViewMedicalRecord")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var records = await _context.MedicalRecords
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.VisitDate)
                .Select(r => new
                {
                    r.Id,
                    Patient = new
                    {
                        r.Patient.Id,
                        r.Patient.FullName,
                        r.Patient.CardNumber
                    },
                    Doctor = new
                    {
                        r.Doctor.Id,
                        r.Doctor.FullName,
                        r.Doctor.Specialization
                    },
                    r.VisitDate,
                    r.CheckInTime,
                    r.CheckOutTime,
                    VisitDuration = (r.CheckInTime.HasValue && r.CheckOutTime.HasValue)
                        ? (double?)((r.CheckOutTime.Value - r.CheckInTime.Value).TotalMinutes)
                        : null,
                    r.Status,
                    r.FollowUpDate,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(new ApiResponse<object>(true, 200, "All medical records", records));
        }

        // [RequirePermission("ViewMedicalRecord")]
        [HttpPost("patient/{patientId}")]
        public async Task<IActionResult> GetByPatient(int patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null || patient.IsDeleted)
                return NotFound(new ApiResponseNoData(false, 404, "Patient not found"));

            var records = await _context.MedicalRecords
                .Include(r => r.Doctor)
                .Where(r => r.PatientId == patientId)
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.VisitDate)
                .Select(r => new
                {
                    r.Id,
                    Doctor = new
                    {
                        r.Doctor.Id,
                        r.Doctor.FullName,
                        r.Doctor.Specialization
                    },
                    r.VisitDate,
                    r.CheckInTime,
                    r.CheckOutTime,
                    VisitDuration = (r.CheckInTime.HasValue && r.CheckOutTime.HasValue)
                        ? (double?)((r.CheckOutTime.Value - r.CheckInTime.Value).TotalMinutes)
                        : null,
                    r.Symptoms,
                    r.Diagnosis,
                    r.Prescription,
                    r.Notes,
                    r.FollowUpDate,
                    r.Status,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(new ApiResponse<object>(true, 200, "Medical records for patient", records));
        }

        // [RequirePermission("EditMedicalRecord")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMedicalRecordDto dto)
        {
            var record = await _context.MedicalRecords.FindAsync(id);
            if (record == null)
                return NotFound(new ApiResponseNoData(false, 404, "Medical record not found"));

            record.Symptoms = dto.Symptoms ?? record.Symptoms;
            record.Diagnosis = dto.Diagnosis ?? record.Diagnosis;
            record.Prescription = dto.Prescription ?? record.Prescription;
            record.Notes = dto.Notes ?? record.Notes;
            record.FollowUpDate = dto.FollowUpDate ?? record.FollowUpDate;
            record.Status = dto.Status ?? record.Status;
            record.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var staffId = HttpContext.Session?.GetInt32("StaffId");
            _audit.Log(
                action: "UpdateMedicalRecord",
                staffId: staffId,
                targetPatientId: record.PatientId,
                targetStaffId: record.DoctorId,
                details: $"Updated medical record {record.Id} for patient {record.PatientId}"
            );

            return Ok(new ApiResponseNoData(true, 200, "Medical record updated successfully"));
        }

        // [RequirePermission("DeleteMedicalRecord")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var record = await _context.MedicalRecords.FindAsync(id);
            if (record == null || record.IsDeleted)
                return NotFound(new ApiResponseNoData(false, 404, "Medical record not found"));

            record.IsDeleted = true;
            record.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var staffId = HttpContext.Session?.GetInt32("StaffId");
            _audit.Log(
                action: "SoftDeleteMedicalRecord",
                staffId: staffId,
                targetPatientId: record.PatientId,
                targetStaffId: record.DoctorId,
                details: $"Soft-deleted medical record {record.Id}"
            );

            return Ok(new ApiResponseNoData(true, 200, "Medical record deleted successfully"));
        }

        // [RequirePermission("RestoreMedicalRecord")]
        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            var record = await _context.MedicalRecords.FindAsync(id);
            if (record == null)
                return NotFound(new ApiResponseNoData(false, 404, "Medical record not found"));

            if (!record.IsDeleted)
                return BadRequest(new ApiResponseNoData(false, 400, "Record is not deleted"));

            record.IsDeleted = false;
            record.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var staffId = HttpContext.Session?.GetInt32("StaffId");
            _audit.Log(
                action: "RestoreMedicalRecord",
                staffId: staffId,
                targetPatientId: record.PatientId,
                targetStaffId: record.DoctorId,
                details: $"Restored medical record {record.Id}"
            );

            return Ok(new ApiResponseNoData(true, 200, "Medical record restored successfully"));
        }

        // [RequirePermission("ViewMedicalRecord")]
        [HttpPost("search")]
        public async Task<IActionResult> Search(
            int? patientId,
            int? doctorId,
            VisitStatus? status,
            DateTime? dateFrom,
            DateTime? dateTo
        )
        {
            var query = _context.MedicalRecords
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Where(r => !r.IsDeleted);

            if (patientId.HasValue)
                query = query.Where(r => r.PatientId == patientId.Value);

            if (doctorId.HasValue)
                query = query.Where(r => r.DoctorId == doctorId.Value);

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            if (dateFrom.HasValue)
                query = query.Where(r => r.VisitDate >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(r => r.VisitDate <= dateTo.Value);

            var records = await query
                .OrderByDescending(r => r.VisitDate)
                .Select(r => new
                {
                    r.Id,
                    Patient = new
                    {
                        r.Patient.Id,
                        r.Patient.FullName,
                        r.Patient.CardNumber
                    },
                    Doctor = new
                    {
                        r.Doctor.Id,
                        r.Doctor.FullName,
                        r.Doctor.Specialization
                    },
                    r.VisitDate,
                    r.CheckInTime,
                    r.CheckOutTime,
                    VisitDuration = r.CheckInTime.HasValue && r.CheckOutTime.HasValue
                        ? (double?)((r.CheckOutTime.Value - r.CheckInTime.Value).TotalMinutes)
                        : null,
                    r.Status,
                    r.FollowUpDate,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(new ApiResponse<object>(true, 200, "Filtered medical records", records));
        }

        // [RequirePermission("ViewDeletedMedicalRecord")]
        [HttpGet("deleted")]
        public async Task<IActionResult> GetDeleted()
        {
            var records = await _context.MedicalRecords
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Where(r => r.IsDeleted)   // ✅ only deleted
                .OrderByDescending(r => r.VisitDate)
                .Select(r => new
                {
                    r.Id,
                    Patient = new
                    {
                        r.Patient.Id,
                        r.Patient.FullName,
                        r.Patient.CardNumber
                    },
                    Doctor = new
                    {
                        r.Doctor.Id,
                        r.Doctor.FullName,
                        r.Doctor.Specialization
                    },
                    r.VisitDate,
                    r.CheckInTime,
                    r.CheckOutTime,
                    VisitDuration = r.CheckInTime.HasValue && r.CheckOutTime.HasValue
                        ? (double?)((r.CheckOutTime.Value - r.CheckInTime.Value).TotalMinutes)
                        : null,
                    r.Status,
                    r.FollowUpDate,
                    r.CreatedAt,
                    r.UpdatedAt
                })
                .ToListAsync();

            return Ok(new ApiResponse<object>(true, 200, "Deleted medical records", records));
        }


    }


}

