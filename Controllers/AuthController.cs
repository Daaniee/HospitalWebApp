using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hospitalwebapp.Models;
using Microsoft.AspNetCore.Identity;

namespace hospitalwebapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<object> _passwordHasher;

        public AuthController(AppDbContext context, IPasswordHasher<object> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] dynamic body)
        {
            string email = body.email;
            string password = body.password;

            // 🔹 Staff login
            var staff = await _context.Staff
                .Include(s => s.Role)
                .FirstOrDefaultAsync(s => s.Email == email && !s.IsDeleted);

            if (staff != null)
            {
                var result = _passwordHasher.VerifyHashedPassword(staff, staff.PasswordHash, password);
                if (result == PasswordVerificationResult.Success && staff.IsActive)
                {
                    staff.LastLogin = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    // Store staff info in session
                    HttpContext.Session.SetInt32("StaffId", staff.Id);
                    HttpContext.Session.SetString("Role", staff.Role?.Name ?? "Unknown");

                    return Ok(new {
                        message = "Login successful",
                        role = staff.Role?.Name,
                        staff.Id,
                        staff.FullName,
                        staff.Email
                    });
                }
            }

            // 🔹 Patient login
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Email == email && !p.IsDeleted);

            if (patient != null)
            {
                var result = _passwordHasher.VerifyHashedPassword(patient, patient.PasswordHash, password);
                if (result == PasswordVerificationResult.Success)
                {
                    // Store patient info in session
                    HttpContext.Session.SetInt32("PatientId", patient.Id);
                    HttpContext.Session.SetString("Role", "Patient");

                    return Ok(new {
                        message = "Login successful",
                        role = "Patient",
                        patient.Id,
                        patient.FullName,
                        patient.Email
                    });
                }
            }

            return Unauthorized(new { message = "Invalid credentials" });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out successfully" });
        }
    }
}
