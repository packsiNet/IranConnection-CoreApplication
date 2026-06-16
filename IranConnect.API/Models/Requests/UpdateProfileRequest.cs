namespace IranConnect.API.Models.Requests;

public record UpdateProfileRequest(
    string? FullName,
    string? CurrentPassword,
    string? NewPassword);
