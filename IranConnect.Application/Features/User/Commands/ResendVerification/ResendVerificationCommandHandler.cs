using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.User.Commands.ResendVerification;

public class ResendVerificationCommandHandler
    : IRequestHandler<ResendVerificationCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public ResendVerificationCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<Result<string>> Handle(
        ResendVerificationCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(
                u => u.Id == request.UserId,
                cancellationToken);

        if (user is null)
            return Result<string>.Failure("کاربر یافت نشد", 404);

        if (user.IsEmailVerified)
            return Result<string>.Failure("ایمیل قبلاً تایید شده است", 400);

        user.RegenerateVerificationToken();
        await _context.SaveChangesAsync(cancellationToken);

        await _emailService.SendVerificationEmailAsync(
            user.Email,
            user.EmailVerificationToken!,
            cancellationToken);

        return Result<string>.Success("ایمیل تایید مجدداً ارسال شد");
    }
}
