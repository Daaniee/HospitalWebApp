public class PatientSearchDto
{
    public string? Name { get; set; }
    public string? CardNumber { get; set; }
    public string? BloodType { get; set; }
    public int? AgeMin { get; set; }
    public int? AgeMax { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }

    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // Sorting
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortOrder { get; set; } = "desc"; // or "asc"
}
