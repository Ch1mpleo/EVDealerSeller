using EVDealerSales.BO.DTOs.VehicleDTOs;
using EVDealerSales.Services.Utils;

namespace EVDealerSales.Services.Interfaces
{
    public interface IVehicleService
    {
        Task<Pagination<VehicleDto>> GetAllVehicleAsync(string? search, string? sortBy, bool isDescending, int page, int pageSize);
        Task<VehicleDto?> GetVehicleByIdAsync(Guid id);
        Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto dto);
        Task<VehicleDto> UpdateVehicleAsync(Guid id, UpdateVehicleDto dto);
        Task<bool> DeleteVehicleAsync(Guid id);
        Task<List<VehicleModelListDto>> GetVehicleModelListAsync();
    }
}
