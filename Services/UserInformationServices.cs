using hospitalwebapp.Models;
using Microsoft.EntityFrameworkCore;

public class UserInformationServices : UserInformationInterface
{
    private readonly AppDbContext _context;

    public UserInformationServices(AppDbContext context)
    {
        _context = context;
    }

    public Staff GetCurrentUserId(int staffId)
    {
        var staff = _context.Staff
            .Include(s => s.Role)
            .FirstOrDefault(s => s.Id == staffId);

        return staff;
    }

    public ApiResponseNoData CreateStaffs(Staff values)
    {
        _context.Staff.Add(values);
        _context.SaveChanges();

        return new ApiResponseNoData
        {
            Success = true,
            Message = "Staff created successfully."
        };
    }
}