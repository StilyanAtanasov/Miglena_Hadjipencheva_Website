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
using static MHAuthorWebsite.GCommon.ApplicationRules.CacheDefaultDurations;
using static MHAuthorWebsite.GCommon.ApplicationRules.CacheKeys;
using static MHAuthorWebsite.GCommon.ApplicationRules.Roles;
using static MHAuthorWebsite.GCommon.EntityConstraints.AnnouncementEmailDelivery;

namespace MHAuthorWebsite.Core.Admin;

public class AdminAnnouncementsService : IAdminAnnouncementsService
{
    private readonly IEmailService _emailService;
    private readonly IEmailUserProvider _emailUserProvider;
    private readonly IApplicationRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFastCacheService _cache;
    private readonly IUrlProvider _urlProvider;
    private readonly ILogger<AdminAnnouncementsService> _logger;

    public AdminAnnouncementsService(
        IEmailService emailService,
        IEmailUserProvider emailUserProvider,
        IApplicationRepository repository,
        UserManager<ApplicationUser> userManager,
        IFastCacheService cache,
        IUrlProvider urlProvider,
        ILogger<AdminAnnouncementsService> logger)
    {
        _emailService = emailService;
        _emailUserProvider = emailUserProvider;
        _repository = repository;
        _userManager = userManager;
        _cache = cache;
        _urlProvider = urlProvider;
        _logger = logger;
    }

    public async Task<ICollection<AnnouncementListItemDto>> GetAnnouncementsPagedReadonlyAsync(int page, string? search = null)
    {
        IQueryable<Announcement> query = _repository.AllReadonly<Announcement>();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a => a.Subject.Contains(search));

        Guid[] announcementIds = await query
            .OrderByDescending(a => a.CreatedOn)
            .Skip((page - 1) * AnnouncementsPerPage)
            .Take(AnnouncementsPerPage)
            .Select(a => a.Id)
            .ToArrayAsync();

        if (!announcementIds.Any())
            return Array.Empty<AnnouncementListItemDto>();

        ICollection<AnnouncementListItemDto> cards = await GetAnnouncementCardsBatchAsync(announcementIds);

        AnnouncementListItemDto[] orderedCards = cards
            .OrderBy(c => Array.IndexOf(announcementIds, c.Id))
            .ToArray();

        _logger.LogInformation(
            "Successfully retrieved announcements page {Page}. Search: {Search}. Count: {Count}",
            page,
            search,
            orderedCards.Length);

        return orderedCards;
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
        AnnouncementDetailsDto? cached = await _cache.GetAsync<AnnouncementDetailsDto>(AnnouncementDetailsKey(id));
        if (cached is not null)
            return ServiceResult<AnnouncementDetailsDto>.Ok(cached);

        Announcement? announcement = await _repository.WhereReadonly<Announcement>(a => a.Id == id).FirstOrDefaultAsync();
        if (announcement is null)
            return ServiceResult<AnnouncementDetailsDto>.NotFound();

        ApplicationUser? admin = await _repository.WhereReadonly<ApplicationUser>(u => u.Id == announcement.AdminId).FirstOrDefaultAsync();
        string adminName = admin is null
            ? "Администратор"
            : string.IsNullOrWhiteSpace(admin.Name) ? (admin.Email ?? "Администратор") : admin.Name;

        AnnouncementRecipientDeliveryDto[] deliveries = await _repository
            .WhereReadonly<AnnouncementEmailDelivery>(d => d.AnnouncementId == id)
            .OrderByDescending(d => d.DeliveryStatus == AnnouncementDeliveryStatus.Sent)
            .ThenBy(d => d.Email)
            .Select(d => new AnnouncementRecipientDeliveryDto
            {
                Email = d.Email,
                RecipientSource = d.RecipientSource,
                DeliveryStatus = d.DeliveryStatus,
                ErrorMessage = d.ErrorMessage,
                DeliveredOn = d.DeliveredOn
            })
            .ToArrayAsync();

        AnnouncementDetailsDto details = new()
        {
            Id = announcement.Id,
            Subject = announcement.Subject,
            MessageDelta = announcement.MessageDelta,
            RecipientGroup = announcement.RecipientGroup,
            AdditionalRecipients = announcement.AdditionalRecipients,
            RecipientCount = announcement.RecipientCount,
            FailedRecipientCount = deliveries.Count(d => d.DeliveryStatus == AnnouncementDeliveryStatus.Failed),
            Deliveries = deliveries,
            CreatedOn = announcement.CreatedOn,
            AdminName = adminName
        };

