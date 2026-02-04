using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Configuration.EmailConfiguration.Contracts;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Admin.ContactRequests;
using MHAuthorWebsite.Core.Extensions;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using Microsoft.AspNetCore.Identity;
using static MHAuthorWebsite.GCommon.ApplicationRules.Application;
using static MHAuthorWebsite.GCommon.ApplicationRules.ContactRequestsBoard;
using static MHAuthorWebsite.GCommon.ApplicationRules.Emails;

namespace MHAuthorWebsite.Core.Admin;

public class AdminContactRequestsService : ContactsService, IAdminContactRequestsService
{
    public AdminContactRequestsService(IEmailService emailService, IEmailUserProvider emailUserProvider,
        IApplicationRepository repository, UserManager<ApplicationUser> userManager, IUrlProvider urlProvider,
        IServiceProvider serviceProvider)
        : base(emailService, emailUserProvider, repository, userManager, urlProvider, serviceProvider) { }

    public async Task<ICollection<ContactRequestCardDto>> GetContactRequestsPagedReadonlyAsync(int page)
        => await Repository
            .AllReadonly<ContactRequest>()
            .OrderByDescending(cr => cr.CreatedOn)
            .Skip((page - 1) * RequestsPerPage)
            .Take(RequestsPerPage)
            .Select(cr => new ContactRequestCardDto
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

    public async Task<ServiceResult<ContactRequestDetailsDto>> GetContactRequestDetailsReadonlyAsync(Guid requestId)
    {
        ContactRequest? contactRequest = await Repository
            .WhereReadonly<ContactRequest>(cr => cr.Id == requestId)
            .FirstOrDefaultAsync();
        if (contactRequest is null) return ServiceResult<ContactRequestDetailsDto>.NotFound();

        ContactRequestDetailsDto dto = new ContactRequestDetailsDto
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

        return ServiceResult<ContactRequestDetailsDto>.Ok(dto);
    }

    public async Task<ServiceResult> ReplyToContactRequestAsync(Guid requestId, string replyMessage, string adminId)
    {
        ContactRequest? contactRequest = await Repository
            .Where<ContactRequest>(cr => cr.Id == requestId)
            .FirstOrDefaultAsync();
        if (contactRequest is null) return ServiceResult.NotFound();

        string htmlBody = $@"
            <!DOCTYPE html>
            <html lang=""bg"">
            <head>
                <meta charset=""UTF-8"">
                <style>
                    .content-text {{ line-height: 1.6; color: #181717; font-size: 16px; }}
                    .quote-box {{ border-left: 4px solid #f8dff8; padding-left: 15px; margin: 20px 0; color: #616161; font-style: italic; }}
                </style>
            </head>
            <body style=""margin: 0; padding: 0; background-color: #fcfcfc; font-family: 'Segoe UI', Arial, sans-serif;"">
                <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"" style=""background-color: #fcfcfc;"">
                    <tr>
                        <td align=""center"" style=""padding: 20px 0;"">
                            <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""600"" style=""background-color: #ffffff; border: 1px solid #f0f0f0; border-radius: 8px;"">
                                <tr>
                                    <td style=""padding: 20px; background-color: #3a053a; border-radius: 8px 8px 0 0; text-align: center;"">
                                        <span style=""color: #fcfcfc; font-size: 20px; font-weight: bold; letter-spacing: 1px;"">
                                            Отговор на Вашето запитване
                                        </span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 40px 30px;"">
                                        <div class=""content-text"">
                                            <p> {ContactRequestReplyGreeting}</p>
                                            <p>{replyMessage.Trim().Replace("\n", "<br>")}</p>
                                        </div>

                                        <div class=""quote-box"">
                                            <p style=""margin: 0; font-size: 14px;"">
                                                <strong>Относно Вашето запитване: ""{contactRequest.Subject}""</strong><br>
                                                ""{contactRequest.Message}""
                                            </p>
                                        </div>

                                        <p style=""font-size: 14px; color: #999; margin-top: 30px;"">
                                           {ContactRequestReplyRegardsInnerHtml}
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 20px; background-color: #fbf3fb; text-align: center; font-size: 12px; color: #616161; border-radius: 0 0 8px 8px;"">
                                        <p style=""margin: 0;"">Този имейл е изпратен във връзка с Вашето запитване през нашата контактна форма.</p>
                                        <p style=""margin: 5px 0 0 0;"">&copy; {DateTime.UtcNow.Year} {WebsiteName}</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";

        await EmailService.SendEmailAsync(
            to: contactRequest.Email,
            subject: $"Re: {contactRequest.Subject}",
            body: htmlBody,
            isBodyHtml: true,
            from: EmailUserProvider.GetContactUser()
        );

        contactRequest.ReplyMessage = replyMessage;
        contactRequest.RepliedOn = DateTime.UtcNow;
        contactRequest.AdminId = adminId;

        await Repository.SaveChangesAsync();

        return ServiceResult.Ok();
    }
}