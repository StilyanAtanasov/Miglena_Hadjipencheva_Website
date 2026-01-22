using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Configuration.EmailConfiguration.Contracts;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Error;
using MHAuthorWebsite.Core.Models;
using Microsoft.AspNetCore.Identity;
using static MHAuthorWebsite.GCommon.ApplicationRules.Roles;

namespace MHAuthorWebsite.Core;

public class ErrorService : IErrorService
{
    private readonly IEmailService _emailService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailUserProvider _emailUserProvider;

    public ErrorService(IEmailService emailService, UserManager<ApplicationUser> userManager, IEmailUserProvider emailUserProvider)
    {
        _emailService = emailService;
        _userManager = userManager;
        _emailUserProvider = emailUserProvider;
    }

    public async Task<ServiceResult> HandleErrorAsync(HandleErrorDto errorDto)
    {
        // TODO log to the logging provider 

        ServiceResult sr = await SendAdminsErrorNotificationAsync(errorDto.Exception, errorDto.Path, errorDto.Method, errorDto.RequestId, errorDto.UserNameIdentifier);
        if (!sr.Success)
            return ServiceResult.Failure();

        return ServiceResult.Ok();
    }

    private async Task<ServiceResult> SendAdminsErrorNotificationAsync(Exception? ex, string path, string method, string requestId, string userIdentifier)
    {
        string exMessage = ex?.Message ?? "Няма съобщение за грешка (възможно е директен код 500).";
        string exStack = ex?.StackTrace ?? "Няма наличен Stack Trace.";
        string stackSnippet = exStack.Length > 1500 ? exStack.Substring(0, 1500) + "..." : exStack;

        string adminErrorBody = $@"
        <!DOCTYPE html>
        <html>
        <body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, sans-serif; background-color: #fff5f5;"">
            <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""padding: 20px;"">
                <tr>
                    <td align=""center"">
                        <table role=""presentation"" width=""100%"" style=""max-width: 800px; background-color: #ffffff; border: 2px solid #e53e3e; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.1);"">
                            <tr>
                                <td style=""background-color: #e53e3e; padding: 20px;"">
                                    <h1 style=""color: #ffffff; margin: 0; font-size: 22px;"">⚠️ Критична сървърна грешка (500)</h1>
                                    <p style=""color: #ffcccc; margin: 5px 0 0 0; font-size: 14px;"">Време: {DateTime.UtcNow:dd.MM.yyyy HH:mm:ss} (UTC)</p>
                                </td>
                            </tr>
                            
                            <tr>
                                <td style=""padding: 20px; background-color: #f8f9fa;"">
                                    <table width=""100%"" style=""font-size: 14px; color: #4a5568;"">
                                        <tr><td style=""width: 120px;""><strong>URL:</strong></td><td>{path}</td></tr>
                                        <tr><td><strong>Метод:</strong></td><td>{method}</td></tr>
                                        <tr><td><strong>ID на заявка:</strong></td><td><code style=""background: #edf2f7; padding: 2px 4px;"">{requestId}</code></td></tr>
                                        <tr><td><strong>Потребител:</strong></td><td>{userIdentifier}</td></tr>
                                    </table>
                                </td>
                            </tr>

                            <tr>
                                <td style=""padding: 20px;"">
                                    <p style=""color: #e53e3e; font-weight: bold; margin-bottom: 10px;"">Съобщение на изключението:</p>
                                    <div style=""background-color: #1a202c; color: #cbd5e0; padding: 15px; border-radius: 5px; font-size: 12px; line-height: 1.4; overflow-x: auto; font-family: 'Consolas', monospace;"">
                                        {exMessage}
                                    </div>

                                    <p style=""color: #4a5568; font-weight: bold; margin: 20px 0 10px 0;"">Откъс от Stack Trace:</p>
                                    <pre style=""background-color: #edf2f7; color: #2d3748; padding: 15px; border-radius: 5px; font-size: 11px; white-space: pre-wrap; word-break: break-all; font-family: 'Consolas', monospace;"">{stackSnippet}</pre>
                                </td>
                            </tr>

                            <tr>
                                <td style=""padding: 20px; background-color: #f9f9f9; text-align: center;"">
                                    <p style=""font-size: 12px; color: #718096;"">Това е автоматично системно съобщение. Проверете сървърните логове за пълни детайли.</p>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </body>
        </html>";

        string[] adminEmails = (await _userManager.GetUsersInRoleAsync(AdminRoleName)).Where(u => !u.IsDeleted).Select(u => u.Email).ToArray()!;

        if (adminEmails.Any())
            await _emailService.SendEmailsBulkAsync(
                _emailUserProvider.GetNotificationsUser(),
                adminEmails,
                "Критична сървърна грешка",
                adminErrorBody,
                true);

        return ServiceResult.Ok();
    }
}