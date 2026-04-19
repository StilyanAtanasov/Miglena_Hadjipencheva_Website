using CloudinaryDotNet;
using MHAuthorWebsite.Core;
using MHAuthorWebsite.Core.Admin;
using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Contracts.DataServices;
using MHAuthorWebsite.Core.Background_Services;
using MHAuthorWebsite.Core.Background_Services.Data_Services;
using MHAuthorWebsite.Core.Configuration.EcontApi;
using MHAuthorWebsite.Core.Configuration.EmailConfiguration;
using MHAuthorWebsite.Core.Configuration.EmailConfiguration.Contracts;
using MHAuthorWebsite.Core.Configuration.Security;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Contracts.DataServices;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Data;
using MHAuthorWebsite.Data.DataServices;
using MHAuthorWebsite.Data.DataServices.Admin;
using MHAuthorWebsite.Data.Seeding;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.GCommon;
using MHAuthorWebsite.Infrastructure.Caching;
using MHAuthorWebsite.Infrastructure.Cloudinary;
using MHAuthorWebsite.Infrastructure.Econt;
using MHAuthorWebsite.Infrastructure.Email;
using MHAuthorWebsite.Infrastructure.Rendering;
using MHAuthorWebsite.Web.Common.Localization.Identity;
using MHAuthorWebsite.Web.Infrastructure.Initialization;
using MHAuthorWebsite.Web.Utils.Attributes;
using MHAuthorWebsite.Web.Utils.Authorization;
using MHAuthorWebsite.Web.Utils.Contracts;
using MHAuthorWebsite.Web.Utils.Enums;
using MHAuthorWebsite.Web.Utils.Middleware;
using MHAuthorWebsite.Web.Utils.Providers;
using MHAuthorWebsite.Web.Utils.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RazorLight;
using Serilog;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web host...");

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    if (builder.Environment.IsDevelopment()) builder.Configuration.AddUserSecrets<Program>(optional: true);
    if (builder.Environment.IsStaging()) builder.Configuration.AddEnvironmentVariables(prefix: "Staging__");

    // Add services to the container.
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                              throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    builder.Services
        .AddDefaultIdentity<ApplicationUser>(options =>
        {
            options.User.AllowedUserNameCharacters = null!;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedAccount = true;
        })
        .AddRoles<IdentityRole>()
        .AddErrorDescriber<BulgarianIdentityErrorDescriber>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

    builder.Services.AddAuthentication()
        .AddGoogle(googleOptions =>
        {
            googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
            googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;

            googleOptions.CallbackPath = "/signin-google";
            googleOptions.Events.OnCreatingTicket = ctx =>
            {
                ClaimsIdentity identity = (ClaimsIdentity)ctx.Principal!.Identity!;
                string email = ctx.User.GetProperty("email").GetString()!;
                string name = ctx.User.GetProperty("name").GetString()!;

                // Add claims
                identity.AddClaim(new Claim(ClaimTypes.Email, email));
                identity.AddClaim(new Claim(ClaimTypes.Name, name));
                return Task.CompletedTask;
            };
        })
        .AddMicrosoftAccount(microsoftOptions =>
        {
            microsoftOptions.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"]!;
            microsoftOptions.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"]!;
            microsoftOptions.CallbackPath = "/signin-microsoft";

            microsoftOptions.Events.OnCreatingTicket = ctx =>
            {
                ClaimsIdentity identity = (ClaimsIdentity)ctx.Principal!.Identity!;

                string? email = null;
                if (ctx.User.TryGetProperty("mail", out JsonElement mailProp) &&
                    mailProp.ValueKind != JsonValueKind.Null)
                    email = mailProp.GetString();
                else if (ctx.User.TryGetProperty("userPrincipalName", out JsonElement upnProp) &&
                         upnProp.ValueKind != JsonValueKind.Null)
                    email = upnProp.GetString();

                if (!string.IsNullOrEmpty(email))
                    identity.AddClaim(new Claim(ClaimTypes.Email, email));

                string? name = null;
                if (ctx.User.TryGetProperty("displayName", out JsonElement nameProp) &&
                    nameProp.ValueKind != JsonValueKind.Null)
                    name = nameProp.GetString();
                else if (ctx.User.TryGetProperty("givenName", out JsonElement givenProp) &&
                         givenProp.ValueKind != JsonValueKind.Null)
                    name = givenProp.GetString();

                if (!string.IsNullOrEmpty(name))
                    identity.AddClaim(new Claim(ClaimTypes.Name, name));

                return Task.CompletedTask;
            };
        });

    builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();

    // Data Services
    builder.Services.AddScoped<ICloudinaryAdminProductImageDataService, CloudinaryAdminProductImageDataService>();
    builder.Services.AddScoped<IAdminProductDataService, AdminProductDataService>();
    builder.Services.AddScoped<IAdminOrderDataService, AdminOrderDataService>();

    builder.Services.AddScoped<ICartDataService, CartDataService>();
    builder.Services.AddScoped<IOrderDataService, OrderDataService>();
    builder.Services.AddScoped<IProductCommentDataService, ProductCommentDataService>();
    builder.Services.AddScoped<IProductDataService, ProductDataService>();
    builder.Services.AddScoped<IShipmentUpdateDataService, ShipmentUpdateDataService>();
    builder.Services
        .AddScoped<IScheduledEmailNotificationSenderDataService, ScheduledEmailNotificationSenderDataService>();
    builder.Services.AddScoped<IScheduledNotificationIntegrityDataService, ScheduledNotificationIntegrityDataService>();
    builder.Services.AddScoped<IWorkDataService, WorkDataService>();
    builder.Services.AddScoped<IAdminWorkDataService, AdminWorkDataService>();

    // Core Services
    builder.Services.AddScoped<IImageService, CloudinaryImageService>();
    builder.Services.AddScoped<IAdminProductImageService, CloudinaryAdminProductImageService>();
    builder.Services.AddScoped<ICommentImageService, CloudinaryCommentImageService>();
    builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

    builder.Services.AddScoped<IAdminProductTypeService, AdminProductTypeService>();
    builder.Services.AddScoped<IAdminProductService, AdminProductService>();

    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<IWorkService, WorkService>();
    builder.Services.AddScoped<ISitemapService, SitemapService>();
    builder.Services.AddScoped<IProductCommentService, ProductCommentService>();
    builder.Services.AddScoped<IAdminWorkService, AdminWorkService>();
    builder.Services.AddScoped<ICartService, CartService>();
    builder.Services.AddScoped<IOrderService, OrderService>();
    builder.Services.AddScoped<IAdminOrderService, AdminOrderService>();

    builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
    builder.Services.AddScoped<IAdminUserManagementService, AdminUserManagementService>();

    builder.Services.AddScoped<IAdminAnnouncementsService, AdminAnnouncementsService>();
    builder.Services.AddScoped<IAdminContactRequestsService, AdminContactRequestsService>();
    builder.Services.AddScoped<IAdminNotificationPreferencesService, AdminNotificationPreferencesService>();
    builder.Services.AddScoped<IAdminLegalDocumentsService, AdminLegalDocumentsService>();
    builder.Services.AddScoped<ILegalDocumentsService, LegalDocumentsService>();
    builder.Services.AddScoped<IContactsService, ContactsService>();
    builder.Services.AddHttpClient<IRecaptchaValidationService, RecaptchaValidationService>();

    builder.Services.AddHttpClient<IEcontService, EcontService>();
    builder.Services.AddHttpClient<IAdminEcontService, AdminEcontService>();

    builder.Services.AddScoped<IUrlProvider, UrlProvider>();

    builder.Services.AddScoped<IErrorService, ErrorService>();

    builder.Services.AddHostedService<ShipmentUpdateService>();
    builder.Services.AddHostedService<ScheduledEmailNotificationSenderService>();
    builder.Services.AddHostedService<ScheduledNotificationIntegrityService>();

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.HttpOnly = true;
        options.SlidingExpiration = true;

        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

    builder.Services.AddControllersWithViews(options =>
    {
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
        options.Filters.Add(new SecurityHeadersAttribute(CspFeature.Recaptcha));
    });

    builder.Services.AddOutputCache(options =>
    {
        options.AddPolicy("SitemapPolicy", policyBuilder =>
        {
            policyBuilder.Expire(TimeSpan.FromHours(6));
            policyBuilder.SetVaryByHost(true);
        });
    });

    builder.Services.AddAuthorization(options =>
    {
        AuthorizationPolicy policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new LatestLegalDocumentsAcceptedRequirement())
            .Build();

        options.DefaultPolicy = policy;
        options.AddPolicy("RequireLatestLegalDocuments", policyBuilder =>
        {
            policyBuilder.RequireAuthenticatedUser();
            policyBuilder.AddRequirements(new LatestLegalDocumentsAcceptedRequirement());
        });
    });

    builder.Services.AddScoped<IAuthorizationHandler, LatestLegalDocumentsAcceptedHandler>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DefaultPolicy", p =>
        {
            p.WithOrigins("http://stilyan-001-site1.stempurl.com/")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    string? cloudName = builder.Configuration["Cloudinary:CloudName"];
    string? apiKey = builder.Configuration["Cloudinary:ApiKey"];
    string? apiSecret = builder.Configuration["Cloudinary:ApiSecret"];

    if (new[] { cloudName, apiKey, apiSecret }.Any(string.IsNullOrWhiteSpace))
        throw new ArgumentException("Please specify Cloudinary account details!");

    builder.Services.AddSingleton(new Cloudinary(new Account(cloudName, apiKey, apiSecret)));

    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(
            new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "App_Data", "keys")))
        .SetApplicationName(ApplicationRules.Application.ProjectName);

    builder.Services.Configure<EcontApiSettings>(builder.Configuration.GetSection("Econt"));
    builder.Services.Configure<RecaptchaSettings>(options =>
    {
        options.V2SiteKey = builder.Configuration["Recaptcha:V2SiteKey"]
                            ?? builder.Configuration["RECAPTCHA_V2_SITE_KEY"]
                            ?? string.Empty;

        options.V2SecretKey = builder.Configuration["Recaptcha:V2SecretKey"]
                              ?? builder.Configuration["RECAPTCHA_V2_SECRET_KEY"]
                              ?? string.Empty;

        options.V3SiteKey = builder.Configuration["Recaptcha:V3SiteKey"]
                            ?? builder.Configuration["RECAPTCHA_V3_SITE_KEY"]
                            ?? string.Empty;

        options.V3SecretKey = builder.Configuration["Recaptcha:V3SecretKey"]
                              ?? builder.Configuration["RECAPTCHA_V3_SECRET_KEY"]
                              ?? string.Empty;

        string? minimumScoreRaw = builder.Configuration["Recaptcha:V3MinimumScore"];
        if (double.TryParse(minimumScoreRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out double minimumScore))
            options.V3MinimumScore = minimumScore;
    });

    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
    builder.Services.AddSingleton<IEmailUserProvider, EmailUserProvider>();
    builder.Services.AddTransient<IEmailService, EmailService>();

    builder.Services.AddHttpContextAccessor();

    builder.Services.AddScoped<ICacheService, RedisCacheService>();
    builder.Services.AddScoped<IFastCacheService, RedisCacheService>();
    builder.Services.AddScoped<IGlobalCacheKeysManagementService, GlobalCacheKeysManagementService>();

    string templatePath = Path.Combine(AppContext.BaseDirectory, "NotificationTemplates\\Razor Templates");

    builder.Services.AddSingleton<IRazorLightEngine>(_ => new RazorLightEngineBuilder()
        .UseFileSystemProject(templatePath)
        .UseMemoryCachingProvider()
        .SetOperatingAssembly(typeof(RazorLightRenderingService).Assembly)
        .Build());

    builder.Services.AddScoped<INotificationRenderingService, RazorLightRenderingService>();

    // --- Configure Logger ---
    builder.Host.UseSerilog((context, _, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration));

    WebApplication app = builder.Build();

    AppEnvironment.Initialize(app.Environment.EnvironmentName);
    QueryBridge.Initialize();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error/Error");
        app.UseHsts();
    }

    app.UseForwardedHeaders();

    app.UseStatusCodePagesWithReExecute("/Error/Error/{0}");

    app.UseCookiePolicy(new CookiePolicyOptions
    {
        Secure = CookieSecurePolicy.Always,
        HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always,
        MinimumSameSitePolicy = SameSiteMode.None
    });

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseMiddleware<SeoEnvironmentMiddleware>();

    app.UseRouting();

    app.UseCors("DefaultPolicy");
    app.UseOutputCache();

    app.UseAuthentication();
    app.UseMiddleware<LegalDocumentsAccessMiddleware>();
    app.UseAuthorization();

    string[] supportedCultures = { "bg-BG" };
    RequestLocalizationOptions localizationOptions = new RequestLocalizationOptions()
        .SetDefaultCulture("bg-BG")
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);

    app.UseRequestLocalization(localizationOptions);

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

    app.MapAreaControllerRoute(
        name: "Admin",
        areaName: "Admin",
        pattern: "Admin/{controller}/{action}/{id?}",
        defaults: new { controller = "AdminDashboard", action = "Dashboard" });

    app.MapRazorPages();

    using (IServiceScope scope = app.Services.CreateScope())
    {
        IServiceProvider services = scope.ServiceProvider;
        await AdminSeeder.SeedAsync(services);

        ApplicationDbContext db = services.GetRequiredService<ApplicationDbContext>();
        IImageService imageService = services.GetRequiredService<IImageService>();

        await DbInitializer.GenerateCommentImagesPreviewsAsync(db, imageService);
    }

    Log.Information("The application has started successfully.");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
