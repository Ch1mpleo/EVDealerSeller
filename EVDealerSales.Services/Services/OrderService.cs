using EVDealerSales.Models.DTOs.OrderDTOs;
using EVDealerSales.Models.DTOs.OrderItemDTOs;
using EVDealerSales.Models.Entities;
using EVDealerSales.Models.Interfaces;
using EVDealerSales.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace EVDealerSales.Services.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        //private readonly IClaimsService _claimsService;

        public OrderService(IUnitOfWork unitOfWork, ILogger<OrderService> logger/*, IClaimsService claimsService*/)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            //_claimsService = claimsService;
        }

        public async Task<OrderResponseDto> CreateOrderAsync(OrderDto dto)
        {
            try
            {
                _logger.LogInformation("Starting order creation process.");

                if (dto.QuoteId == Guid.Empty)
                    throw new ArgumentException("Quote ID is required.");
                if (dto.OrderDate == default)
                    throw new ArgumentException("Order date is required.");
                if (dto.SubtotalAmount < 0)
                    throw new ArgumentException("Subtotal amount must be non-negative.");
                if (dto.TotalAmount < 0)
                    throw new ArgumentException("Total amount must be non-negative.");
                if (dto.DiscountValue.HasValue && dto.DiscountValue < 0)
                    throw new ArgumentException("Discount value must be non-negative.");
                if (dto.Items == null)
                    throw new ArgumentException("Order item is required.");
                if (dto.Items.Quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than zero.");
                if (dto.Items.UnitPrice < 0)
                    throw new ArgumentException("Unit price must be non-negative.");

                _logger.LogInformation("Creating a new order for Quote ID: {QuoteId}", dto.QuoteId);

                var existingQuote = await _unitOfWork.Quotes.GetByIdAsync(dto.QuoteId, q => q.Customer, q => q.Staff, q => q.Vehicle);
                if (existingQuote == null || existingQuote.IsDeleted)
                    throw new ArgumentException("Associated quote not found.");

                _logger.LogInformation("Associated quote with ID {QuoteId} found.", dto.QuoteId);

                // Create audit fields
                //var currentUserId = _claimsService.GetCurrentUserId;
                var now = DateTime.UtcNow;

                // Create the single order item using the vehicle from the quote
                var orderItem = new OrderItem
                {
                    VehicleId = existingQuote.VehicleId, // Always use the vehicle from the quote
                    Quantity = dto.Items.Quantity,
                    UnitPrice = dto.Items.UnitPrice,
                    LineTotal = dto.Items.LineTotal,
                    CreatedAt = now,
                    CreatedBy = new Guid("00000000-0000-0000-0000-000000000001") // Placeholder for current user ID
                };

                var order = new Order
                {
                    QuoteId = dto.QuoteId,
                    CustomerId = existingQuote.CustomerId,
                    StaffId = existingQuote.StaffId,
                    OrderDate = dto.OrderDate,
                    Status = dto.Status,
                    DiscountType = dto.DiscountType,
                    DiscountValue = dto.DiscountValue,
                    DiscountNote = dto.DiscountNote,
                    SubtotalAmount = dto.SubtotalAmount,
                    TotalAmount = dto.TotalAmount,
                    CreatedAt = now,
                    CreatedBy = new Guid("00000000-0000-0000-0000-000000000001"),
                    Items = new List<OrderItem> { orderItem }
                };

                var createdOrder = await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully created order with ID {OrderId}.", createdOrder.Id);

                return new OrderResponseDto
                {
                    Id = createdOrder.Id,
                    QuoteId = createdOrder.QuoteId,
                    CustomerId = createdOrder.CustomerId,
                    CustomerName = $"{existingQuote.Customer.FirstName} {existingQuote.Customer.LastName}",
                    StaffId = createdOrder.StaffId,
                    StaffName = existingQuote.Staff.FullName,
                    OrderDate = createdOrder.OrderDate,
                    Status = createdOrder.Status,
                    DiscountType = createdOrder.DiscountType,
                    DiscountValue = createdOrder.DiscountValue,
                    DiscountNote = createdOrder.DiscountNote,
                    SubtotalAmount = createdOrder.SubtotalAmount,
                    TotalAmount = createdOrder.TotalAmount,
                    Items = new OrderItemDto
                    {
                        Id = createdOrder.Items.First().Id,
                        VehicleId = createdOrder.Items.First().VehicleId,
                        Name = existingQuote.Vehicle.ModelName,
                        Quantity = createdOrder.Items.First().Quantity,
                        UnitPrice = createdOrder.Items.First().UnitPrice,
                        LineTotal = createdOrder.Items.First().LineTotal
                    }
                };
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create order. Exception: {Message}", ex.Message);
                throw new Exception("An error occurred while creating the order. Please try again later.");
            }
        }

        public async Task<OrderResponseDto?> GetOrderByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Fetching order with ID: {OrderId}", id);

                var order = await _unitOfWork.Orders.GetByIdAsync(id,
                    o => o.Items,
                    o => o.Customer,
                    o => o.Staff);

                if (order == null || order.IsDeleted)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found or is deleted.", id);
                    return null;
                }

                // Get vehicle information for the order item
                Vehicle? vehicle = null;
                if (order.Items?.Any() == true)
                {
                    vehicle = await _unitOfWork.Vehicles.GetByIdAsync(order.Items.First().VehicleId);
                }

                return new OrderResponseDto
                {
                    Id = order.Id,
                    QuoteId = order.QuoteId,
                    CustomerId = order.CustomerId,
                    CustomerName = $"{order.Customer.FirstName} {order.Customer.LastName}",
                    StaffId = order.StaffId,
                    StaffName = order.Staff.FullName,
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    DiscountType = order.DiscountType,
                    DiscountValue = order.DiscountValue,
                    DiscountNote = order.DiscountNote,
                    SubtotalAmount = order.SubtotalAmount,
                    TotalAmount = order.TotalAmount,
                    Items = order.Items?.FirstOrDefault() != null ? new OrderItemDto
                    {
                        Id = order.Items.First().Id,
                        VehicleId = order.Items.First().VehicleId,
                        Name = vehicle?.ModelName ?? "Unknown Vehicle",
                        Quantity = order.Items.First().Quantity,
                        UnitPrice = order.Items.First().UnitPrice,
                        LineTotal = order.Items.First().LineTotal
                    } : null!
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve order with ID {OrderId}. Exception: {Message}", id, ex.Message);
                throw new Exception("An error occurred while retrieving the order. Please try again later.");
            }
        }

        public async Task<List<ListOrderDto>> GetAllOrdersAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all orders.");

                var orders = await _unitOfWork.Orders.GetAllAsync(
                    o => !o.IsDeleted,
                    o => o.Items,
                    o => o.Customer,
                    o => o.Staff);

                var result = new List<ListOrderDto>();

                foreach (var order in orders)
                {
                    // Get vehicle information for the order item
                    string vehicleModel = "Unknown Vehicle";
                    if (order.Items?.Any() == true)
                    {
                        var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(order.Items.First().VehicleId);
                        vehicleModel = vehicle?.ModelName ?? "Unknown Vehicle";
                    }

                    result.Add(new ListOrderDto
                    {
                        Id = order.Id,
                        CustomerName = $"{order.Customer.FirstName} {order.Customer.LastName}",
                        StaffName = order.Staff.FullName,
                        VehicleModel = vehicleModel,
                        OrderDate = order.OrderDate,
                        Status = order.Status.ToString(),
                        TotalAmount = order.TotalAmount
                    });
                }

                _logger.LogInformation("Retrieved {Count} orders successfully.", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve orders. Exception: {Message}", ex.Message);
                throw new Exception("An error occurred while retrieving orders. Please try again later.");
            }
        }

        public async Task<bool> DeleteOrderAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting order with ID: {OrderId}", id);

                var order = await _unitOfWork.Orders.GetByIdAsync(id, o => o.Items);
                if (order == null || order.IsDeleted)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found or is deleted.", id);
                    return false;
                }

                // Add audit fields before deletion
                //var currentUserId = _claimsService.GetCurrentUserId;
                var now = DateTime.UtcNow;

                order.IsDeleted = true;
                order.DeletedAt = now;
                order.DeletedBy = new Guid("00000000-0000-0000-0000-000000000001");

                // Also mark order items as deleted
                if (order.Items?.Any() == true)
                {
                    foreach (var item in order.Items)
                    {
                        item.IsDeleted = true;
                        item.DeletedAt = now;
                        item.DeletedBy = new Guid("00000000-0000-0000-0000-000000000001");
                    }
                }

                await _unitOfWork.Orders.Update(order);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted order with ID {OrderId}.", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting order with ID: {OrderId}. Exception: {Message}", id, ex.Message);
                return false;
            }
        }
    }
}