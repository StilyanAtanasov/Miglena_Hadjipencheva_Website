namespace MHAuthorWebsite.Web.ViewModels.ProductComment;

public class StarsInputViewModel
{
    public short MaxValue { get; set; }

    public string InputName { get; set; } = null!;

    public byte Rating { get; set; } = 0;
}