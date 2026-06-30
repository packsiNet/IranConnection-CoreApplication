namespace IranConnect.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(string userId, string email, string plan, bool isAdmin, bool showAds);
    bool ValidateToken(string token);
}
