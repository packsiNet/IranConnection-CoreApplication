using IranConnect.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace IranConnect.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IConfiguration configuration,
        ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendVerificationEmailAsync(string email, string code, CancellationToken cancellationToken)
    {
        await SendEmailAsync(
            email,
            "کد تایید ایمیل IranConnect",
            $"<p>کد تایید ایمیل شما:</p>" +
            $"<h2 style='letter-spacing:4px'>{code}</h2>" +
            $"<p>این کد ۱۵ دقیقه معتبر است.</p>",
            cancellationToken);
    }

    public async Task SendPasswordResetEmailAsync(string email, string code, CancellationToken cancellationToken)
    {
        await SendEmailAsync(
            email,
            "کد بازیابی پسورد IranConnect",
            $"<p>کد بازیابی پسورد شما:</p>" +
            $"<h2 style='letter-spacing:4px'>{code}</h2>" +
            $"<p>این کد ۱۵ دقیقه معتبر است.</p>",
            cancellationToken);
    }

    public async Task SendWelcomeEmailAsync(string email, CancellationToken cancellationToken)
    {
        await SendEmailAsync(
            email,
            "خوش آمدید به IranConnect",
            "<p>حساب کاربری شما با موفقیت فعال شد.</p>",
            cancellationToken);
    }

    private async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _configuration["Email:SenderName"],
                _configuration["Email:SenderEmail"]!));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _configuration["Email:SmtpHost"]!,
                int.Parse(_configuration["Email:SmtpPort"]!),
                SecureSocketOptions.StartTls,
                cancellationToken);
            await client.AuthenticateAsync(
                _configuration["Email:Username"]!,
                _configuration["Email:Password"]!,
                cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", to);
        }
    }
}
