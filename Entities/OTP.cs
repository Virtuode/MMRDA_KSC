namespace mmrdaconsent.API.Entities;

public class OTP
{
    public int OTPID { get; set; }
    public string MobileNo { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public DateTime ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
    public bool Deleted { get; set; } = false;
    public int? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public int? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}