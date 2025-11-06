using hospitalwebapp.Models;

public interface UserInformationInterface
{
    Staff GetCurrentUserId(int staffId);
    ApiResponseNoData CreateStaffs(Staff values);
}