namespace MHAuthorWebsite.Core.Dtos.Admin.ContactRequests;

public class ContactRequestCardDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string MessagePreview { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public bool IsAnswered { get; set; }
}