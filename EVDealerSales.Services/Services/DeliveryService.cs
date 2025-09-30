using EVDealerSales.Models.DTOs.DeliveryDTOs;
using EVDealerSales.Models.Entities;
using EVDealerSales.Models.Enums;
using EVDealerSales.Models.Interfaces;
using EVDealerSales.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace EVDealerSales.Services.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public DeliveryService(IUnitOfWork unitOfWork, ILogger<DeliveryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<DeliveryResponseDto> CreateDeliveryAsync(DeliveryDto dto)
        {
            try
            {
                _logger.LogInformation("Starting delivery creation process. OrderId: {OrderId}", dto.OrderId);

                if (dto.OrderId == Guid.Empty)
                    throw new ArgumentException("Order ID is required.");

                // Load order and ensure only one delivery per order
                var order = await _unitOfWork.Orders.GetByIdAsync(dto.OrderId, o => o.Customer, o => o.Delivery);
                if (order == null || order.IsDeleted)
                    throw new ArgumentException("Associated order not found.");
                if (order.Delivery != null && !order.Delivery.IsDeleted)
                    throw new ArgumentException("This order already has a delivery.");

                var now = DateTime.UtcNow;

                var status = dto.Status ?? DeliveryStatus.Scheduled;

                var delivery = new Delivery
                {
                    OrderId = order.Id,
                    PlannedDate = dto.PlannedDate?.ToUniversalTime(),
                    ActualDate = dto.ActualDate?.ToUniversalTime(),
                    Status = status,
                    CreatedAt = now,
                    CreatedBy = new Guid("00000000-0000-0000-0000-000000000001")
                };

                var created = await _unitOfWork.Deliveries.AddAsync(delivery);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully created delivery with ID {DeliveryId}.", created.Id);

                return new DeliveryResponseDto
                {
                    Id = created.Id,
                    OrderId = created.OrderId,
                    CustomerName = $"{order.Customer.FirstName} {order.Customer.LastName}",
                    PlannedDate = created.PlannedDate,
                    ActualDate = created.ActualDate,
                    Status = created.Status
                };
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create delivery. Exception: {Message}", ex.Message);
                throw new Exception("An error occurred while creating the delivery. Please try again later.");
            }
        }

        public async Task<DeliveryResponseDto?> GetDeliveryByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Fetching delivery with ID: {DeliveryId}", id);

                var delivery = await _unitOfWork.Deliveries.GetByIdAsync(id, d => d.Order, d => d.Order.Customer);
                if (delivery == null || delivery.IsDeleted)
                {
                    _logger.LogWarning("Delivery with ID {DeliveryId} not found or is deleted.", id);
                    return null;
                }

                return new DeliveryResponseDto
                {
                    Id = delivery.Id,
                    OrderId = delivery.OrderId,
                    CustomerName = $"{delivery.Order.Customer.FirstName} {delivery.Order.Customer.LastName}",
                    PlannedDate = delivery.PlannedDate,
                    ActualDate = delivery.ActualDate,
                    Status = delivery.Status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve delivery with ID {DeliveryId}. Exception: {Message}", id, ex.Message);
                throw new Exception("An error occurred while retrieving the delivery. Please try again later.");
            }
        }

        public async Task<List<ListDeliveryDto>> GetAllDeliveriesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all deliveries.");

                var deliveries = await _unitOfWork.Deliveries.GetAllAsync(
                    d => !d.IsDeleted,
                    d => d.Order,
                    d => d.Order.Customer);

                var result = deliveries.Select(d => new ListDeliveryDto
                {
                    Id = d.Id,
                    OrderId = d.OrderId,
                    CustomerName = $"{d.Order.Customer.FirstName} {d.Order.Customer.LastName}",
                    PlannedDate = d.PlannedDate,
                    ActualDate = d.ActualDate,
                    Status = d.Status
                }).ToList();

                _logger.LogInformation("Retrieved {Count} deliveries successfully.", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve deliveries. Exception: {Message}", ex.Message);
                throw new Exception("An error occurred while retrieving deliveries. Please try again later.");
            }
        }

        public async Task<bool> UpdateDeliveryAsync(Guid id, DeliveryDto dto)
        {
            try
            {
                _logger.LogInformation("Updating delivery with ID: {DeliveryId}", id);

                var delivery = await _unitOfWork.Deliveries.GetByIdAsync(id, d => d.Order);
                if (delivery == null || delivery.IsDeleted)
                {
                    _logger.LogWarning("Delivery with ID {DeliveryId} not found or is deleted.", id);
                    return false;
                }

                bool isUpdated = false;

                if (dto.PlannedDate.HasValue && dto.PlannedDate.Value.ToUniversalTime() != delivery.PlannedDate)
                {
                    delivery.PlannedDate = dto.PlannedDate.Value.ToUniversalTime();
                    isUpdated = true;
                }

                if (dto.ActualDate.HasValue && dto.ActualDate.Value.ToUniversalTime() != delivery.ActualDate)
                {
                    delivery.ActualDate = dto.ActualDate.Value.ToUniversalTime();
                    isUpdated = true;
                }

                if (dto.Status.HasValue && dto.Status.Value != delivery.Status)
                {
                    delivery.Status = dto.Status.Value;
                    isUpdated = true;

                    // Implied rules
                    if (delivery.Status == DeliveryStatus.Delivered && delivery.ActualDate == null)
                    {
                        delivery.ActualDate = DateTime.UtcNow;
                    }
                    if (delivery.Status == DeliveryStatus.Scheduled && delivery.ActualDate != null)
                    {
                        delivery.ActualDate = null;
                    }
                }

                if (!isUpdated)
                {
                    _logger.LogWarning("[UpdateDelivery] No changes detected for DeliveryId: {DeliveryId}", id);
                    return true;
                }

                await _unitOfWork.Deliveries.Update(delivery);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully updated delivery with ID: {DeliveryId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update delivery with ID {DeliveryId}. Exception: {Message}", id, ex.Message);
                throw new Exception("An error occurred while updating the delivery. Please try again later.");
            }
        }

        public async Task<bool> UpdateDeliveryStatusAsync(Guid id, DeliveryStatus status)
        {
            try
            {
                _logger.LogInformation("Updating status to {Status} for delivery with ID: {DeliveryId}", status, id);

                var delivery = await _unitOfWork.Deliveries.GetByIdAsync(id);
                if (delivery == null || delivery.IsDeleted)
                {
                    _logger.LogWarning("Delivery with ID {DeliveryId} not found or is deleted.", id);
                    return false;
                }

                if (delivery.Status == status)
                {
                    _logger.LogInformation("Delivery {DeliveryId} already in status {Status}", id, status);
                    return true;
                }

                delivery.Status = status;

                // Business rules
                if (status == DeliveryStatus.InTransit && delivery.PlannedDate == null)
                {
                    // backfill planned date to now if missing
                    delivery.PlannedDate = DateTime.UtcNow;
                }
                if (status == DeliveryStatus.Delivered && delivery.ActualDate == null)
                {
                    delivery.ActualDate = DateTime.UtcNow;
                }
                if (status == DeliveryStatus.Scheduled)
                {
                    delivery.ActualDate = null;
                }

                await _unitOfWork.Deliveries.Update(delivery);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully updated status to {Status} for delivery with ID: {DeliveryId}", status, id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating status for delivery with ID: {DeliveryId}. Exception: {Message}", id, ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteDeliveryAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting delivery with ID: {DeliveryId}", id);

                var delivery = await _unitOfWork.Deliveries.GetByIdAsync(id);
                if (delivery == null || delivery.IsDeleted)
                {
                    _logger.LogWarning("Delivery with ID {DeliveryId} not found or is already deleted.", id);
                    return false;
                }

                delivery.IsDeleted = true;
                delivery.DeletedAt = DateTime.UtcNow;
                delivery.DeletedBy = new Guid("00000000-0000-0000-0000-000000000001");

                await _unitOfWork.Deliveries.Update(delivery);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted delivery with ID {DeliveryId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting delivery with ID: {DeliveryId}. Exception: {Message}", id, ex.Message);
                return false;
            }
        }
    }
}