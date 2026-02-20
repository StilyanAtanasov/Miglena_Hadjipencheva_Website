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
using StackExchange.Redis;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using static MHAuthorWebsite.GCommon.ApplicationRules.AnnouncementsBoard;
using static MHAuthorWebsite.GCommon.ApplicationRules.Application;
using static MHAuthorWebsite.GCommon.ApplicationRules.CacheDefaultDurations;
using static MHAuthorWebsite.GCommon.ApplicationRules.CacheKeys;
using static MHAuthorWebsite.GCommon.ApplicationRules.Roles;

namespace MHAuthorWebsite.Core.Admin;

public class AdminAnnouncementsService : IAdminAnnouncementsService
{
    private readonly IEmailService _emailService;
    private readonly IEmailUserProvider _emailUserProvider;
    private readonly IApplicationRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFastCacheService _cache;
    private readonly ILogger<AdminAnnouncementsService> _logger;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    public AdminAnnouncementsService(IEmailService emailService, IEmailUserProvider emailUserProvider,
        IApplicationRepository repository, UserManager<ApplicationUser> userManager, IFastCacheService cache,
        ILogger<AdminAnnouncementsService> logger)
    {
        _emailService = emailService;
        _emailUserProvider = emailUserProvider;
        _repository = repository;
        _userManager = userManager;
        _cache = cache;
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

        if (!announcementIds.Any()) return Array.Empty<AnnouncementListItemDto>();

        ICollection<AnnouncementListItemDto> cards = await GetAnnouncementCardsBatchAsync(announcementIds);

        AnnouncementListItemDto[] orderedCards = cards
            .OrderBy(c => Array.IndexOf(announcementIds, c.Id))
            .ToArray();

        _logger.LogInformation("Successfully retrieved announcements page {Page}. Search: {Search}. Count: {Count}",
            page, search, orderedCards.Length);

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
        if (cached is not null) return ServiceResult<AnnouncementDetailsDto>.Ok(cached);

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

        _cache.SetFireAndForget(AnnouncementDetailsKey(id), details, TimeSpan.FromDays(AnnouncementDetailsTtlDays));
        return ServiceResult<AnnouncementDetailsDto>.Ok(details);
    }

    public async Task<ServiceResult> CreateAnnouncementAsync(CreateAnnouncementDto model, string adminId)
    {
        ApplicationUser? admin = await _userManager.FindByIdAsync(adminId);
        if (admin is null) return ServiceResult.NotFound();

        string messageText = ExtractPlainTextFromQuillDelta(model.MessageDelta).Trim();
        if (string.IsNullOrWhiteSpace(messageText))
            return ServiceResult.BadRequest(new() { [nameof(model.MessageDelta)] = "Съобщението е задължително." });

        if (string.IsNullOrWhiteSpace(model.MessageHtml))
            return ServiceResult.BadRequest(new() { [nameof(model.MessageHtml)] = "HTML съдържанието е задължително." });

        ServiceResult<ICollection<string>> customEmailsResult = ParseAndValidateEmails(model.AdditionalRecipients);
        if (!customEmailsResult.Success) return ServiceResult.BadRequest(customEmailsResult.Errors);
        if (model.RecipientGroup == AnnouncementRecipientGroup.AdditionalRecipientsOnly && customEmailsResult.Result!.Count == 0)
            return ServiceResult.BadRequest(new()
            {
                [nameof(model.AdditionalRecipients)] =
                    "При избор \"Само допълнителни имейли\" трябва да въведете поне един имейл адрес."
            });

        ICollection<string> recipients = await ResolveRecipientsAsync(model.RecipientGroup);
        HashSet<string> uniqueRecipients = new(recipients, StringComparer.OrdinalIgnoreCase);
        foreach (string email in customEmailsResult.Result!) uniqueRecipients.Add(email);

        if (uniqueRecipients.Count == 0)
            return ServiceResult.BadRequest(new() { [nameof(model.RecipientGroup)] = "Няма намерени получатели за избраната аудитория." });

        string body = BuildAnnouncementEmailBody(model.Subject, model.MessageHtml, admin.Name ?? admin.Email ?? "Администратор");

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

        AnnouncementListItemDto cardDto = new()
        {
            Id = announcement.Id,
            Subject = announcement.Subject,
            MessagePreview = ToMessagePreview(messageText),
            RecipientGroup = announcement.RecipientGroup,
            RecipientCount = announcement.RecipientCount,
            CreatedOn = announcement.CreatedOn,
            AdminName = string.IsNullOrWhiteSpace(admin.Name) ? (admin.Email ?? "Администратор") : admin.Name
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
            CreatedOn = announcement.CreatedOn,
            AdminName = cardDto.AdminName
        };
        _cache.SetFireAndForget(AnnouncementDetailsKey(announcement.Id), detailsDto, TimeSpan.FromDays(AnnouncementDetailsTtlDays));

        _logger.LogInformation("Announcement {AnnouncementId} sent by admin {AdminId} to {RecipientCount} recipients.",
            announcement.Id, adminId, uniqueRecipients.Count);

        return ServiceResult.Ok();
    }

