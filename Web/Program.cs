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
builder.Services.AddScoped<ISatisfactionService, SatisfactionService>();
builder.Services.AddScoped<IVisioService, VisioService>();
builder.Services.AddScoped<IEmargementService, EmargementService>();
builder.Services.AddScoped<IQuestionnaireMiParcoursService, QuestionnaireMiParcoursService>();
builder.Services.AddScoped<IAttestationService, AttestationService>();
builder.Services.AddScoped<IReclamationService, ReclamationService>();
builder.Services.AddScoped<ITestPositionnementService, TestPositionnementService>();
builder.Services.AddScoped<IDiscService, DiscService>();
builder.Services.AddScoped<IRiasecService, RiasecService>();
builder.Services.AddScoped<IBigFiveService, BigFiveService>();
builder.Services.AddScoped<IPreBilanCarboneService, PreBilanCarboneService>();
builder.Services.AddScoped<IQualiopiDashboardService, QualiopiDashboardService>();

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
app.UseStaticFiles(new StaticFileOptions
{
    // Les assets portant un parametre de version (?v=hash via asp-append-version, cf.
    // _Layout.cshtml) ou places sous /uploads/ (nommage par GUID genere a l'upload, jamais
    // reecrits en place - cf. CartesController.EnregistrerImageAsync) sont immuables par
    // construction : un changement de contenu change toujours l'URL, donc un cache
    // navigateur tres long ne peut jamais servir une version perimee. Les autres assets
    // (pas encore versionnes) gardent un cache plus court.
    OnPrepareResponse = context =>
    {
        var estVersionne = context.Context.Request.Query.ContainsKey("v")
            || context.Context.Request.Path.StartsWithSegments("/uploads");

        context.Context.Response.Headers.CacheControl = estVersionne
            ? "public, max-age=31536000, immutable"
            : "public, max-age=604800";
    },
});
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<DocumentsLegauxMiddleware>();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
