using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Travelogram.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using System.Threading.RateLimiting;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext and configure for SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ApplicationDbContext")));
builder.Services.AddRazorPages();



// Configure Identity with password policies
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    // Password policy configuration
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    // Other Identity configurations
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configure HSTS
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    options.Preload = true;
});

var app = builder.Build();

// Apply migrations and ensure the database is created on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();  // Corrected HSTS usage
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Middleware to add the Content-Security-Policy header
app.Use(async (context, next) =>
{
    string csp = "default-src 'self'; " +
                 "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                 "style-src 'self' 'unsafe-inline'; " +
                 "img-src 'self' data:; " +
                 "font-src 'self'; " +
                 "connect-src 'self'; " +
                 "media-src 'self'; " +
                 "object-src 'self'; " +
                 "child-src 'self'; " +
                 "frame-ancestors 'self'; " +
                 "form-action 'self';";
    context.Response.Headers.Add("Content-Security-Policy", csp);
    await next();
});

// Middleware to add the X-Frame-Options header
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
    await next();
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();


app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();
