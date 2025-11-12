namespace MHAuthorWebsite.Web.ViewModels.Admin.UserManagement;

public class UserDetailsViewModel
{
    public string Id { get; set; } = null!;

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public bool IsActive { get; set; }

    public bool IsAdmin { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsBanned { get; set; }

    public DateTime DateJoined { get; set; }

    public DateTime LastActive { get; set; }
}