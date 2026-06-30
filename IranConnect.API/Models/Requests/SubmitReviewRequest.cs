namespace IranConnect.API.Models.Requests;

public class SubmitReviewRequest
{
    public string FullName { get; set; } = default!;
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
