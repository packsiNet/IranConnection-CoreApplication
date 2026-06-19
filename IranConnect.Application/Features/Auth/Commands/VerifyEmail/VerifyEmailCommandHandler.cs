using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandHandler
    : IRequestHandler<VerifyEmailCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public VerifyEmailCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<string>> Handle(
        VerifyEmailCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(
                u => u.Email == request.Email.ToLowerInvariant().Trim(),
                cancellationToken);

        if (user is null)
            return Result<string>.Failure("کاربر یافت نشد", 404);

        if (user.IsEmailVerified)
            return Result<string>.Failure("ایمیل قبلاً تایید شده است", 400);

        if (!user.IsEmailVerificationTokenValid(request.Code))
            return Result<string>.Failure("کد نامعتبر یا منقضی شده است", 400);

        user.VerifyEmail();
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("ایمیل با موفقیت تایید شد");
    }
}
