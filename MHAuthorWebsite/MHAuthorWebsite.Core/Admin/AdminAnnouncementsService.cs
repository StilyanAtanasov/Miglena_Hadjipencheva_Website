using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Configuration.EmailConfiguration.Contracts;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Admin.Announcements;
using MHAuthorWebsite.Core.Extensions;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Core.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using static MHAuthorWebsite.GCommon.ApplicationRules.AnnouncementsBoard;
using static MHAuthorWebsite.GCommon.ApplicationRules.Application;
using static MHAuthorWebsite.GCommon.ApplicationRules.Roles;

namespace MHAuthorWebsite.Core.Admin;

// TODO / Caching, uses none 
public class AdminAnnouncementsService : IAdminAnnouncementsService
{
    private readonly IEmailService _emailService;
    private readonly IEmailUserProvider _emailUserProvider;
    private readonly IApplicationRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdminAnnouncementsService> _logger;

    public AdminAnnouncementsService(IEmailService emailService, IEmailUserProvider emailUserProvider,
        IApplicationRepository repository, UserManager<ApplicationUser> userManager, ILogger<AdminAnnouncementsService> logger)
    {
        _emailService = emailService;
        _emailUserProvider = emailUserProvider;
        _repository = repository;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<ICollection<AnnouncementListItemDto>> GetAnnouncementsPagedReadonlyAsync(int page, string? search = null)
    {
        IQueryable<Announcement> query = _repository.AllReadonly<Announcement>();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a => a.Subject.Contains(search));

        Announcement[] announcements = await query
            .OrderByDescending(a => a.CreatedOn)
            .Skip((page - 1) * AnnouncementsPerPage)
            .Take(AnnouncementsPerPage)
            .ToArrayAsync();

        string[] adminIds = announcements.Select(a => a.AdminId).Distinct(StringComparer.Ordinal).ToArray();

        ApplicationUser[] admins = await _repository
            .WhereReadonly<ApplicationUser>(u => adminIds.Contains(u.Id))
            .ToArrayAsync();
        Dictionary<string, string> adminNames = admins.ToDictionary(
            u => u.Id,
            u => string.IsNullOrWhiteSpace(u.Name) ? (u.Email ?? "Администратор") : u.Name!,
            StringComparer.Ordinal);

        AnnouncementListItemDto[] result = announcements
            .Select(a => new AnnouncementListItemDto
            {
                Id = a.Id,
                Subject = a.Subject,
                MessagePreview = ToMessagePreview(ExtractPlainTextFromQuillDelta(a.MessageDelta)),
                RecipientGroup = a.RecipientGroup,
                RecipientCount = a.RecipientCount,
                CreatedOn = a.CreatedOn,
                AdminName = adminNames.TryGetValue(a.AdminId, out string? adminName) ? adminName : "Администратор"
            })
            .ToArray();

        _logger.LogInformation("Successfully retrieved announcements page {Page}. Search: {Search}. Count: {Count}",
            page, search, result.Length);
        return result;
    }

    public async Task<int> GetAnnouncementsCountReadonlyAsync(string? search = null)
    {
        IQueryable<Announcement> query = _repository.AllReadonly<Announcement>();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a => a.Subject.Contains(search));