        _cache.SetFireAndForget(AnnouncementDetailsKey(id), details, TimeSpan.FromDays(AnnouncementDetailsTtlDays));
        return ServiceResult<AnnouncementDetailsDto>.Ok(details);
    }

    public async Task<ServiceResult> CreateAnnouncementAsync(CreateAnnouncementDto model, string adminId)
    {
        ApplicationUser? admin = await _userManager.FindByIdAsync(adminId);
        if (admin is null)
            return ServiceResult.NotFound();

        string messageText = ExtractPlainTextFromQuillDelta(model.MessageDelta).Trim();
        if (string.IsNullOrWhiteSpace(messageText))
            return ServiceResult.BadRequest(new() { [nameof(model.MessageDelta)] = "Съобщението е задължително." });

        if (string.IsNullOrWhiteSpace(model.MessageHtml))
            return ServiceResult.BadRequest(new() { [nameof(model.MessageHtml)] = "HTML съдържанието е задължително." });

        ServiceResult<ICollection<string>> customEmailsResult = ParseAndValidateEmails(model.AdditionalRecipients);
        if (!customEmailsResult.Success)
            return ServiceResult.BadRequest(customEmailsResult.Errors);

        if (model.RecipientGroup == AnnouncementRecipientGroup.AdditionalRecipientsOnly &&
            customEmailsResult.Result!.Count == 0)
        {
            return ServiceResult.BadRequest(new()
            {
                [nameof(model.AdditionalRecipients)] =
                    "При избор \"Само допълнителни имейли\" трябва да въведете поне един имейл адрес."
            });
        }

        ICollection<ResolvedRecipient> resolvedRecipients = await ResolveRecipientsAsync(model.RecipientGroup);
        Dictionary<string, ResolvedRecipient> uniqueRecipients = new(StringComparer.OrdinalIgnoreCase);

        foreach (ResolvedRecipient resolvedRecipient in resolvedRecipients)
            AddOrUpgradeRecipient(uniqueRecipients, resolvedRecipient);

        foreach (string customEmail in customEmailsResult.Result!)
        {
            ResolvedRecipient additionalRecipient = new()
            {
                Email = customEmail,
                RecipientSource = AnnouncementRecipientSource.AdditionalEmail,
                UserId = null,
                UnsubscribeToken = null
            };

            AddOrUpgradeRecipient(uniqueRecipients, additionalRecipient);
        }

        if (uniqueRecipients.Count == 0)
            return ServiceResult.BadRequest(new() { [nameof(model.RecipientGroup)] = "Няма намерени получатели за избраната аудитория." });

        string senderName = admin.Name ?? admin.Email ?? "Администратор";
        List<AnnouncementEmailDelivery> deliveries = new();
        int successfulDeliveries = 0;

        foreach (ResolvedRecipient recipient in uniqueRecipients.Values)
        {
            string? unsubscribeUrl = recipient.UnsubscribeToken is null
                ? null
                : _urlProvider.GetMarketingUnsubscribeUrl(recipient.Email, recipient.UnsubscribeToken);

            string body = BuildAnnouncementEmailBody(model.Subject, model.MessageHtml, senderName, unsubscribeUrl);

            try
            {
                await _emailService.SendEmailAsync(
                    _emailUserProvider.GetNotificationsUser(),
                    recipient.Email,
                    model.Subject,
                    body,
                    true);

                successfulDeliveries++;
                deliveries.Add(new AnnouncementEmailDelivery
                {
                    Id = Guid.NewGuid(),
                    Email = recipient.Email,
                    UserId = recipient.UserId,
                    RecipientSource = recipient.RecipientSource,
                    DeliveryStatus = AnnouncementDeliveryStatus.Sent,
                    ErrorMessage = null,
                    DeliveredOn = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send announcement to {Email}. Subject: {Subject}", recipient.Email, model.Subject);

                string error = ex.Message.Length > ErrorMessageMaxLength
                    ? ex.Message.Substring(0, ErrorMessageMaxLength)
                    : ex.Message;

                deliveries.Add(new AnnouncementEmailDelivery
                {
                    Id = Guid.NewGuid(),
                    Email = recipient.Email,
                    UserId = recipient.UserId,
                    RecipientSource = recipient.RecipientSource,
                    DeliveryStatus = AnnouncementDeliveryStatus.Failed,
                    ErrorMessage = error,
                    DeliveredOn = null
                });
            }
        }

        Announcement announcement = new()
        {
            Id = Guid.NewGuid(),
            Subject = model.Subject.Trim(),
            MessageDelta = model.MessageDelta,
            RecipientGroup = model.RecipientGroup,
            AdditionalRecipients = customEmailsResult.Result!.Count == 0 ? null : string.Join("; ", customEmailsResult.Result),
            RecipientCount = successfulDeliveries,
            CreatedOn = DateTime.UtcNow,
            AdminId = adminId
        };

        foreach (AnnouncementEmailDelivery delivery in deliveries)
            delivery.AnnouncementId = announcement.Id;

        await _repository.AddAsync(announcement);
        await _repository.AddRangeAsync(deliveries);
        await _repository.SaveChangesAsync();

        string previewText = ToMessagePreview(messageText);
        string resolvedAdminName = string.IsNullOrWhiteSpace(admin.Name) ? (admin.Email ?? "Администратор") : admin.Name;

        AnnouncementListItemDto cardDto = new()
        {
            Id = announcement.Id,
            Subject = announcement.Subject,
            MessagePreview = previewText,
            RecipientGroup = announcement.RecipientGroup,
            RecipientCount = announcement.RecipientCount,
            CreatedOn = announcement.CreatedOn,
            AdminName = resolvedAdminName
        };
        _cache.SetFireAndForget(AnnouncementCardKey(announcement.Id), cardDto, TimeSpan.FromDays(AnnouncementCardTtlDays));

        AnnouncementDetailsDto detailsDto = new()
        {
            Id = announcement.Id,
            Subject = announcement.Subject,
            MessageDelta = announcement.MessageDelta,
            RecipientGroup = announcement.RecipientGroup,
            AdditionalRecipients = announcement.AdditionalRecipients,
            RecipientCount = announcement.RecipientCount,
            FailedRecipientCount = deliveries.Count(d => d.DeliveryStatus == AnnouncementDeliveryStatus.Failed),
            Deliveries = deliveries.Select(d => new AnnouncementRecipientDeliveryDto
            {
                Email = d.Email,
                RecipientSource = d.RecipientSource,
                DeliveryStatus = d.DeliveryStatus,
                ErrorMessage = d.ErrorMessage,
                DeliveredOn = d.DeliveredOn
            }).ToArray(),
            CreatedOn = announcement.CreatedOn,
            AdminName = resolvedAdminName
        };
        _cache.SetFireAndForget(AnnouncementDetailsKey(announcement.Id), detailsDto, TimeSpan.FromDays(AnnouncementDetailsTtlDays));

        _logger.LogInformation(
            "Announcement {AnnouncementId} sent by admin {AdminId}. Sent: {SentCount}. Failed: {FailedCount}.",
            announcement.Id,
            adminId,
            successfulDeliveries,
            deliveries.Count(d => d.DeliveryStatus == AnnouncementDeliveryStatus.Failed));

        return ServiceResult.Ok();
    }

    private async Task<ICollection<AnnouncementListItemDto>> GetAnnouncementCardsBatchAsync(Guid[] announcementIds)
    {
        ICollection<string> keys = announcementIds
            .Select(AnnouncementCardKey)
            .ToList();

        IEnumerable<AnnouncementListItemDto?> cachedValues = await _cache.GetBatchAsync<AnnouncementListItemDto>(keys);

        List<AnnouncementListItemDto> cards = new();
        List<Guid> missingIds = new();

        var cachedValuesList = cachedValues.ToList();
        for (int i = 0; i < announcementIds.Length; i++)
        {
            AnnouncementListItemDto? cachedCard = cachedValuesList[i];
            if (cachedCard is not null)
                cards.Add(cachedCard);
            else
                missingIds.Add(announcementIds[i]);
        }

        if (!missingIds.Any())
            return cards;

        Announcement[] missingAnnouncements = await _repository
            .WhereReadonly<Announcement>(a => missingIds.Contains(a.Id))
            .ToArrayAsync();

        string[] adminIds = missingAnnouncements.Select(a => a.AdminId).Distinct(StringComparer.Ordinal).ToArray();
        ApplicationUser[] admins = await _repository
            .WhereReadonly<ApplicationUser>(u => adminIds.Contains(u.Id))
            .ToArrayAsync();

        Dictionary<string, string> adminNames = admins.ToDictionary(
            u => u.Id,
            u => string.IsNullOrWhiteSpace(u.Name) ? (u.Email ?? "Администратор") : u.Name!,
            StringComparer.Ordinal);

        IDictionary<string, AnnouncementListItemDto> valuesToCache = new Dictionary<string, AnnouncementListItemDto>();
        foreach (Announcement announcement in missingAnnouncements)
        {
            AnnouncementListItemDto dto = new()
            {
                Id = announcement.Id,
                Subject = announcement.Subject,
                MessagePreview = ToMessagePreview(ExtractPlainTextFromQuillDelta(announcement.MessageDelta)),
                RecipientGroup = announcement.RecipientGroup,
                RecipientCount = announcement.RecipientCount,
                CreatedOn = announcement.CreatedOn,
                AdminName = adminNames.TryGetValue(announcement.AdminId, out string? adminName) ? adminName : "Администратор"
            };

            valuesToCache.Add(AnnouncementCardKey(dto.Id), dto);
            cards.Add(dto);
        }

        if (valuesToCache.Any())
        {
            await _cache.SetBatchAsync(valuesToCache, TimeSpan.FromDays(AnnouncementCardTtlDays), fireAndForget: true);
        }

        return cards;
    }

    private async Task<ICollection<ResolvedRecipient>> ResolveRecipientsAsync(AnnouncementRecipientGroup group)
    {
        if (group == AnnouncementRecipientGroup.AdditionalRecipientsOnly)
            return Array.Empty<ResolvedRecipient>();

        ICollection<ApplicationUser> adminsInRole = await _userManager.GetUsersInRoleAsync(AdminRoleName);
        ApplicationUser[] adminUsers = adminsInRole
            .Where(u => u is { Email: not null, IsDeleted: false, IsBanned: false, EmailConfirmed: true, IsMarketingSubscribed: true })
            .ToArray();

        HashSet<string> adminIds = adminUsers
            .Select(u => u.Id)
            .ToHashSet(StringComparer.Ordinal);

        IQueryable<ApplicationUser> subscribedUsersQuery = _repository
            .Where<ApplicationUser>(u =>
                !u.IsDeleted &&
                !u.IsBanned &&
                u.EmailConfirmed &&
                u.Email != null &&
                u.IsMarketingSubscribed);

        List<ResolvedRecipient> recipients = new();

        if (group == AnnouncementRecipientGroup.Admins || group == AnnouncementRecipientGroup.SubscribedUsersAndAdmins)
        {
            recipients.AddRange(adminUsers.Select(admin => new ResolvedRecipient
            {
                Email = admin.Email!,
                UserId = admin.Id,
                RecipientSource = AnnouncementRecipientSource.Admin,
                UnsubscribeToken = admin.MarketingUnsubscribeToken
            }));
        }

        if (group == AnnouncementRecipientGroup.SubscribedUsers || group == AnnouncementRecipientGroup.SubscribedUsersAndAdmins)
        {
            ApplicationUser[] subscribedUsers = await subscribedUsersQuery
                .Where(u => !adminIds.Contains(u.Id))
                .ToArrayAsync();

            recipients.AddRange(subscribedUsers.Select(user => new ResolvedRecipient
            {
                Email = user.Email!,
                UserId = user.Id,
                RecipientSource = AnnouncementRecipientSource.SubscribedUser,
                UnsubscribeToken = user.MarketingUnsubscribeToken
            }));
        }

        await EnsureUnsubscribeTokensAsync(recipients);
        return recipients;
    }

    private async Task EnsureUnsubscribeTokensAsync(ICollection<ResolvedRecipient> recipients)
    {
        string[] missingUserIds = recipients
            .Where(r => r.UserId is not null && string.IsNullOrWhiteSpace(r.UnsubscribeToken))
            .Select(r => r.UserId!)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (missingUserIds.Length == 0)
            return;

        ApplicationUser[] trackedUsers = await _repository
            .Where<ApplicationUser>(u => missingUserIds.Contains(u.Id))
            .ToArrayAsync();

        Dictionary<string, string> generatedTokens = new(StringComparer.Ordinal);
        foreach (ApplicationUser trackedUser in trackedUsers)
        {
            string token = GenerateOneTimeToken();
            trackedUser.MarketingUnsubscribeToken = token;
            trackedUser.MarketingUnsubscribeTokenCreatedOn = DateTime.UtcNow;
            generatedTokens[trackedUser.Id] = token;
        }

        foreach (ResolvedRecipient recipient in recipients.Where(r => r.UserId is not null && string.IsNullOrWhiteSpace(r.UnsubscribeToken)))
        {
            if (recipient.UserId is not null && generatedTokens.TryGetValue(recipient.UserId, out string? token))
                recipient.UnsubscribeToken = token;
        }
    }

    private static void AddOrUpgradeRecipient(Dictionary<string, ResolvedRecipient> uniqueRecipients, ResolvedRecipient candidate)
    {
        if (!uniqueRecipients.TryGetValue(candidate.Email, out ResolvedRecipient? existingRecipient))
        {
            uniqueRecipients[candidate.Email] = candidate;
            return;
        }

        if (candidate.RecipientSource == AnnouncementRecipientSource.AdditionalEmail &&
            existingRecipient.RecipientSource != AnnouncementRecipientSource.AdditionalEmail)
        {
            uniqueRecipients[candidate.Email] = candidate;
            return;
        }

        if (string.IsNullOrWhiteSpace(existingRecipient.UnsubscribeToken) && !string.IsNullOrWhiteSpace(candidate.UnsubscribeToken))
            existingRecipient.UnsubscribeToken = candidate.UnsubscribeToken;
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
            {
                return ServiceResult<ICollection<string>>.BadRequest(
                    new() { [nameof(CreateAnnouncementDto.AdditionalRecipients)] = $"Невалиден имейл адрес: {email}" });
            }

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
        if (normalized.Length <= MaxMessagePreviewLength)
            return normalized;

        return normalized.Substring(0, MaxMessagePreviewLength) + "...";
    }

    private static string BuildAnnouncementEmailBody(string subject, string messageHtml, string senderName, string? unsubscribeUrl)
    {
        string encodedSubject = WebUtility.HtmlEncode(subject.Trim());
        string encodedSenderName = WebUtility.HtmlEncode(senderName);
        string unsubscribeSection = string.IsNullOrWhiteSpace(unsubscribeUrl)
            ? string.Empty
            : $@"<p style=""margin-top: 18px; font-size: 12px; color: #616161;"">
                    Ако не желаете да получавате маркетинг съобщения, можете да се
                    <a href=""{WebUtility.HtmlEncode(unsubscribeUrl)}"" style=""color: #2767e7;"">отпишете от бюлетина</a>.
                 </p>";

        return $@"
            <!DOCTYPE html>
            <html lang=""bg"">
            <head>
                <meta charset=""UTF-8"">
                <style>
                    .content-text {{ line-height: 1.6; color: #181717; font-size: 16px; }}
                    .message-box {{ border-left: 4px solid #f8dff8; padding-left: 15px; margin: 20px 0; color: #181717; }}
                    .message-box a {{ color: #2767e7; text-decoration: underline; }}
                    .message-box h1, .message-box h2, .message-box h3 {{ margin: 14px 0 8px 0; color: #3a053a; }}
                    .message-box p {{ margin: 8px 0; }}
                    .message-box blockquote {{ margin: 12px 0; padding-left: 12px; border-left: 3px solid #e7c4e7; color: #616161; }}
                    .message-box .ql-align-center {{ text-align: center; }}
                    .message-box .ql-align-right {{ text-align: right; }}
                    .message-box .ql-align-justify {{ text-align: justify; }}
                    .message-box .ql-font-serif {{ font-family: Georgia, 'Times New Roman', serif; }}
                    .message-box .ql-font-monospace {{ font-family: Consolas, 'Courier New', monospace; }}
                    .message-box .ql-size-small {{ font-size: 0.75em; }}
                    .message-box .ql-size-large {{ font-size: 1.25em; }}
                    .message-box .ql-size-huge {{ font-size: 1.8em; }}
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
                                        {unsubscribeSection}
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

    private static string GenerateOneTimeToken()
        => $"{Guid.NewGuid():N}{Guid.NewGuid():N}";

    private static string ExtractPlainTextFromQuillDelta(string deltaJson)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(deltaJson);
            if (!doc.RootElement.TryGetProperty("ops", out JsonElement ops))
                return string.Empty;

            StringBuilder sb = new();

            foreach (JsonElement op in ops.EnumerateArray())
            {
                if (!op.TryGetProperty("insert", out JsonElement insert))
                    continue;
                if (insert.ValueKind == JsonValueKind.String)
                    sb.Append(insert.GetString());
            }

            return sb.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }

    private sealed class ResolvedRecipient
    {
        public string Email { get; init; } = null!;

        public string? UserId { get; init; }

        public AnnouncementRecipientSource RecipientSource { get; init; }

        public string? UnsubscribeToken { get; set; }
    }
}
