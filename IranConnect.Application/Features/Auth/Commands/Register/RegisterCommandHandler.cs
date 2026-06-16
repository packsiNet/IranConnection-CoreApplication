using IranConnect.Application.Common.Interfaces;
using IranConnect.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainUser = IranConnect.Domain.Entities.User;
using DomainSubscription = IranConnect.Domain.Entities.Subscription;

namespace IranConnect.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;

    public RegisterCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IEmailService emailService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
    }

    public async Task<Result<RegisterResponse>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == request.Email.ToLowerInvariant().Trim(),
                cancellationToken);

        if (emailExists)
            return Result<RegisterResponse>.Failure(
                "این ایمیل قبلاً ثبت شده است", 409);

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = DomainUser.Create(request.Email, passwordHash, request.FullName);

        var subscription = DomainSubscription.CreateFree(user.Id);
        user.AttachSubscription(subscription);

        _context.Users.Add(user);
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        await _emailService.SendVerificationEmailAsync(
            user.Email,
            user.EmailVerificationToken!,
            cancellationToken);

        return Result<RegisterResponse>.Success(
            new RegisterResponse(
                user.Id.ToString(),
                user.Email,
                "ثبت‌نام موفق. لطفاً ایمیل خود را تایید کنید"),
            201);
    }
}
