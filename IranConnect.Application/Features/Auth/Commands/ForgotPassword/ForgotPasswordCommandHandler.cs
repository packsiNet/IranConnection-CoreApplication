using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler
    : IRequestHandler<ForgotPasswordCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<Result<string>> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(
                u => u.Email == request.Email.ToLowerInvariant().Trim(),
                cancellationToken);

        // Always return success to prevent email enumeration
        if (user is null)
            return Result<string>.Success(
                "اگر این ایمیل ثبت شده باشد، کد بازیابی ارسال می‌شود");

        user.SetPasswordResetToken();
        await _context.SaveChangesAsync(cancellationToken);

        await _emailService.SendPasswordResetEmailAsync(
            user.Email,
            user.PasswordResetToken!,
            cancellationToken);

        return Result<string>.Success(
            "اگر این ایمیل ثبت شده باشد، کد بازیابی ارسال می‌شود");
    }
}