    private async Task<ICollection<AnnouncementListItemDto>> GetAnnouncementCardsBatchAsync(Guid[] announcementIds)
    {
        RedisKey[] keys = announcementIds
            .Select(id => (RedisKey)AnnouncementCardKey(id))
            .ToArray();

        IBatch readBatch = _cache.CreateBatch();
        Task<RedisValue>[] readTasks = keys.Select(key => readBatch.StringGetAsync(key)).ToArray();
        readBatch.Execute();

        RedisValue[] cachedValues = await Task.WhenAll(readTasks);
        List<AnnouncementListItemDto> cards = new();
        List<Guid> missingIds = new();

        for (int i = 0; i < announcementIds.Length; i++)
        {
            if (cachedValues[i].HasValue)
            {
                AnnouncementListItemDto? cachedCard = JsonSerializer.Deserialize<AnnouncementListItemDto>(cachedValues[i].ToString(), _jsonOptions);
                if (cachedCard is not null) cards.Add(cachedCard);
            }
            else
                missingIds.Add(announcementIds[i]);
        }

        if (!missingIds.Any()) return cards;

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

        IBatch writeBatch = _cache.CreateBatch();
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

            await writeBatch.StringSetAsync(
                (RedisKey)AnnouncementCardKey(dto.Id),
                (RedisValue)JsonSerializer.Serialize(dto, _jsonOptions),
                TimeSpan.FromDays(AnnouncementCardTtlDays),
                flags: CommandFlags.FireAndForget);

            cards.Add(dto);
        }
        writeBatch.Execute();

        return cards;
    }

    private async Task<ICollection<string>> ResolveRecipientsAsync(AnnouncementRecipientGroup group)
    {
        if (group == AnnouncementRecipientGroup.AdditionalRecipientsOnly)
            return Array.Empty<string>();

        IQueryable<ApplicationUser> baseQuery = _repository
            .WhereReadonly<ApplicationUser>(u =>
                !u.IsDeleted &&
                !u.IsBanned &&
                u.EmailConfirmed &&
                u.Email != null);

        switch (group)
        {
            case AnnouncementRecipientGroup.Admins:
                return await _userManager
                    .GetUsersInRoleAsync(AdminRoleName)
                    .ContinueWith(t => t.Result
                        .Where(u => u is { Email: not null, IsDeleted: false, IsBanned: false, EmailConfirmed: true })
                        .Select(u => u.Email!)
                        .ToArray());

            case AnnouncementRecipientGroup.SubscribedUsers:
                return await baseQuery
                    .Where(u => !_userManager.IsInRoleAsync(u, AdminRoleName).Result)
                    .Select(u => u.Email!)
                    .ToArrayAsync();

            case AnnouncementRecipientGroup.SubscribedUsersAndAdmins:
                string[] admins = await _userManager
                    .GetUsersInRoleAsync(AdminRoleName)
                    .ContinueWith(t => t.Result
                        .Where(u => u is { Email: not null, IsDeleted: false, IsBanned: false, EmailConfirmed: true })
                        .Select(u => u.Email!)
                        .ToArray());

                string[] users = await baseQuery
                    .Where(u => !admins.Contains(u.Email!))
                    .Select(u => u.Email!)
                    .ToArrayAsync();

                return users.Concat(admins).ToArray();

            default:
                return Array.Empty<string>();
        }
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
