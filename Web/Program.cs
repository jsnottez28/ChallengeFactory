using Application.Common.Interfaces;
using Infrastructure.ExternalServices.Email;
using Infrastructure.ExternalServices.Stockage;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Web.Configuration;
using Web.Data;
using Web.Security;
using Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Overrides locaux non commites (ConnectionStrings, EmailSettings...). Charge apres
// appsettings.{Environment}.json pour pouvoir tout ecraser, avant les variables
// d'environnement/secrets utilisateur qui restent prioritaires.
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager<ApplicationSignInManager>();

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IEmailService>(sp =>
{
    var env = sp.GetRequiredService<IHostEnvironment>();
    var settings = sp.GetRequiredService<IOptions<EmailSettings>>().Value;

    if (env.IsDevelopment() && (settings.UseConsoleEmail || string.IsNullOrWhiteSpace(settings.SmtpHost)))
    {
        return new ConsoleEmailService();
    }

    return new SmtpEmailService(Options.Create(settings));
});

builder.Services.AddTransient<IEmailSender, IdentityEmailSender>();
builder.Services.Configure<ExternalApiOptions>(
    builder.Configuration.GetSection(ExternalApiOptions.SectionName));
builder.Services.AddScoped<IOrganisationService, OrganisationService>();
builder.Services.AddScoped<IRessourceService, RessourceService>();
builder.Services.AddScoped<ITypeActionService, TypeActionService>();
builder.Services.AddScoped<IGroupeDroitService, GroupeDroitService>();
builder.Services.AddScoped<IDroitService, DroitService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IDocumentLegalService, DocumentLegalService>();
builder.Services.AddScoped<ICarteCompetenceService, CarteCompetenceService>();
builder.Services.AddScoped<ICarteApprenantService, CarteApprenantService>();
builder.Services.AddScoped<IChallengeService, ChallengeService>();
builder.Services.AddScoped<ICohorteService, CohorteService>();
builder.Services.AddScoped<IPreuveService, PreuveService>();
builder.Services.AddScoped<IForumService, ForumService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.Configure<PreuveFichierStockageSettings>(
    builder.Configuration.GetSection("PreuveFichierStockage"));
builder.Services.AddScoped<IPreuveFichierStockageService, LocalDiskPreuveFichierStockageService>();

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IClaimsTransformation, DroitsClaimsTransformation>();
builder.Services.AddSingleton<IAuthorizationHandler, DroitAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, DroitPolicyProvider>();
builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<DocumentsLegauxMiddleware>();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
