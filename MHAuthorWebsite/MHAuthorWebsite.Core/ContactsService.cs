using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Configuration.EmailConfiguration.Contracts;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Contacts;
using MHAuthorWebsite.Core.Extensions;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using Microsoft.AspNetCore.Identity;
using System.Text;
using static MHAuthorWebsite.GCommon.ApplicationRules.Roles;

namespace MHAuthorWebsite.Core;

public class ContactsService : IContactsService
{
    protected readonly IEmailService EmailService;
    protected readonly IEmailUserProvider EmailUserProvider;
    protected readonly IApplicationRepository Repository;
    protected readonly UserManager<ApplicationUser> UserManager;

    public ContactsService(IEmailService emailService, IEmailUserProvider emailUserProvider,
        IApplicationRepository repository, UserManager<ApplicationUser> userManager)
    {
        EmailService = emailService;
        EmailUserProvider = emailUserProvider;
        Repository = repository;
        UserManager = userManager;
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

        string[] adminEmails = (await UserManager.GetUsersInRoleAsync(AdminRoleName)).Where(u => !u.IsDeleted).Select(u => u.Email).ToArray()!;
        string body = new StringBuilder()
            .AppendLine("Ново запитване")
            .AppendLine($"Лице: {model.Name}")
            .AppendLine($"Имейл: {model.Email}")
            .AppendLine($"Тема: {model.Subject}")
            .AppendLine()
            .AppendLine("Съобщение:")
            .AppendLine(model.Message)
            .ToString()
            .Trim();

        IEnumerable<Task> sendTasks = adminEmails
            .Select(adminEmail => EmailService
                .SendEmailAsync(emailUser, adminEmail, "Ново запитване", body, false));

        await Task.WhenAll(sendTasks);

        return ServiceResult.Ok();
    }
}