# MHAuthorWebsite

MHAuthorWebsite is a Bulgarian author website and online book/product platform for Miglena Hadjipencheva. It combines a public catalogue of literary works with authenticated reader features, shopping and order management, content publishing, media management, and an administration area.

This README documents the implementation currently present in the repository. It is intentionally detailed because the solution has grown into a multi-project ASP.NET Core application rather than a single-page website.

## Contents

- [Purpose and product idea](#purpose-and-product-idea)
- [Implemented capabilities](#implemented-capabilities)
- [Technology stack](#technology-stack)
- [Architecture](#architecture)
- [Repository layout](#repository-layout)
- [Runtime request flow](#runtime-request-flow)
- [Domain model](#domain-model)
- [Public user experience](#public-user-experience)
- [Identity and account management](#identity-and-account-management)
- [Administration](#administration)
- [Background processing and integrations](#background-processing-and-integrations)
- [Frontend implementation](#frontend-implementation)
- [Security and privacy](#security-and-privacy)
- [Configuration](#configuration)
- [Local setup](#local-setup)
- [Database and migrations](#database-and-migrations)
- [Testing](#testing)
- [Build, run, and publish](#build-run-and-publish)
- [Operational notes and current caveats](#operational-notes-and-current-caveats)
- [Development conventions](#development-conventions)

## Purpose and product idea

The application is designed around two connected ideas:

1. A reader-facing author website where visitors can discover the author's works, read supporting content, contact the author, and view announcements and legal information.
2. A small commerce and content-management system where authenticated users can save products, add them to a cart, place orders, follow shipment updates, discuss products, and manage their account.

The author or an administrator can maintain the catalogue, works, images, announcements, orders, contact requests, users, legal documents, and notification preferences without editing the public Razor views directly.

The solution is a server-rendered, modular monolith. All modules are deployed as one ASP.NET Core web application, but responsibilities are separated across projects and interfaces.

## Implemented capabilities

### Public website

- Home page and author-oriented content.
- Public works catalogue with search, pagination, publicity filtering, and detail pages.
- Product/store catalogue with product cards, search/filter UI, pagination, and detail pages.
- Product images, thumbnails, descriptions, prices, discounts, stock, product types, and dynamic attributes.
- Product likes and a dedicated liked-products page.
- Product comments, star ratings, replies, comment pagination, rating filtering, reactions, editing, deletion, and comment images.
- Contact form and persisted contact requests.
- FAQ, privacy policy, terms of service, and marketing unsubscribe pages.
- Error pages for common HTTP status codes and centralized error handling.
- Sitemap generation with an output-cache policy.

### Shopping and orders

- Cart handling according to the service rules.
- Add-to-cart, quantity updates, item removal, total calculation, and automatic cart cleanup when the last item is removed.
- Checkout/order creation with product snapshots and shipment data.
- Order history, order list, order details, and order-success confirmation flow.
- Shipment and shipment-event persistence.
- Econt integration for shipment services and shipment status processing.
- Prices are represented for the site's Bulgarian and euro display requirements.

### Accounts

- ASP.NET Core Identity registration, login, logout, email confirmation, password reset, and email-change confirmation.
- Confirmed-account requirement before sign-in.
- Google and Microsoft external login.
- Two-factor authentication with authenticator setup, recovery codes, enable/disable, reset, and 2FA login flows.
- Personal data view and deletion flow.
- Password and email management.
- External-login management.
- Email notification preferences.
- Legal-document agreement history and enforcement of the latest required documents.
- Bulgarian Identity validation messages.

### Administration

The admin area is implemented as the `Admin` MVC area and has dedicated controllers, services, view models, Razor views, CSS, and JavaScript.

- Dashboard and system/product statistics.
- Product creation, editing, deletion, publicity toggling, discounts, images, thumbnails, product types, and dynamic product attributes.
- Product-type creation and attribute-definition management.
- Work creation, editing, deletion, cover-image management, and publicity toggling.
- Order list, order details, shipment-service management, and order administration.
- Announcement creation, details, listing, and email-delivery tracking.
- Contact-request listing and details.
- Legal-document listing and editing.
- User-management screens and user summaries/details.
- Administrator notification preferences.
- Quill-based rich-text editing support in admin views.

## Technology stack

### Platform and application framework

- .NET 8 / C#.
- ASP.NET Core MVC with Razor views.
- ASP.NET Core Razor Pages for Identity pages.
- ASP.NET Core Areas for the admin module.
- Nullable reference types and implicit usings are enabled in all projects.
- Out-of-process ASP.NET Core hosting is configured for the web project.

### Persistence

- Entity Framework Core 8.
- `IdentityDbContext<ApplicationUser>` for application data and ASP.NET Identity data.
- SQL Server is the provider configured by the web application's runtime registration.
- SQLite provider references are present in the web project, but the current `Program.cs` registration uses SQL Server.
- EF Core migrations are stored in `MHAuthorWebsite.Data/Migrations`.
- An EF Core InMemory provider is used by service tests.
- Repository abstractions (`IRepository<T>` and `IApplicationRepository`) sit between core services and EF data services.

### Identity and authentication

- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`.
- `Microsoft.AspNetCore.Identity.UI`.
- `Microsoft.AspNetCore.Authentication.Google`.
- `Microsoft.AspNetCore.Authentication.MicrosoftAccount`.
- Role support through `IdentityRole`.

### Infrastructure and external services

- CloudinaryDotNet for product and comment image storage, deletion, linking, thumbnails, previews, and transformations.
- `Microsoft.Extensions.Caching.StackExchangeRedis` with a Redis-backed implementation of the cache contracts.
- `HttpClient` for reCAPTCHA validation and Econt requests.
- Econt services for shipment options and shipment status processing.
- RazorLight for rendering Razor email templates outside the MVC request view pipeline.
- Serilog, Serilog configuration support, and asynchronous file logging.
- ASP.NET Core Data Protection with keys persisted under `App_Data/keys`.

### Frontend

- Razor server-rendered HTML.
- Plain JavaScript organized by feature and shared UI element.
- CSS organized by page, profile, admin, and reusable element.
- jQuery.
- jQuery Validation.
- jQuery Unobtrusive Validation.
- Locally served Bootstrap assets under the Identity area.
- Quill editor integration in admin content editing.
- reCAPTCHA browser integration.
- QR functionality and browser-side time-zone management.

### Testing and developer tooling

- NUnit 3.
- NUnit3TestAdapter.
- NUnit.Analyzers.
- Microsoft.NET.Test.Sdk.
- Moq.
- Coverlet collector.
- EF Core InMemory.
- `Microsoft.VisualStudio.Web.CodeGeneration.Design` for web scaffolding support.
- LibMan configuration exists, but its current library list is empty; the checked-in frontend libraries are already present under `wwwroot`.

## Architecture

The architecture is a layered modular monolith with dependency inversion at the service boundaries:

```text
Browser
  |
  v
MHAuthorWebsite.Web
  Controllers, Areas, Identity Razor Pages, Razor Views, middleware,
  filters, view-model mapping, static assets
  |
  v
MHAuthorWebsite.Core
  Application services, contracts, domain models, DTOs, configuration,
  background-service contracts, service-result handling
  |
  +--> MHAuthorWebsite.Data
  |      EF Core DbContext, entity configuration, repositories,
  |      data services, migrations, seed data
  |
  +--> MHAuthorWebsite.Infrastructure
         Cloudinary, Redis, Econt, email, RazorLight rendering
```

Supporting projects provide shared concerns:

- `MHAuthorWebsite.Core.Common`: small reusable core helpers such as `ServiceResult`, enum extensions, and expression extensions.
- `MHAuthorWebsite.GCommon`: application rules and shared entity constraints/constants.
- `MHAuthorWebsite.Web.Common`: Identity-related shared web resources and Bulgarian validation localization.
- `MHAuthorWebsite.Web.ViewModels`: request, response, admin, profile, product, cart, order, and page view models kept out of the core/domain project.
- `MHAuthorWebsite.Web.Infrastructure`: web startup initialization, including comment-image preview generation.
- `MHAuthorWebsite.Tests.Services`: service-level unit tests using mocked contracts and in-memory EF data.

The core services depend on contracts instead of directly embedding infrastructure details. The web composition root in `Program.cs` maps those contracts to data and infrastructure implementations using ASP.NET Core dependency injection.

## Repository layout

The solution is `MHAuthorWebsite/MHAuthorWebsite.sln` and contains 11 projects:

```text
MHAuthorWebsite/
  MHAuthorWebsite.sln
  MHAuthorWebsite.Web/                 Executable ASP.NET Core application
    Areas/Admin/                       Admin MVC area
    Areas/Identity/                     Customized Identity Razor Pages
    Controllers/                       Public MVC controllers
    Views/                             Public Razor views and partials
    Utils/                             Security, middleware, mappers, helpers
    wwwroot/                           CSS, JavaScript, images, SVG, video, libraries
    Program.cs                         Service registration and HTTP pipeline
  MHAuthorWebsite.Core/                Application services, contracts, models, DTOs
  MHAuthorWebsite.Core.Common/         Shared core utilities
  MHAuthorWebsite.GCommon/             Global rules, constraints, and constants
  MHAuthorWebsite.Data/                EF Core, repositories, data services, migrations
  MHAuthorWebsite.Infrastructure/     Cloudinary, Redis, Econt, email, rendering
  MHAuthorWebsite.Web.Common/          Web localization and Identity shared resources
  MHAuthorWebsite.Web.Infrastructure/ Web initialization services
  MHAuthorWebsite.Web.ViewModels/      MVC/Razor view models
  MHAuthorWebsite.Tests.Services/      NUnit service tests
```

Generated `bin` and `obj` directories are present in the working tree in some environments, but they are build output and are excluded by `.gitignore`. They should not be treated as source architecture.

## Runtime request flow

The application starts in `MHAuthorWebsite.Web/Program.cs`:

1. Serilog creates a bootstrap logger.
2. `WebApplication.CreateBuilder` loads JSON configuration, environment variables, and development user secrets.
3. The SQL Server `ApplicationDbContext` is registered.
4. ASP.NET Identity, roles, external providers, custom Bulgarian error descriptions, and EF stores are registered.
5. Data services, core services, infrastructure services, HTTP clients, cache services, hosted services, authorization handlers, and rendering services are registered.
6. MVC antiforgery and security-header filters are added globally.
7. Data Protection keys, Cloudinary, Econt, reCAPTCHA, email, Redis, and RazorLight are configured.
8. Serilog is connected to application configuration.
9. The middleware pipeline is built.
10. The admin seed operation runs in a scoped service provider.
11. Existing product-comment images without previews are uploaded through the image service and their preview URLs are persisted.
12. MVC routes, the admin area route, and Identity Razor Pages are mapped.

The request pipeline includes forwarded headers, HTTPS redirection, static files, SEO environment middleware, routing, CORS, output caching, authentication, legal-document access middleware, authorization, Bulgarian request localization, and endpoint mapping.

## Domain model

The main EF Core entities are:

### Catalogue and content

- `Product`, `ProductType`, `ProductDiscount`.
- `ProductImage`, `ProductThumbnail`.
- `ProductAttribute`, `ProductAttributeDefinition`, `ProductAttributeOption`.
- `Work` for author works and related public content.

### Reader interaction

- `ProductComment`, `ProductCommentImage`, and `ProductCommentReaction`.
- `Cart` and `CartItem`.
- `ApplicationUser` extending `IdentityUser`.

### Commerce and delivery

- `Order` and `OrderProduct`.
- `Shipment`, `ShipmentEvent`, and `ShipmentService`.

### Communication and administration

- `ContactRequest`.
- `Announcement` and `AnnouncementEmailDelivery`.
- `AdminNotificationPreference`.
- `ScheduledNotification`.

### Legal and compliance

- `LegalDocument`.
- `UserLegalAgreement`.

`ApplicationDbContext` derives from `IdentityDbContext<ApplicationUser>`, exposes all of these sets, applies entity configurations from the data assembly, and applies a DateTime value converter that reads persisted dates as UTC.

## Public user experience

Public controllers include:

- `HomeController`: home, FAQ, privacy, terms, unsubscribe, and related pages.
- `WorkController`: work listing and details.
- `ProductController`: product listing, details, likes, and liked products.
- `ProductCommentController`: add, edit, details, replies, reactions, and comment-image interactions.
- `CartController`: cart display and mutations.
- `OrderController`: checkout, orders, order details, and order-success flow.
- `ContactsController`: contact request submission.
- `ErrorController`: status-code and exception views.

The UI is primarily server-rendered through Razor. JavaScript progressively enhances forms and page interactions with AJAX-style requests, loaders, modals, pagination, search/filter controls, image previews, notifications, cart operations, comment reactions, and profile settings.

## Identity and account management

Identity pages are customized under `Areas/Identity/Pages/Account`. The implementation covers the standard account lifecycle plus the site's business rules:

- A unique email is required.
- Sign-in requires a confirmed account.
- User names accept the application's configured character policy rather than a narrow default character set.
- Cookies are HTTP-only and configured for secure delivery.
- External provider claims are normalized into email and name claims for Google and Microsoft accounts.
- A default authorization policy requires authentication and acceptance of the latest required legal documents.
- The `LatestLegalDocumentsAcceptedHandler` evaluates that requirement.

This means an authenticated user may still be redirected or denied from protected functionality until the latest applicable legal documents have been accepted.

## Administration

Admin controllers inherit from `AdminBaseController` and are grouped in the `Admin` area. The application separates admin orchestration into dedicated core services and data services rather than placing EF queries in controllers.

The admin product workflow supports dynamic product-type attributes, Cloudinary image operations, cover/title-image selection, product publicity, discounts, and editing. The admin work workflow applies the same publication and cover-image concepts to author works. Order and shipment screens connect persisted order data with the Econt service integration.

Announcements can be persisted and delivered through scheduled notification records. Delivery tracking and administrator preferences are modeled explicitly so notification behavior can be controlled without changing the core announcement entity.

## Background processing and integrations

Three hosted services are registered by the web application:

- `ShipmentUpdateService`: processes shipment updates through the shipment data/integration services.
- `ScheduledEmailNotificationSenderService`: sends due scheduled email notifications.
- `ScheduledNotificationIntegrityService`: checks and repairs/maintains scheduled-notification consistency.

### Cloudinary

The Cloudinary integration is split into contracts and specialized implementations:

- General image service.
- Admin product image service.
- Comment image service.
- Cloudinary account/service wrapper.

Product and comment images are represented in the database with URLs and public IDs. The application can upload, link, delete, replace, and generate preview images. Startup initialization finds comment images with an empty preview URL and creates previews through Cloudinary.

### Econt

Econt is accessed through typed HTTP clients (`IEcontService` and `IAdminEcontService`). The integration provides shipment-service data and supports shipment update processing. Its settings are bound from the `Econt` configuration section.

### Email and templates

Email settings are bound to `EmailSettings`. Email rendering uses RazorLight and the copied template at `NotificationTemplates/Razor Templates/OrderStatusUpdateNotificationTemplate.cshtml`. The Core project marks the template for output and publish copying so the infrastructure renderer can find it after deployment.

### Redis

Both the normal cache contract and the fast-cache contract are mapped to `RedisCacheService`. Cache-key management is handled by `GlobalCacheKeysManagementService`. The repository contains the Redis package reference and service mapping; a Redis connection setting must be supplied by the deployment configuration used by the selected implementation.

## Frontend implementation

Static assets are under `MHAuthorWebsite.Web/wwwroot`.

### CSS organization

- Root files cover home, store, cart, orders, products, works, comments, contacts, legal pages, layout, forms, and shared behavior.
- `css/admin` contains dashboard, tables, product/work editors, announcements, legal documents, user management, and admin layout styles.
- `css/profile` contains account, orders, 2FA, email preference, and profile styles.
- `css/elements` contains reusable loaders, modals, notifications, pagination, search bars, selects, sliders, stars, toggles, editors, and delivery components.

### JavaScript organization

- Root scripts implement cart, product details, comments, orders, works, contacts, layout, notifications, recaptcha, loading, QR, and time-zone behavior.
- `js/admin` contains admin product, product-type, announcement, legal-document, work, and user-management behavior.
- `js/account` contains password and forgot-password helpers.
- `js/profile` contains profile, order, email-preference, and legal-agreement behavior.
- `js/elements` contains reusable pagination, search, modal, slider, select, stars, loader, popup, and scroll-to-top behavior.
- `js/utils` and `js/validation` contain image-size and custom-validation helpers.

The project also contains local jQuery, jQuery Validation, and unobtrusive validation assets. This reduces dependence on a runtime CDN for those libraries.

## Security and privacy

The application has security controls at several layers:

- Global automatic antiforgery validation for MVC actions.
- Security-header attribute and CSP policy builder infrastructure.
- HTTPS redirection and HSTS outside development.
- Secure, HTTP-only cookies with an explicit SameSite policy.
- Authentication and authorization through ASP.NET Identity and roles.
- A custom authorization requirement for current legal-document acceptance.
- reCAPTCHA validation service with v2 and v3 settings and a configurable v3 score threshold.
- Error handling that avoids exposing developer exception details outside development.
- Status-code re-execution to the application's user-facing error controller.
- Data Protection key persistence for stable cookie/token behavior across restarts.
- CORS policy registration with an explicit configured origin and credentials.
- Environment-specific Microsoft identity association files under `wwwroot/.well-known`.
- Bulgarian localization for Identity validation messages and request UI culture.

The repository ignores `Secrets`, `App_Data`, publish profiles, and system logs. Credentials, provider secrets, database passwords, and private deployment settings must remain outside committed source.

## Configuration

Configuration is assembled from `appsettings.json`, an environment-specific appsettings file, environment variables, and development user secrets. The web project has a user-secrets ID in its project file.

### Required or integration-specific settings

The exact deployment names should follow the keys read by `Program.cs` and the corresponding options classes:

```text
ConnectionStrings:DefaultConnection
Cloudinary:CloudName
Cloudinary:ApiKey
Cloudinary:ApiSecret
Authentication:Google:ClientId
Authentication:Google:ClientSecret
Authentication:Microsoft:ClientId
Authentication:Microsoft:ClientSecret
Recaptcha:V2SiteKey
Recaptcha:V2SecretKey
Recaptcha:V3SiteKey
Recaptcha:V3SecretKey
Recaptcha:V3MinimumScore
EmailSettings:...
Econt:...
Redis:...
AdminEmail
AdminPassword
```

The application fails fast when Cloudinary account details are missing. It also throws when `ConnectionStrings:DefaultConnection` is absent. OAuth, email, Econt, reCAPTCHA, and Redis values are required for the features that use them.

### Environments

- **Development**: loads optional user secrets, enables the EF migrations endpoint and developer exception page, and uses the development appsettings file.
- **Staging**: adds environment variables with the `Staging__` prefix and serves the staging Microsoft identity association file.
- **Production**: uses the production database template and serves the production Microsoft identity association file; exception details are handled by the normal error route and HSTS is enabled.

`AllowedHosts` is currently set to `*` in the base and production configuration files. Review this value for a production deployment with a known host set.

## Local setup

### Prerequisites

- .NET 8 SDK.
- SQL Server or a compatible SQL Server instance reachable by the configured connection string.
- Cloudinary account and credentials.
- Redis instance for the configured cache implementation.
- Econt credentials/configuration if shipment functionality is enabled.
- SMTP/email provider settings.
- Google and/or Microsoft OAuth application credentials if external login is enabled.
- reCAPTCHA keys for the forms protected by reCAPTCHA.

### Configure secrets

Run the following from `MHAuthorWebsite/MHAuthorWebsite.Web` and replace every placeholder with a real value. Do not commit the resulting secrets.

```powershell
dotnet user-secrets init
dotnet user-secrets set "Cloudinary:CloudName" "your-cloud-name"
dotnet user-secrets set "Cloudinary:ApiKey" "your-api-key"
dotnet user-secrets set "Cloudinary:ApiSecret" "your-api-secret"
dotnet user-secrets set "AdminEmail" "admin@example.com"
dotnet user-secrets set "AdminPassword" "a-strong-password | e.g. Admin!123"
dotnet user-secrets set "Authentication:Google:ClientId" "your-google-client-id"
dotnet user-secrets set "Authentication:Google:ClientSecret" "your-google-client-secret"
dotnet user-secrets set "Authentication:Microsoft:ClientId" "your-microsoft-client-id"
dotnet user-secrets set "Authentication:Microsoft:ClientSecret" "your-microsoft-client-secret"
dotnet user-secrets set "Recaptcha:V3SiteKey" "your-V3SiteKey"
dotnet user-secrets set "Recaptcha:V3SecretKey" "your-V3SecretKey"
dotnet user-secrets set "Recaptcha:V2SiteKey" "your-V2SiteKey"
dotnet user-secrets set "Recaptcha:V2SecretKey" "your-V2SecretKey"
dotnet user-secrets set "EmailSettings:UseSsl" "true/false"
dotnet user-secrets set "EmailSettings:Port" "your-email-server-port"
dotnet user-secrets set "EmailSettings:Host" "your-email-server-host"
dotnet user-secrets set "EmailSettings:NotificationsEmailUser:Username" "your-notifications-email-user-username"
dotnet user-secrets set "EmailSettings:NotificationsEmailUser:Password" "your-notifications-email-user-password"
dotnet user-secrets set "EmailSettings:ContactEmailUser:Username" "your-contact-email-user-username"
dotnet user-secrets set "EmailSettings:ContactEmailUser:Password" "your-contact-email-user-password"
dotnet user-secrets set "Econt:EcontApiShopId" "your-econt-api-shop-id"
dotnet user-secrets set "Econt:EcontApiSecret" "your-econt-api-secret"
dotnet user-secrets set "Redis:RestUrl" "your-redis-rest-url"
dotnet user-secrets set "Redis:RestToken" "your-redis-token"
```

The DB connection is done via ConnectionStrings:DefaultConnection. It is already provided in the appsettings.Development.json file. You can change it as you want.
The local default connection string expects a SQL Server instance named `.` and a database named `MHAuthorWebsite`; change it if your local setup differs.

> #### [⚠️ WARNING ⚠️] If you want to use the Staging environment note that all secrets are expected have a Staging: or Staging** prefix (":" or "**" depends on your setup). For example ConnectionStrings**DefaultConnection becomes Staging**ConnectionStrings\_\_DefaultConnection or ConnectionStrings:DefaultConnection becomes Staging:ConnectionStrings:DefaultConnection

### Restore, migrate, and run

From the solution directory:

```powershell
dotnet restore .\MHAuthorWebsite.sln
dotnet build .\MHAuthorWebsite.sln
dotnet ef database update --project .\MHAuthorWebsite.Data --startup-project .\MHAuthorWebsite.Web
dotnet run --project .\MHAuthorWebsite.Web
```

The development launch profiles expose HTTP at `http://localhost:5186` and HTTPS at `https://localhost:7254` when using the project profile. IIS Express settings are also provided.

At startup, the application seeds the administrator account/role through `AdminSeeder` and generates missing comment-image previews. Ensure the database and Cloudinary configuration are available before starting the web project.

## Database and migrations

The data project contains:

- `ApplicationDbContext`.
- Entity configuration classes under `Data/Configuration`.
- Repository implementations under `Data/Shared`.
- Feature-specific data services under `Data/DataServices` and `Data/DataServices/Admin`.
- EF migrations under `Data/Migrations`.
- `AdminSeeder` under `Data/Seeding`.

The checked-in migrations currently include shipment-service additions and a shipment-service count type change. Create new migrations from the solution directory with the web project as startup project:

```powershell
dotnet ef migrations add YourMigrationName --project .\MHAuthorWebsite.Data --startup-project .\MHAuthorWebsite.Web
dotnet ef database update --project .\MHAuthorWebsite.Data --startup-project .\MHAuthorWebsite.Web
```

Use a separate development database when experimenting with migrations. Production database changes should be reviewed, backed up, and applied through the deployment process.

## Testing

The test project is `MHAuthorWebsite.Tests.Services`. It focuses on core service behavior and uses NUnit, Moq, and EF Core InMemory where appropriate.

Current suites cover:

- `AdminProductServiceTests`: product creation, editing, deletion, publicity, image/attribute behavior, missing records, and failures.
- `AdminProductTypeServiceTests`: product-type listing and dynamic-property creation.
- `AdminWorkServiceTests`: work creation, editing, cover-image replacement, deletion, publicity, and failure handling.
- `CartServiceTests`: cart creation, add/update/remove behavior, stock/product validation, totals, and cleanup.
- `CloudinaryImageServiceTests`: upload, linking, deletion, title-image changes, invalid inputs, and partial failures.
- `ProductServiceTests`: product cards, likes, details, counts, missing users/products, and failure paths.
- `WorkServiceTests`: public visibility, search, paging, details, and counts.

Run all tests with:

```powershell
dotnet test .\MHAuthorWebsite.sln
```

For coverage output through the installed collector, use your normal Coverlet-compatible test-runner options, for example:

```powershell
dotnet test .\MHAuthorWebsite\MHAuthorWebsite.Tests.Services\MHAuthorWebsite.Tests.Services.csproj --collect:"XPlat Code Coverage"
```

The current automated coverage is service-focused. Controller behavior, full Razor rendering, external OAuth providers, real Cloudinary calls, Redis, Econt, SMTP delivery, hosted-service scheduling, and browser workflows require integration or end-to-end testing beyond these unit suites.

## Build, run, and publish

Common commands from `MHAuthorWebsite/`:

```powershell
dotnet restore .\MHAuthorWebsite.sln
dotnet build .\MHAuthorWebsite.sln --configuration Release
dotnet test .\MHAuthorWebsite.sln --configuration Release
dotnet publish .\MHAuthorWebsite.Web\MHAuthorWebsite.Web.csproj --configuration Release --output .\artifacts\publish
```

The web project uses `Microsoft.NET.Sdk.Web`. The Core project copies the Razor email template to output/publish. Production and staging build rules copy the matching Microsoft identity association JSON into `wwwroot/.well-known/microsoft-identity-association.json`.

The repository also contains deployment-related files under `Properties`, including launch settings and service-dependency metadata. Publish profiles are intentionally ignored and must be supplied by the deployment environment.

## Operational notes and current caveats

The following points are important when maintaining or deploying the project:

- The solution targets .NET 8, but several package references use patch versions from different release lines. Keep package upgrades coordinated and verify the full solution after changing them.
- `Microsoft.Extensions.Caching.StackExchangeRedis` is referenced and `RedisCacheService` is registered, but the visible base appsettings file does not show a Redis connection setting. Supply and verify the deployment-specific setting.
- The web project references both SQL Server and SQLite, while `Program.cs` currently configures SQL Server. SQLite should not be assumed to be a supported production runtime without changing and testing the registration.
- Cloudinary credentials are required during application startup, including for the startup preview-generation step.
- The CORS origin in `Program.cs` includes a trailing slash. Validate the effective origin format against the intended deployment host before relying on cross-origin requests.
- `AllowedHosts` is permissive in the visible configuration. Restrict it when deploying behind a known host name.
- The current source includes generated build directories in the workspace listing. Do not document or modify generated `bin`/`obj` output as if it were source.
- The changelog describes a stable 1.0 release and subsequent comment/order/account improvements, while also noting that minor bugs or unimplemented functionality may remain. Treat it as release history rather than a complete feature specification.
- Service unit tests are valuable but do not prove that all external integrations or browser workflows work in a deployed environment.

## Development conventions

- Keep controllers thin: validate/authorize requests, call a core service, and translate the result to a view or HTTP response.
- Put business rules in core services and expose them through interfaces.
- Keep EF queries and persistence operations in data services/repositories.
- Keep external providers behind infrastructure contracts.
- Use view models for Razor input/output rather than exposing persistence entities directly to views.
- Preserve the existing service-result pattern for success, not-found, bad-request, forbidden, and failure outcomes.
- Add or update focused service tests when changing a business workflow.
- Keep credentials and generated output outside source control.
- Maintain Bulgarian localization resources when changing Identity validation or user-facing validation messages.

## License

See [LICENSE](LICENSE) for the repository's license terms.
