using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Configuration.EmailConfiguration.Contracts;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Contacts;
using MHAuthorWebsite.Core.Extensions;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using Microsoft.AspNetCore.Identity;
using static MHAuthorWebsite.GCommon.ApplicationRules.Roles;

namespace MHAuthorWebsite.Core;

public class ContactsService : IContactsService
{
    protected readonly IEmailService EmailService;
    protected readonly IEmailUserProvider EmailUserProvider;
    protected readonly IApplicationRepository Repository;
    protected readonly UserManager<ApplicationUser> UserManager;
    protected readonly IUrlProvider UrlProvider;

    public ContactsService(IEmailService emailService, IEmailUserProvider emailUserProvider,
        IApplicationRepository repository, UserManager<ApplicationUser> userManager, IUrlProvider urlProvider)
    {
        EmailService = emailService;
        EmailUserProvider = emailUserProvider;
        Repository = repository;
        UserManager = userManager;
        UrlProvider = urlProvider;
    }

    public async Task<ServiceResult> SendContactMessageAsync(SendContactMessageDto model, string? userId)
    {
        IEmailUser emailUser = EmailUserProvider.GetContactUser();

        ApplicationUser? user = userId is not null ?
            await UserManager.Users.Where(u => u.Id == userId && !u.IsDeleted).FirstOrDefaultAsync()
            : null;
        if (user is null && userId is not null) return ServiceResult.Forbidden();

        ContactRequest contactRequest = new ContactRequest
        {
            Name = model.Name,
            Email = model.Email,
            Subject = model.Subject,
            Message = model.Message,
            CreatedOn = DateTime.UtcNow,
            UserId = user?.Id
        };

        await Repository.AddAsync(contactRequest);
        await Repository.SaveChangesAsync();

        string requestDetailsUrl = UrlProvider.GetContactRequestPageUrl(contactRequest.Id);

        try
        {
            string[] adminEmails = (await UserManager.GetUsersInRoleAsync(AdminRoleName)).Where(u => !u.IsDeleted).Select(u => u.Email).ToArray()!;
            string emailBody = $@"
                <!DOCTYPE html>
                <html>
                <body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, sans-serif; background-color: #f0f0f0;"">
                    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""padding: 20px;"">
                        <tr>
                            <td align=""center"">
                                <table role=""presentation"" width=""100%"" style=""max-width: 600px; background-color: #ffffff; border-radius: 10px; border: 1px solid #ddd; overflow: hidden;"">
                                    <tr>
                                        <td style=""background-color: #3a053a; padding: 15px; text-align: center;"">
                                            <h2 style=""color: #fcfcfc; margin: 0; font-size: 18px; text-transform: uppercase; letter-spacing: 1px;"">Ново запитване от сайта</h2>
                                        </td>
                                    </tr>
                                    
                                    <tr>
                                        <td style=""padding: 30px;"">
                                            <table role=""presentation"" width=""100%"" style=""border-collapse: collapse;"">
                                                <tr>
                                                    <td style=""padding: 10px 0; border-bottom: 1px solid #f0f0f0; width: 100px; font-weight: bold; color: #616161;"">Лице:</td>
                                                    <td style=""padding: 10px 0; border-bottom: 1px solid #f0f0f0; color: #181717;"">{model.Name}</td>
                                                </tr>
                                                <tr>
                                                    <td style=""padding: 10px 0; border-bottom: 1px solid #f0f0f0; font-weight: bold; color: #616161;"">Имейл:</td>
                                                    <td style=""padding: 10px 0; border-bottom: 1px solid #f0f0f0;"">
                                                        <a href=""mailto:{model.Email}"" style=""color: #2767e7; text-decoration: none;"">{model.Email}</a>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style=""padding: 10px 0; border-bottom: 1px solid #f0f0f0; font-weight: bold; color: #616161;"">Тема:</td>
                                                    <td style=""padding: 10px 0; border-bottom: 1px solid #f0f0f0; color: #181717;"">{model.Subject}</td>
                                                </tr>
                                            </table>

                                            <div style=""margin-top: 25px;"">
                                                <p style=""font-weight: bold; color: #616161; margin-bottom: 10px;"">Съобщение:</p>
                                                <div style=""background-color: #fbf3fb; padding: 15px; border-radius: 8px; color: #181717; line-height: 1.5; white-space: pre-wrap;"">
                                                    {model.Message}
                                                </div>
                                            </div>

                                            <div style=""text-align: center; margin-top: 30px;"">
                                                <a href=""{requestDetailsUrl}"" 
                                                   style=""background-color: #3a053a; color: #fcfcfc; padding: 12px 25px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;"">
                                                    Отговори директно
                                                </a>
                                            </div>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td style=""background-color: #f9f9f9; padding: 15px; text-align: center; font-size: 11px; color: #999;"">
                                            Това е автоматично известие, изпратено от системата на {DateTime.UtcNow:dd.MM.yyyy HH:mm} (UTC).
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";

            IEnumerable<Task> sendTasks = adminEmails
                .Select(adminEmail => EmailService
                    .SendEmailAsync(emailUser, adminEmail, $"Ново запитване от {model.Name}", emailBody, true));

            await Task.WhenAll(sendTasks);
        }
        catch
        {
            // TODO  Log email sending failure (not implemented here)
        }

        return ServiceResult.Ok();
    }
}