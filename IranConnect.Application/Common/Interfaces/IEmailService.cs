namespace IranConnect.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string token, CancellationToken cancellationToken);
    Task SendPasswordResetEmailAsync(string email, string token, CancellationToken cancellationToken);
    Task SendWelcomeEmailAsync(string email, CancellationToken cancellationToken);
}
