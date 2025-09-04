namespace MHAuthorWebsite.Core.Dto;

public class CustomerInfoDto
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Face { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? CountryCode { get; set; }

    public string? CityName { get; set; }

    public string? PostCode { get; set; }

    public string? OfficeCode { get; set; }

    public string? ZipCode { get; set; }

    public string? Address { get; set; }

    public string? PriorityFrom { get; set; }

    public string? PriorityTo { get; set; }
}