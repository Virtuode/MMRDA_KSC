
namespace mmrdaconsent.API.Interfaces;

public class TataSmsSettings
{
    public string BaseUrl { get; set; } = null!;
    public string User { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string SenderId { get; set; } = null!;
    public string PeId { get; set; } = null!;
    public string LoginTemplateId { get; set; } = null!;
}