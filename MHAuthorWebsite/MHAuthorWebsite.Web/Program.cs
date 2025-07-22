using MHAuthorWebsite.Core;
using MHAuthorWebsite.Core.Admin;
using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Data;
using MHAuthorWebsite.Data.Seeding;
using MHAuthorWebsite.Data.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();

builder.Services.AddScoped<IAdminProductTypeService, AdminProductTypeService>();
builder.Services.AddScoped<IAdminProductService, AdminProductService>();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.SlidingExpiration = true;
});

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", p =>
    {
        p.DisallowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error/Error/{0}");

app.UseCookiePolicy(new CookiePolicyOptions
{
    Secure = CookieSecurePolicy.Always,
    HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always,
    MinimumSameSitePolicy = SameSiteMode.Strict
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=()";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "connect-src 'self' " +
                "ws: wss: http://localhost:* https://localhost:*;" +
        "script-src 'self'" +
                " https://site-assets.fontawesome.com " +
                "https://cdn.jsdelivr.net/npm/tom-select@2.4.3/dist/js/tom-select.complete.min.js " +
                "https://cdn.jsdelivr.net/npm/quill@2.0.3/dist/quill.js; " +
        "script-src-elem 'self' " +
                "https://cdn.jsdelivr.net/npm/tom-select@2.4.3/dist/js/tom-select.complete.min.js " +
                "https://cdn.jsdelivr.net/npm/quill@2.0.3/dist/quill.js " +
                "https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/highlight.min.js " +
                "https://cdn.jsdelivr.net/npm/katex@0.16.9/dist/katex.min.js; " +
        "style-src 'self' https://fonts.googleapis.com https://site-assets.fontawesome.com " +
                "https://cdn.jsdelivr.net/npm/tom-select@2.4.3/dist/css/tom-select.css " +
                "https://cdn.jsdelivr.net/npm/quill@2.0.3/dist/quill.snow.css " +
                "https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/atom-one-dark.min.css " +
                "https://cdn.jsdelivr.net/npm/katex@0.16.9/dist/katex.min.css; " +
        "font-src 'self' " +
                "https://fonts.gstatic.com https://site-assets.fontawesome.com; " +
        "img-src 'self' data:;";

    await next();
});

app.UseRouting();

app.UseCors("DefaultPolicy");

app.UseAuthentication();
app.UseAuthorization();

var supportedCultures = new[] { "bg" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("bg")
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
    pattern: "Admin/{controller=Admin}/{action=Dashboard}/{id?}");

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    IServiceProvider services = scope.ServiceProvider;
    await AdminSeeder.SeedAsync(services);
}

app.Run();
