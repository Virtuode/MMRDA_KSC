using mmrdaconsent.API.DTOs;

namespace mmrdaconsent.API.Interfaces;

public interface ILocationService
{
    Task<List<StateDto>> GetStatesAsync();
    Task<List<DistrictDto>> GetDistrictsByStateAsync(int stateId);
    Task<List<CityDto>> GetCitiesByStateAsync(int stateId);
    Task<List<TalukaDto>> GetTalukasByDistrictAsync(int districtId);
    Task<List<VillageDto>> GetVillagesByDistrictAsync(int districtId); // FIXED: was ByTaluka
}