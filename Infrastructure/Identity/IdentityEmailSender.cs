using Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Infrastructure.Identity;

public class IdentityEmailSender : IEmailSender
{
    private readonly IEmailService _emailService;

    public IdentityEmailSender(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        await _emailService.EnvoyerAsync(email, subject, htmlMessage);
    }
}