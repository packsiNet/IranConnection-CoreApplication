using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IranConnect.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler
    : IRequestHandler<ResetPasswordCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public ResetPasswordCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<string>> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(
                u => u.Email == request.Email.ToLowerInvariant().Trim(),
                cancellationToken);

        if (user is null)
            return Result<string>.Failure("کاربر یافت نشد", 404);

        if (!user.IsPasswordResetTokenValid(request.Token))
            return Result<string>.Failure("توکن نامعتبر یا منقضی شده است", 400);

        var newHash = _passwordHasher.Hash(request.NewPassword);
        user.ResetPassword(newHash);

        // Revoke all refresh tokens on password reset
        foreach (var token in user.RefreshTokens.Where(t => t.IsActive))
            token.Revoke("password reset");

        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("پسورد با موفقیت تغییر کرد");
    }
}