        int count = await query.CountAsync();
        _logger.LogInformation("Retrieved announcements count. Search: {Search}. Count: {Count}", search, count);
        return count;
    }

    public async Task<ServiceResult<AnnouncementDetailsDto>> GetAnnouncementDetailsReadonlyAsync(Guid id)
    {
        Announcement? announcement = await _repository.WhereReadonly<Announcement>(a => a.Id == id).FirstOrDefaultAsync();
        if (announcement is null) return ServiceResult<AnnouncementDetailsDto>.NotFound();

        ApplicationUser? admin = await _repository.WhereReadonly<ApplicationUser>(u => u.Id == announcement.AdminId).FirstOrDefaultAsync();
        string adminName = admin is null
            ? "Администратор"
            : string.IsNullOrWhiteSpace(admin.Name) ? (admin.Email ?? "Администратор") : admin.Name;

        AnnouncementDetailsDto details = new()
        {
            Id = announcement.Id,
            Subject = announcement.Subject,
            MessageDelta = announcement.MessageDelta,
            RecipientGroup = announcement.RecipientGroup,
            AdditionalRecipients = announcement.AdditionalRecipients,
            RecipientCount = announcement.RecipientCount,
            CreatedOn = announcement.CreatedOn,
            AdminName = adminName
        };

        return ServiceResult<AnnouncementDetailsDto>.Ok(details);
    }

    public async Task<ServiceResult> CreateAnnouncementAsync(CreateAnnouncementDto model, string adminId)
    {
        ApplicationUser? admin = await _userManager.FindByIdAsync(adminId);
        if (admin is null) return ServiceResult.NotFound();

        string messageText = ExtractPlainTextFromQuillDelta(model.MessageDelta).Trim();
        if (string.IsNullOrWhiteSpace(messageText))
            return ServiceResult.BadRequest(new() { [nameof(model.MessageDelta)] = "Съобщението е задължително." });

        ServiceResult<ICollection<string>> customEmailsResult = ParseAndValidateEmails(model.AdditionalRecipients);
        if (!customEmailsResult.Success) return ServiceResult.BadRequest(customEmailsResult.Errors);

        ICollection<string> recipients = await ResolveRecipientsAsync(model.RecipientGroup);
        HashSet<string> uniqueRecipients = new(recipients, StringComparer.OrdinalIgnoreCase);
        foreach (string email in customEmailsResult.Result!) uniqueRecipients.Add(email);

        if (uniqueRecipients.Count == 0)
            return ServiceResult.BadRequest(new() { [nameof(model.RecipientGroup)] = "Няма намерени получатели за избраната аудитория." });

        string messageHtml = ConvertQuillDeltaToHtml(model.MessageDelta);
        string body = BuildAnnouncementEmailBody(model.Subject, messageHtml, admin.Name ?? admin.Email ?? "Администратор");

        try
        {
            await _emailService.SendEmailsBulkAsync(
                _emailUserProvider.GetNotificationsUser(),
                uniqueRecipients.ToArray(),
                model.Subject,
                body,
                true
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send announcement emails. Subject: {Subject}", model.Subject);
            return ServiceResult.Failure(new() { [string.Empty] = "Възникна грешка при изпращането на съобщението." });
        }

        Announcement announcement = new()
        {
            Subject = model.Subject.Trim(),
            MessageDelta = model.MessageDelta,
            RecipientGroup = model.RecipientGroup,
            AdditionalRecipients = customEmailsResult.Result!.Count == 0 ? null : string.Join("; ", customEmailsResult.Result!),
            RecipientCount = uniqueRecipients.Count,
            CreatedOn = DateTime.UtcNow,
            AdminId = adminId
        };

        await _repository.AddAsync(announcement);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Announcement {AnnouncementId} sent by admin {AdminId} to {RecipientCount} recipients.",
            announcement.Id, adminId, uniqueRecipients.Count);

        return ServiceResult.Ok();
    }

    private async Task<ICollection<string>> ResolveRecipientsAsync(AnnouncementRecipientGroup group)
    {
        ICollection<ApplicationUser> admins = await _userManager.GetUsersInRoleAsync(AdminRoleName);

        string[] adminEmails = admins
            .Where(u => u is { IsDeleted: false, IsBanned: false, EmailConfirmed: true } && !string.IsNullOrWhiteSpace(u.Email))
            .Select(u => u.Email!)
            .ToArray();

        string[] adminIds = admins.Select(u => u.Id).ToArray();

        string[] subscribedUsers = await _repository
            .WhereReadonly<ApplicationUser>(u =>
                !u.IsDeleted &&
                !u.IsBanned &&
                u.EmailConfirmed &&
                u.Email != null &&
                !adminIds.Contains(u.Id))
            .Select(u => u.Email!)
            .ToArrayAsync();

        return group switch
        {
            AnnouncementRecipientGroup.SubscribedUsers => subscribedUsers,
            AnnouncementRecipientGroup.Admins => adminEmails,
            AnnouncementRecipientGroup.SubscribedUsersAndAdmins => subscribedUsers.Concat(adminEmails).ToArray(),
            _ => Array.Empty<string>()
        };
    }

    private static ServiceResult<ICollection<string>> ParseAndValidateEmails(string? additionalRecipients)
    {
        if (string.IsNullOrWhiteSpace(additionalRecipients))
            return ServiceResult<ICollection<string>>.Ok(Array.Empty<string>());

        string[] rawEmails = additionalRecipients
            .Split(new[] { ",", ";", "\r", "\n", "\t", " " }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        HashSet<string> validEmails = new(StringComparer.OrdinalIgnoreCase);

        foreach (string email in rawEmails)
        {
            if (!IsValidEmail(email))
                return ServiceResult<ICollection<string>>.BadRequest(
                    new() { [nameof(CreateAnnouncementDto.AdditionalRecipients)] = $"Невалиден имейл адрес: {email}" });
            validEmails.Add(email);
        }

        return ServiceResult<ICollection<string>>.Ok(validEmails.ToArray());
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            MailAddress address = new(email);
            return address.Address.Equals(email, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static string ToMessagePreview(string text)
    {
        string normalized = text.ReplaceLineEndings(" ").Trim();
        if (normalized.Length <= MaxMessagePreviewLength) return normalized;
        return normalized.Substring(0, MaxMessagePreviewLength) + "...";
    }

    private static string BuildAnnouncementEmailBody(string subject, string messageHtml, string senderName)
    {
        string encodedSubject = WebUtility.HtmlEncode(subject.Trim());
        string encodedSenderName = WebUtility.HtmlEncode(senderName);

        return $@"
            <!DOCTYPE html>
            <html lang=""bg"">
            <head>
                <meta charset=""UTF-8"">
                <style>
                    .content-text {{ line-height: 1.6; color: #181717; font-size: 16px; }}
                    .message-box {{ border-left: 4px solid #f8dff8; padding-left: 15px; margin: 20px 0; color: #181717; }}
                    .message-box a {{ color: #2767e7; text-decoration: underline; }}
                    .message-box h1 {{ font-size: 28px; margin: 14px 0; }}
                    .message-box h2 {{ font-size: 22px; margin: 12px 0; }}
                    .message-box p {{ margin: 8px 0; }}
                    .message-box ul, .message-box ol {{ margin: 10px 0 10px 22px; padding: 0; }}
                    .message-box blockquote {{ margin: 10px 0; padding-left: 12px; border-left: 3px solid #d8b2d8; color: #616161; }}
                    .message-box code {{ background-color: #f4ebf4; padding: 0 4px; border-radius: 4px; }}
                    .message-box pre {{ background-color: #f4ebf4; padding: 12px; border-radius: 6px; overflow-x: auto; }}
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
                                            Ново съобщение от {WebsiteName}
                                        </span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 34px 30px 30px 30px;"">
                                        <p style=""font-size: 14px; color: #616161; margin: 0 0 14px 0;"">Тема: <strong>{encodedSubject}</strong></p>
                                        <div class=""message-box content-text"">
                                            {messageHtml}
                                        </div>
                                        <p style=""font-size: 14px; color: #999; margin-top: 28px;"">
                                            Изпратено от: {encodedSenderName}
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 20px; background-color: #fbf3fb; text-align: center; font-size: 12px; color: #616161; border-radius: 0 0 8px 8px;"">
                                        <p style=""margin: 0;"">Този имейл е автоматично известие от сайта на {WebsiteName}.</p>
                                        <p style=""margin: 5px 0 0 0;"">&copy; {DateTime.UtcNow.Year} {WebsiteName}</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
    }

    private static string ConvertQuillDeltaToHtml(string deltaJson)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(deltaJson);
            if (!document.RootElement.TryGetProperty("ops", out JsonElement opsElement) || opsElement.ValueKind != JsonValueKind.Array)
                return string.Empty;

            StringBuilder htmlBuilder = new();
            StringBuilder lineBuilder = new();
            string? openListType = null;

            foreach (JsonElement op in opsElement.EnumerateArray())
            {
                if (!op.TryGetProperty("insert", out JsonElement insertElement)) continue;
                JsonElement attributesElement = op.TryGetProperty("attributes", out JsonElement attrs) ? attrs : default;

                if (insertElement.ValueKind != JsonValueKind.String) continue;
                string insert = insertElement.GetString() ?? string.Empty;

                for (int index = 0; index < insert.Length; index++)
                {
                    char current = insert[index];
                    if (current == '\n')
                    {
                        AppendLineHtml(lineBuilder.ToString(), attributesElement, htmlBuilder, ref openListType);
                        lineBuilder.Clear();
                        continue;
                    }

                    int segmentStart = index;
                    while (index < insert.Length && insert[index] != '\n') index++;
                    string segment = insert.Substring(segmentStart, index - segmentStart);
                    lineBuilder.Append(ApplyInlineFormatting(segment, attributesElement));
                    if (index < insert.Length && insert[index] == '\n') index--;
                }
            }

            if (lineBuilder.Length > 0)
                AppendLineHtml(lineBuilder.ToString(), default, htmlBuilder, ref openListType);

            if (!string.IsNullOrEmpty(openListType))
                htmlBuilder.Append(openListType == "ordered" ? "</ol>" : "</ul>");

            return htmlBuilder.ToString();
        }
        catch
        {
            string fallbackText = WebUtility.HtmlEncode(ExtractPlainTextFromQuillDelta(deltaJson)).Replace("\n", "<br />");
            return $"<p>{fallbackText}</p>";
        }
    }

    private static void AppendLineHtml(string lineContent, JsonElement lineAttributes, StringBuilder htmlBuilder, ref string? openListType)
    {
        string content = string.IsNullOrWhiteSpace(lineContent) ? "<br />" : lineContent;

        string? listType = null;
        if (lineAttributes.ValueKind == JsonValueKind.Object &&
            lineAttributes.TryGetProperty("list", out JsonElement listElement) &&
            listElement.ValueKind == JsonValueKind.String)
            listType = listElement.GetString();

        if (listType is not null)
        {
            if (openListType != listType)
            {
                if (!string.IsNullOrEmpty(openListType))
                    htmlBuilder.Append(openListType == "ordered" ? "</ol>" : "</ul>");

                htmlBuilder.Append(listType == "ordered" ? "<ol>" : "<ul>");
                openListType = listType;
            }

            htmlBuilder.Append($"<li>{content}</li>");
            return;
        }

        if (!string.IsNullOrEmpty(openListType))
        {
            htmlBuilder.Append(openListType == "ordered" ? "</ol>" : "</ul>");
            openListType = null;
        }

        string alignment = "left";
        if (lineAttributes.ValueKind == JsonValueKind.Object &&
            lineAttributes.TryGetProperty("align", out JsonElement alignElement) &&
            alignElement.ValueKind == JsonValueKind.String &&
            !string.IsNullOrWhiteSpace(alignElement.GetString()))
            alignment = alignElement.GetString()!;

        string alignStyle = alignment == "left" ? string.Empty : $" style=\"text-align:{alignment};\"";

        if (lineAttributes.ValueKind == JsonValueKind.Object &&
            lineAttributes.TryGetProperty("header", out JsonElement headerElement) &&
            headerElement.ValueKind == JsonValueKind.Number)
        {
            int headerLevel = headerElement.GetInt32();
            if (headerLevel < 1 || headerLevel > 2) headerLevel = 2;
            htmlBuilder.Append($"<h{headerLevel}{alignStyle}>{content}</h{headerLevel}>");
            return;
        }

        if (lineAttributes.ValueKind == JsonValueKind.Object &&
            lineAttributes.TryGetProperty("blockquote", out JsonElement blockquoteElement) &&
            blockquoteElement.ValueKind == JsonValueKind.True)
        {
            htmlBuilder.Append($"<blockquote{alignStyle}>{content}</blockquote>");
            return;
        }

        if (lineAttributes.ValueKind == JsonValueKind.Object &&
            lineAttributes.TryGetProperty("code-block", out JsonElement codeBlockElement) &&
            codeBlockElement.ValueKind == JsonValueKind.True)
        {
            htmlBuilder.Append($"<pre><code>{content}</code></pre>");
            return;
        }

        htmlBuilder.Append($"<p{alignStyle}>{content}</p>");
    }

    private static string ApplyInlineFormatting(string text, JsonElement attributes)
    {
        string encoded = WebUtility.HtmlEncode(text);
        if (string.IsNullOrEmpty(encoded)) return string.Empty;

        if (attributes.ValueKind != JsonValueKind.Object) return encoded;

        bool bold = attributes.TryGetProperty("bold", out JsonElement boldElement) && boldElement.ValueKind == JsonValueKind.True;
        bool italic = attributes.TryGetProperty("italic", out JsonElement italicElement) && italicElement.ValueKind == JsonValueKind.True;
        bool underline = attributes.TryGetProperty("underline", out JsonElement underlineElement) && underlineElement.ValueKind == JsonValueKind.True;
        bool strike = attributes.TryGetProperty("strike", out JsonElement strikeElement) && strikeElement.ValueKind == JsonValueKind.True;
        bool code = attributes.TryGetProperty("code", out JsonElement codeElement) && codeElement.ValueKind == JsonValueKind.True;

        string current = encoded;

        if (code) current = $"<code>{current}</code>";
        if (bold) current = $"<strong>{current}</strong>";
        if (italic) current = $"<em>{current}</em>";

        if (underline || strike)
        {
            List<string> textDecorations = new();
            if (underline) textDecorations.Add("underline");
            if (strike) textDecorations.Add("line-through");
            current = $"<span style=\"text-decoration:{string.Join(" ", textDecorations)};\">{current}</span>";
        }

        List<string> styles = new();
        if (attributes.TryGetProperty("color", out JsonElement colorElement) &&
            colorElement.ValueKind == JsonValueKind.String &&
            !string.IsNullOrWhiteSpace(colorElement.GetString()))
            styles.Add($"color:{WebUtility.HtmlEncode(colorElement.GetString())}");

        if (attributes.TryGetProperty("background", out JsonElement backgroundElement) &&
            backgroundElement.ValueKind == JsonValueKind.String &&
            !string.IsNullOrWhiteSpace(backgroundElement.GetString()))
            styles.Add($"background-color:{WebUtility.HtmlEncode(backgroundElement.GetString())}");

        if (styles.Count > 0)
            current = $"<span style=\"{string.Join(";", styles)};\">{current}</span>";

        if (attributes.TryGetProperty("link", out JsonElement linkElement) &&
            linkElement.ValueKind == JsonValueKind.String)
        {
            string? rawLink = linkElement.GetString();
            if (!string.IsNullOrWhiteSpace(rawLink))
            {
                string href = BuildSafeLinkHref(rawLink);
                current = $"<a href=\"{WebUtility.HtmlEncode(href)}\" target=\"_blank\" rel=\"noopener noreferrer\">{current}</a>";
            }
        }

        return current;
    }

    private static string BuildSafeLinkHref(string rawLink)
    {
        if (Uri.TryCreate(rawLink, UriKind.Absolute, out Uri? absolute))
            return absolute.Scheme is "http" or "https" ? absolute.ToString() : "https://" + rawLink;

        if (Uri.TryCreate("https://" + rawLink, UriKind.Absolute, out Uri? withHttps))
            return withHttps.ToString();

        return "https://example.com";
    }

    private static string ExtractPlainTextFromQuillDelta(string deltaJson)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(deltaJson);
            if (!doc.RootElement.TryGetProperty("ops", out JsonElement ops)) return string.Empty;

            StringBuilder sb = new();

            foreach (JsonElement op in ops.EnumerateArray())
            {
                if (!op.TryGetProperty("insert", out JsonElement insert)) continue;
                if (insert.ValueKind == JsonValueKind.String) sb.Append(insert.GetString());
            }

            return sb.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }
}

