namespace IranConnect.API.Models.Requests;

public record ReviewReceiptRequest(bool Approved, string? Note = null);
