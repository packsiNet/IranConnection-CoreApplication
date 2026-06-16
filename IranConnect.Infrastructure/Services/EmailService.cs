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

    public async Task SendVerificationEmailAsync(string email, string token, CancellationToken cancellationToken)
    {
        var verifyUrl =
            $"{_configuration["App:BaseUrl"]}/api/auth/verify-email" +
            $"?email={Uri.EscapeDataString(email)}&token={token}";

        await SendEmailAsync(
            email,
            "تایید ایمیل IranConnect",
            $"<p>برای تایید ایمیل خود روی لینک زیر کلیک کنید:</p>" +
            $"<a href='{verifyUrl}'>{verifyUrl}</a>" +
            $"<p>این لینک ۲۴ ساعت معتبر است.</p>",
            cancellationToken);
    }

    public async Task SendPasswordResetEmailAsync(string email, string token, CancellationToken cancellationToken)
    {
        var resetUrl =
            $"{_configuration["App:BaseUrl"]}/reset-password" +
            $"?email={Uri.EscapeDataString(email)}&token={token}";

        await SendEmailAsync(
            email,
            "بازیابی پسورد IranConnect",
            $"<p>برای بازیابی پسورد روی لینک زیر کلیک کنید:</p>" +
            $"<a href='{resetUrl}'>{resetUrl}</a>" +
            $"<p>این لینک ۱ ساعت معتبر است.</p>",
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
