namespace mmrdaconsent.API.Entities;

public class City
{
    public int CityID { get; set; }
    public int StateID { get; set; }
    public string CityName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool Deleted { get; set; } = false;
    public int? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public int? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }

    public State State { get; set; } = null!;
}