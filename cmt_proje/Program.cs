using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using cmt_proje.Core.Entities;
using cmt_proje.Infrastructure.Data;
using cmt_proje.Infrastructure.Identity; // <<< EKLEND�

var builder = WebApplication.CreateBuilder(args);

// Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// DbContext
builder.Services.AddDbContext<ConferenceDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity + Roles
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()                     // <<< ROL DESTE��
    .AddEntityFrameworkStores<ConferenceDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// ---- ROL & CHAIR SEED ----
await IdentitySeed.SeedRolesAndChairAsync(app.Services);
// ---------------------------

// ---- FIX AboutContents TABLE ----
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ConferenceDbContext>();
    try
    {
        // Check and add missing columns to AboutContents table
        var sql = @"
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[AboutContents]') AND name = 'ImageUrl')
            BEGIN
                ALTER TABLE [AboutContents] ADD [ImageUrl] nvarchar(500) NULL;
            END

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[AboutContents]') AND name = 'LinkText')
            BEGIN
                ALTER TABLE [AboutContents] ADD [LinkText] nvarchar(200) NULL;
            END

            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[AboutContents]') AND name = 'LinkUrl')
            BEGIN
                ALTER TABLE [AboutContents] ADD [LinkUrl] nvarchar(500) NULL;
            END
        ";
        
        await context.Database.ExecuteSqlRawAsync(sql);
    }
    catch (Exception ex)
    {
        // Log error but don't stop application startup
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Error while fixing AboutContents table columns. This is usually safe to ignore if columns already exist.");
    }
}
// ---------------------------

// HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();   // �nemli
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();       // Identity UI i�in

app.Run();
