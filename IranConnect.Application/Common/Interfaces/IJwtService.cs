namespace IranConnect.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(string userId, string email, string plan, bool isAdmin);
    bool ValidateToken(string token);
}
