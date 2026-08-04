using MHAuthorWebsite.Core.Models.Enums;

namespace MHAuthorWebsite.Core.Dtos.ProductComment;

public class ReactToCommentDto
{
    public Guid CommentId { get; set; }

    public CommentReaction ReactionType { get; set; }
}