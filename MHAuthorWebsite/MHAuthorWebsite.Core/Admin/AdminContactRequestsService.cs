using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.EmailConfiguration.Contracts;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.Admin.ContactRequests;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static MHAuthorWebsite.GCommon.ApplicationRules.ContactRequestsBoard;

namespace MHAuthorWebsite.Core.Admin;

public class AdminContactRequestsService : ContactsService, IAdminContactRequestsService
{
    public AdminContactRequestsService(IEmailService emailService, IEmailUserProvider emailUserProvider,
        IApplicationRepository repository, UserManager<ApplicationUser> userManager)
        : base(emailService, emailUserProvider, repository, userManager) { }

    public async Task<ICollection<ContactRequestCardViewModel>> GetContactRequestsPagedReadonlyAsync(int page)
        => await Repository
            .AllReadonly<ContactRequest>()
            .OrderByDescending(cr => cr.CreatedOn)
            .Skip((page - 1) * RequestsPerPage)
            .Take(RequestsPerPage)
            .Select(cr => new ContactRequestCardViewModel
            {
                Id = cr.Id,
                Name = cr.Name,
                Email = cr.Email,
                Subject = cr.Subject,
                MessagePreview = cr.Message.Length <= MaxMessageLength ? cr.Message : cr.Message.Substring(0, MaxMessageLength) + "...",
                CreatedOn = cr.CreatedOn,
                IsAnswered = cr.ReplyMessage != null
            })
            .ToArrayAsync();

    public async Task<ServiceResult<ContactRequestDetailsViewModel>> GetContactRequestDetailsReadonlyAsync(Guid requestId)
    {
        ContactRequest? contactRequest = await Repository
            .AllReadonly<ContactRequest>()
            .FirstOrDefaultAsync(cr => cr.Id == requestId);
        if (contactRequest is null) return ServiceResult<ContactRequestDetailsViewModel>.NotFound();

        ContactRequestDetailsViewModel viewModel = new ContactRequestDetailsViewModel
        {
            Id = contactRequest.Id,
            Name = contactRequest.Name,
            Email = contactRequest.Email,
            Subject = contactRequest.Subject,
            Message = contactRequest.Message,
            CreatedOn = contactRequest.CreatedOn,
            RepliedOn = contactRequest.RepliedOn,
            ReplyMessage = contactRequest.ReplyMessage ?? "",
            IsAnswered = contactRequest.ReplyMessage != null
        };

        return ServiceResult<ContactRequestDetailsViewModel>.Ok(viewModel);
    }

    public async Task<ServiceResult> ReplyToContactRequestAsync(Guid requestId, string replyMessage, string adminId)
    {
        ContactRequest? contactRequest = await Repository
            .All<ContactRequest>()
            .FirstOrDefaultAsync(cr => cr.Id == requestId);
        if (contactRequest is null) return ServiceResult.NotFound();

        contactRequest.ReplyMessage = replyMessage;
        contactRequest.RepliedOn = DateTime.Now;
        contactRequest.AdminId = adminId;

        await Repository.SaveChangesAsync();

        await EmailService.SendEmailAsync(
            to: contactRequest.Email,
            subject: $"Re: {contactRequest.Subject}",
            body: replyMessage.Trim(),
            isBodyHtml: false,
            from: EmailUserProvider.GetContactUser()
        );

        return ServiceResult.Ok();
    }
}