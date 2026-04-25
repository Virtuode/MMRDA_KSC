namespace mmrdaconsent.API.DTOs;

public record StateDto(int StateID, string StateName);
public record DistrictDto(int DistrictID, int StateID, string DistrictName);
public record CityDto(int CityID, int StateID, string CityName);
public record TalukaDto(int TalukaID, int DistrictID, string TalukaName);
public record VillageDto(int VillageID, int DistrictID, string VillageName);