namespace MHAuthorWebsite.Core.Dtos.Admin.ContactRequests;

public class ContactRequestDetailsDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string Message { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public DateTime? RepliedOn { get; set; }

    public string ReplyMessage { get; set; } = null!;

    public bool IsAnswered { get; set; }
}