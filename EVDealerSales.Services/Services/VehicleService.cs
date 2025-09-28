using EVDealerSales.Models.Commons;
using EVDealerSales.Models.DTOs.VehicleDTOs;
using EVDealerSales.Models.Entities;
using EVDealerSales.Models.Interfaces;
using EVDealerSales.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EVDealerSales.Services.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public VehicleService(IUnitOfWork unitOfWork, ILogger<VehicleService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.ModelName))
                    throw new ArgumentException("Model name is required.");
                if (string.IsNullOrWhiteSpace(dto.TrimName))
                    throw new ArgumentException("Trim name is required.");
                if (!dto.ModelYear.HasValue || dto.ModelYear < 1886 || dto.ModelYear > DateTime.Now.Year + 1)
                    throw new ArgumentException("Model year is invalid.");
                if (dto.BasePrice < 0)
                    throw new ArgumentException("Base price must be non-negative.");
                if (dto.BatteryCapacity < 0)
                    throw new ArgumentException("Battery capacity must be non-negative.");
                if (dto.RangeKM < 0)
                    throw new ArgumentException("RangeKM must be non-negative.");
                if (dto.ChargingTime < 0)
                    throw new ArgumentException("Charging time must be non-negative.");
                if (dto.TopSpeed < 0)
                    throw new ArgumentException("Top speed must be non-negative.");
                if (!string.IsNullOrWhiteSpace(dto.ImageUrl) && !Uri.IsWellFormedUriString(dto.ImageUrl, UriKind.Absolute))
                    throw new ArgumentException("ImageUrl is not a valid URL.");

                var vehicle = new Vehicle
                {
                    ModelName = dto.ModelName,
                    TrimName = dto.TrimName,
                    ModelYear = dto.ModelYear ?? 0,
                    BasePrice = dto.BasePrice,
                    ImageUrl = dto.ImageUrl,
                    BatteryCapacity = dto.BatteryCapacity,
                    RangeKM = dto.RangeKM,
                    ChargingTime = dto.ChargingTime,
                    TopSpeed = dto.TopSpeed,
                    IsActive = dto.IsActive
                };

                var createdVehicle = await _unitOfWork.Vehicles.AddAsync(vehicle);
                await _unitOfWork.SaveChangesAsync();

                return new VehicleDto
                {
                    Id = createdVehicle.Id,
                    ModelName = createdVehicle.ModelName,
                    TrimName = createdVehicle.TrimName,
                    ModelYear = createdVehicle.ModelYear ?? 0,
                    BasePrice = createdVehicle.BasePrice,
                    ImageUrl = createdVehicle.ImageUrl,
                    BatteryCapacity = createdVehicle.BatteryCapacity,
                    RangeKM = createdVehicle.RangeKM,
                    ChargingTime = createdVehicle.ChargingTime,
                    TopSpeed = createdVehicle.TopSpeed,
                    IsActive = createdVehicle.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create vehicle. Exception: {ex.Message}");
                throw new Exception("An error occurred while creating the vehicle. Please try again later.");
            }
        }

        public async Task<bool> DeleteVehicleAsync(Guid id)
        {
            try
            {
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id);
                if (vehicle == null || vehicle.IsDeleted)
                {
                    _logger.LogWarning($"Vehicle with ID {id} not found or is deleted.");
                    return false;
                }

                await _unitOfWork.Vehicles.SoftRemove(vehicle);
                await _unitOfWork.SaveChangesAsync();
                       
                _logger.LogWarning($"Successfully deleted vehicle with {id}.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while deleting vehicle : {ex.Message}");
                return false;
            }
        }

        public async Task<Pagination<VehicleDto>> GetAllVehicleAsync(string? search, string? sortBy, bool isDescending, int page, int pageSize)
        {
            try
            {
                _logger.LogInformation($"Fetching vehicles - Page {page}, PageSize {pageSize}, Search: {search}");

                var listVehicles = await _unitOfWork.Vehicles.GetAllAsync();

                var vehicles = listVehicles.Where(v => !v.IsDeleted).AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    vehicles = vehicles.Where(v =>
                        (!string.IsNullOrEmpty(v.ModelName) && v.ModelName.ToLower().Contains(searchLower)) ||
                        (!string.IsNullOrEmpty(v.TrimName) && v.TrimName.ToLower().Contains(searchLower))
                    );
                }

                var totalVehicles = vehicles.Count();

                vehicles = sortBy?.ToLower() switch
                {
                    "modelname" => isDescending ? vehicles.OrderByDescending(v => v.ModelName) : vehicles.OrderBy(v => v.ModelName),
                    "modelyear" => isDescending ? vehicles.OrderByDescending(v => v.ModelYear) : vehicles.OrderBy(v => v.ModelYear),
                    "baseprice" => isDescending ? vehicles.OrderByDescending(v => v.BasePrice) : vehicles.OrderBy(v => v.BasePrice),
                    _ => vehicles.OrderBy(v => v.Id)
                };

                var pagedVehicles = vehicles
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var result = pagedVehicles.Select(v => new VehicleDto
                {
                    Id = v.Id,
                    ModelName = v.ModelName,
                    TrimName = v.TrimName,
                    ModelYear = v.ModelYear ?? 0,
                    BasePrice = v.BasePrice,
                    ImageUrl = v.ImageUrl,
                    BatteryCapacity = v.BatteryCapacity,
                    RangeKM = v.RangeKM,
                    ChargingTime = v.ChargingTime,
                    TopSpeed = v.TopSpeed,
                    IsActive = v.IsActive
                }).ToList();

                _logger.LogInformation($"Retrieved {result.Count} vehicles on page {page} successfully.");

                return new Pagination<VehicleDto>(result, totalVehicles, page, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to retrieve vehicles. Exception: {ex.Message}");
                throw new Exception("An error occurred while retrieving vehicles. Please try again later.");
            }
        }

        public async Task<VehicleDto?> GetVehicleByIdAsync(Guid id)
        {
            try
            {
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id);
                if (vehicle == null || vehicle.IsDeleted)
                {
                    _logger.LogWarning($"Vehicle with ID {id} not found or is deleted.");
                    return null;
                }

                return new VehicleDto
                {
                    Id = vehicle.Id,
                    ModelName = vehicle.ModelName,
                    TrimName = vehicle.TrimName,
                    ModelYear = vehicle.ModelYear ?? 0,
                    BasePrice = vehicle.BasePrice,
                    ImageUrl = vehicle.ImageUrl,
                    BatteryCapacity = vehicle.BatteryCapacity,
                    RangeKM = vehicle.RangeKM,
                    ChargingTime = vehicle.ChargingTime,
                    TopSpeed = vehicle.TopSpeed,
                    IsActive = vehicle.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to retrieve vehicle with ID {id}. Exception: {ex.Message}");
                throw new Exception("An error occurred while retrieving the vehicle. Please try again later.");
            }
        }

        public async Task<VehicleDto> UpdateVehicleAsync(Guid id, UpdateVehicleDto dto)
        {
            try
            {
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id);
                if (vehicle == null || vehicle.IsDeleted)
                {
                    _logger.LogWarning($"Vehicle with ID {id} not found or is deleted.");
                    throw new KeyNotFoundException("Vehicle not found.");
                }

                bool isUpdated = false;

                if (!string.IsNullOrWhiteSpace(dto.ModelName) && dto.ModelName != vehicle.ModelName)
                {
                    vehicle.ModelName = dto.ModelName;
                    isUpdated = true;
                }

                if (!string.IsNullOrWhiteSpace(dto.TrimName) && dto.TrimName != vehicle.TrimName)
                {
                    vehicle.TrimName = dto.TrimName;
                    isUpdated = true;
                }

                if (dto.ModelYear.HasValue && dto.ModelYear != vehicle.ModelYear)
                {
                    if (dto.ModelYear < 1886)
                        throw new ArgumentException("ModelYear is invalid.");
                    vehicle.ModelYear = dto.ModelYear;
                    isUpdated = true;
                }

                if (!string.IsNullOrWhiteSpace(dto.ImageUrl) && dto.ImageUrl != vehicle.ImageUrl)
                {
                    vehicle.ImageUrl = dto.ImageUrl;
                    isUpdated = true;
                }

                if (dto.BatteryCapacity != vehicle.BatteryCapacity)
                {
                    vehicle.BatteryCapacity = dto.BatteryCapacity;
                    isUpdated = true;
                }

                if (dto.RangeKM != vehicle.RangeKM)
                {
                    vehicle.RangeKM = dto.RangeKM;
                    isUpdated = true;
                }

                if (dto.ChargingTime != vehicle.ChargingTime)
                {
                    vehicle.ChargingTime = dto.ChargingTime;
                    isUpdated = true;
                }

                if (dto.TopSpeed != vehicle.TopSpeed)
                {
                    vehicle.TopSpeed = dto.TopSpeed;
                    isUpdated = true;
                }

                if (dto.BasePrice >= 0 && vehicle.BasePrice != dto.BasePrice)
                {
                    vehicle.BasePrice = dto.BasePrice;
                    isUpdated = true;
                }

                if (vehicle.IsActive != dto.IsActive)
                {
                    vehicle.IsActive = dto.IsActive;
                    isUpdated = true;
                }

                if (!isUpdated)
                {
                    _logger.LogWarning($"[UpdateVehicle] No changes detected for VehicleId: {id}");
                    return new VehicleDto
                    {
                        Id = vehicle.Id,
                        ModelName = vehicle.ModelName,
                        TrimName = vehicle.TrimName,
                        ModelYear = vehicle.ModelYear,
                        BasePrice = vehicle.BasePrice,
                        ImageUrl = vehicle.ImageUrl,
                        BatteryCapacity = vehicle.BatteryCapacity,
                        RangeKM = vehicle.RangeKM,
                        ChargingTime = vehicle.ChargingTime,
                        TopSpeed = vehicle.TopSpeed,
                        IsActive = vehicle.IsActive
                    };
                }

                await _unitOfWork.Vehicles.Update(vehicle);
                await _unitOfWork.SaveChangesAsync();

                return new VehicleDto
                {
                    Id = vehicle.Id,
                    ModelName = vehicle.ModelName,
                    TrimName = vehicle.TrimName,
                    ModelYear = vehicle.ModelYear,
                    BasePrice = vehicle.BasePrice,
                    ImageUrl = vehicle.ImageUrl,
                    BatteryCapacity = vehicle.BatteryCapacity,
                    RangeKM = vehicle.RangeKM,
                    ChargingTime = vehicle.ChargingTime,
                    TopSpeed = vehicle.TopSpeed,
                    IsActive = vehicle.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update vehicle with ID {id}. Exception: {ex.Message}");
                throw new Exception("An error occurred while updating the vehicle. Please try again later.");
            }
        }
    }
}
