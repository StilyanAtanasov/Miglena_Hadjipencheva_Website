# MHAuthorWebsite

**MHAuthorWebsite** is a modern, secure, and scalable ASP.NET Core web application dedicated to the Bulgarian author **Miglena Hadjipencheva**. It serves as a public platform for readers to explore her literary works and for the author (and admins) to manage publications, content, and related digital assets efficiently.

---

## ✒️ About the Author

**Miglena Hadjipencheva** is a contemporary Bulgarian writer known for her poetic depth, narrative sensitivity, and evocative prose. This website is a central hub for her works, biography, announcements, and media content.

---

## 🌐 Live Features

- **Public-facing website**:
  - Browse products and literary products
  - Detailed product pages
  - Author biography and press coverage
  - Contact form
- **Admin area** (secured):
  - Role-based access
  - Add/edit/delete  products
  - Upload and manage media
  - View system and product statistics

---

## 📦 Tech Stack

| Layer             | Tech / Library                     |
|------------------|------------------------------------|
| Backend          | ASP.NET Core 8                      |
| Identity/Auth    | ASP.NET Core Identity               |
| ORM              | Entity Framework Core              |
| Database         | SQL Server                         |
| Image Storage    | [Cloudinary](https://cloudinary.com/) |
| Frontend         | Razor Views, FontAwesome           |
| Cloud Hosting    | Cloudinary for image assets        |
| Security         | HTTPS, CSP, CORS, Anti-Forgery     |

---

## ☁️ Cloudinary Integration

Recently, the application was upgraded with **Cloudinary** integration for storing images such as product covers and gallery items in a secure and scalable cloud infrastructure.

### Benefits:
- Automatic image optimization
- AVIF support for modern compression
- Secure delivery (signed URLs)
- Image transformation (resizing, thumbnails)

Images are no longer stored locally but uploaded securely to Cloudinary through signed uploads and stored in a private folder structure.

---

## 🔐 Security Highlights

- CORS policy set to **deny all origins by default**
- CSP headers included to limit loading of external resources
- X-Content-Type-Options, Referrer-Policy, and frame options configured
- CSRF protection with antiforgery tokens
- Cookie security: HTTP-only, secure, and SameSite attributes
- 2FA is enabled and accessible to all registered users 

---

## 🗂 Project Structure - Monolithic three-layer architecture

```
MHAuthorWebsite/
  Areas/
    Admin/          # Admin module (products, dashboard, etc.)
    User/           # Public-facing pages

  Core/
    Contracts/      # Interfaces for services
    Models/         # Domain and DTO models
    Common/         # Shared utilities and constants

  Data/
    Migrations/     # EF Core migrations
    Seeding/        # Admin role/user seeders

  Web/
    Controllers/    # Main user-facing controllers
    ViewModels/     # All view-specific models
    Views/          # Razor views and partials

  wwwroot/          # Static assets (JS, CSS, images)
```


## 🧪 Development & Testing

### Prerequisites:
- .NET 8 SDK
- SQL Server
- Cloudinary account (with API key/secret in user secrets)

### Setup:

## NOTE: Replace all user secret values with valid ones ( the email must be valid and the password must be strong )

```bash
dotnet user-secrets set "Cloudinary:CloudName" "your_cloud_name"
dotnet user-secrets set "Cloudinary:ApiKey" "your_api_key"
dotnet user-secrets set "Cloudinary:ApiSecret" "your_api_secret"
dotnet user-secrets set "AdminPassword" "admin_pass"
dotnet user-secrets set "AdminEmail" "admin_email"
