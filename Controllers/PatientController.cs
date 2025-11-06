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
    public class PatientController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuditLogger _audit;

        public PatientController(AppDbContext context, IAuditLogger audit)
        {
            _context = context;
            _audit = audit;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllPatients()
        {
            var patients = await _context.Patients
                .Where(p => !p.IsDeleted)
                .Select(p => new
                {
                    p.Id,
                    p.CardNumber,
                    p.FullName,
                    p.Age,
                    p.Address,
                    p.BloodType,
                    p.CreatedAt
                })
                .ToListAsync();


            return Ok(new ApiResponse<object>(true, 200, "Patients retrieved", patients));
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchPatients([FromQuery] PatientSearchDto filter)
        {
            var query = _context.Patients.AsQueryable();

            // Filtering
            if (!string.IsNullOrWhiteSpace(filter.Name))
                query = query.Where(p => p.FullName.ToLower().Contains(filter.Name.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.CardNumber))
                query = query.Where(p => p.CardNumber.ToLower().Contains(filter.CardNumber.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.BloodType))
                query = query.Where(p => p.BloodType.ToLower() == filter.BloodType.ToLower());

            if (filter.AgeMin.HasValue)
                query = query.Where(p => p.Age >= filter.AgeMin.Value);

            if (filter.AgeMax.HasValue)
                query = query.Where(p => p.Age <= filter.AgeMax.Value);

            if (filter.CreatedAfter.HasValue)
                query = query.Where(p => p.CreatedAt >= filter.CreatedAfter.Value);

            if (filter.CreatedBefore.HasValue)
                query = query.Where(p => p.CreatedAt <= filter.CreatedBefore.Value);

            // Sorting
            if (!string.IsNullOrWhiteSpace(filter.SortBy))
            {
                bool ascending = filter.SortOrder?.ToLower() == "asc";
                query = filter.SortBy.ToLower() switch
                {
                    "fullname" => ascending ? query.OrderBy(p => p.FullName) : query.OrderByDescending(p => p.FullName),
                    "age" => ascending ? query.OrderBy(p => p.Age) : query.OrderByDescending(p => p.Age),
                    "createdat" => ascending ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt),
                    _ => query.OrderByDescending(p => p.CreatedAt)
                };
            }

            // Pagination
            int skip = (filter.Page - 1) * filter.PageSize;
            var results = await query.Skip(skip).Take(filter.PageSize)
                .Where(p => !p.IsDeleted)
                .Select(p => new
                {
                    p.Id,
                    p.CardNumber,
                    p.FullName,
                    p.Age,
                    p.Address,
                    p.BloodType,
                    p.CreatedAt
                })
                .ToListAsync();

            return Ok(new ApiResponse<object>(true, 200, "Filtered patients retrieved", results));
        }


        private async Task<string> GenerateNextCardNumber()
        {
            var lastCard = await _context.Patients
                .OrderByDescending(p => p.Id)
                .Select(p => p.CardNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastCard) && lastCard.StartsWith("P-") &&
                int.TryParse(lastCard.Substring(2), out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }

            return $"P-{nextNumber:D4}";
        }

        [RequirePermission("RegisterPatient")]
        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] CreatePatientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseNoData(false, 400, "Invalid patient data"));

            var cardNumber = await GenerateNextCardNumber();

            var patient = new Patient
            {
                CardNumber = cardNumber,
                FullName = dto.FullName,
                Age = dto.Age,
                Address = dto.Address,
                BloodType = dto.BloodType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            var staffId = HttpContext.Session?.GetInt32("StaffId");

            _audit.Log(
                action: "CreatePatient",
                staffId: staffId,
                targetPatientId: patient.Id,
                details: $"Patient {patient.FullName} created with card number {cardNumber}"
            );

            return Ok(new ApiResponse<object>(true, 201, "Patient created", new
            {
                patient.Id,
                patient.CardNumber
            }));
        }

        [RequirePermission("EditPatientInfo")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] UpdatePatientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseNoData(false, 400, "Invalid patient data"));

            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
                return NotFound(new ApiResponseNoData(false, 404, "Patient not found"));

            // Capture original values for audit
            var original = new
            {
                patient.FullName,
                patient.Age,
                patient.Address,
                patient.BloodType
            };

            // Apply partial updates
            if (dto.FullName != null) patient.FullName = dto.FullName;
            if (dto.Age.HasValue) patient.Age = dto.Age.Value;
            if (dto.Address != null) patient.Address = dto.Address;
            if (dto.BloodType != null) patient.BloodType = dto.BloodType;

            patient.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var staffId = HttpContext.Session?.GetInt32("StaffId");

            _audit.Log(
                action: "UpdatePatient",
                staffId: staffId,
                targetPatientId: patient.Id,
                details: $"Updated patient {patient.CardNumber}. Before: {System.Text.Json.JsonSerializer.Serialize(original)}"
            );

            return Ok(new ApiResponseNoData(true, 200, "Patient updated successfully"));
        }

        [RequirePermission("DeletePatient")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeletePatient(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
                return NotFound(new ApiResponseNoData(false, 404, "Patient not found"));

            if (patient.IsDeleted)
                return BadRequest(new ApiResponseNoData(false, 400, "Patient already deleted"));

            patient.IsDeleted = true;
            patient.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var staffId = HttpContext.Session?.GetInt32("StaffId");

            _audit.Log(
                action: "DeletePatient",
                staffId: staffId,
                targetPatientId: patient.Id,
                details: $"Soft-deleted patient {patient.CardNumber} ({patient.FullName})"
            );

            return Ok(new ApiResponseNoData(true, 200, "Patient deleted successfully"));
        }

        [RequirePermission("RestorePatient")]
        [HttpPatch("{id}/restore")]
        public async Task<IActionResult> RestorePatient(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
                return NotFound(new ApiResponseNoData(false, 404, "Patient not found"));

            if (!patient.IsDeleted)
                return BadRequest(new ApiResponseNoData(false, 400, "Patient is already active"));

            patient.IsDeleted = false;
            patient.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var staffId = HttpContext.Session?.GetInt32("StaffId");

            _audit.Log(
                action: "RestorePatient",
                staffId: staffId,
                targetPatientId: patient.Id,
                details: $"Restored patient {patient.CardNumber} ({patient.FullName})"
            );

            return Ok(new ApiResponseNoData(true, 200, "Patient restored successfully"));
        }


    }
}