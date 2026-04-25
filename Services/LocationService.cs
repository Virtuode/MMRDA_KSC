using Microsoft.EntityFrameworkCore;
using mmrdaconsent.API.Data;
using mmrdaconsent.API.DTOs;
using mmrdaconsent.API.Interfaces;

namespace mmrdaconsent.API.Services;

public class LocationService : ILocationService
{
    private readonly AppDbContext _context;

    public LocationService(AppDbContext context) => _context = context;

    public async Task<List<StateDto>> GetStatesAsync() =>
        await _context.States
            .Where(s => s.IsActive && !s.Deleted)
            .Select(s => new StateDto(s.StateID, s.StateName))
            .OrderBy(s => s.StateName)
            .ToListAsync();

    public async Task<List<DistrictDto>> GetDistrictsByStateAsync(int stateId) =>
        await _context.Districts
            .Where(d => d.StateID == stateId && d.IsActive && !d.Deleted)
            .Select(d => new DistrictDto(d.DistrictID, d.StateID, d.DistrictName))
            .OrderBy(d => d.DistrictName)
            .ToListAsync();

    public async Task<List<CityDto>> GetCitiesByStateAsync(int stateId) =>
        await _context.Cities
            .Where(c => c.StateID == stateId && c.IsActive && !c.Deleted)
            .Select(c => new CityDto(c.CityID, c.StateID, c.CityName))
            .OrderBy(c => c.CityName)
            .ToListAsync();

    public async Task<List<TalukaDto>> GetTalukasByDistrictAsync(int districtId) =>
        await _context.Talukas
            .Where(t => t.DistrictID == districtId && t.IsActive && !t.Deleted)
            .Select(t => new TalukaDto(t.TalukaID, t.DistrictID, t.TalukaName))
            .OrderBy(t => t.TalukaName)
            .ToListAsync();

    // FIXED: was GetVillagesByTalukaAsync(int districtId) — confusing mismatch
    public async Task<List<VillageDto>> GetVillagesByDistrictAsync(int districtId) =>
        await _context.Villages
            .Where(v => v.DistrictID == districtId && v.IsActive && !v.Deleted)
            .Select(v => new VillageDto(v.VillageID, v.DistrictID, v.VillageName))
            .OrderBy(v => v.VillageName)
            .ToListAsync();
}