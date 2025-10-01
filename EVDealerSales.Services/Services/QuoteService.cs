using EVDealerSales.BO.DTOs.QuoteDTOs;
using EVDealerSales.BO.Enums;
using EVDealerSales.Models.Entities;
using EVDealerSales.Models.Interfaces;
using EVDealerSales.Services.Interfaces;
using EVDealerSales.Services.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVDealerSales.Services.Services
{
    public class QuoteService : IQuoteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<QuoteService> _logger;
        private readonly IClaimsService _claimsService;

        public QuoteService(
            IUnitOfWork unitOfWork,
            ILogger<QuoteService> logger,
            IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _claimsService = claimsService;
        }

        public async Task<QuoteResponseDto> CreateQuoteAsync(QuoteDto quoteDto)
        {
            try
            {
                _logger.LogInformation("Creating new quote for customer: {CustomerId}", quoteDto.CustomerId);

                // Input validation
                if (quoteDto.CustomerId == Guid.Empty)
                    throw new ArgumentException("Customer ID is required.");
                if (quoteDto.VehicleId == Guid.Empty)
                    throw new ArgumentException("Vehicle ID is required.");
                if (quoteDto.QuotedPrice <= 0)
                    throw new ArgumentException("Quoted price must be greater than zero.");
                if (quoteDto.FinalPrice.HasValue && quoteDto.FinalPrice <= 0)
                    throw new ArgumentException("Final price must be greater than zero if provided.");
                if (quoteDto.ValidUntil.HasValue && quoteDto.ValidUntil < DateTime.UtcNow.Date)
                    throw new ArgumentException("Valid until date cannot be in the past.");

                // Verify if customer exists
                var customer = await _unitOfWork.Customers.GetByIdAsync(quoteDto.CustomerId);
                if (customer == null || customer.IsDeleted)
                {
                    _logger.LogWarning("Customer with ID {CustomerId} not found or is deleted", quoteDto.CustomerId);
                    throw new KeyNotFoundException($"Customer with ID {quoteDto.CustomerId} not found");
                }


                // Verify if vehicle exists
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(quoteDto.VehicleId);
                if (vehicle == null || vehicle.IsDeleted)
                {
                    _logger.LogWarning("Vehicle with ID {VehicleId} not found or is deleted", quoteDto.VehicleId);
                    throw new KeyNotFoundException($"Vehicle with ID {quoteDto.VehicleId} not found");
                }

                var currentUserId = _claimsService.GetCurrentUserId;
                // Verify if staff exists
                var staff = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                var now = DateTime.UtcNow;

                var quote = new Quote
                {
                    CustomerId = quoteDto.CustomerId,
                    StaffId = currentUserId,
                    VehicleId = quoteDto.VehicleId,
                    QuotedPrice = quoteDto.QuotedPrice,
                    FinalPrice = quoteDto.FinalPrice,
                    Status = quoteDto.Status,
                    ValidUntil = quoteDto.ValidUntil,
                    Remarks = quoteDto.Remarks,
                    // Audit fields
                    CreatedAt = now,
                    CreatedBy = currentUserId,
                };

                var createdQuote = await _unitOfWork.Quotes.AddAsync(quote);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully created quote with ID: {QuoteId}", createdQuote.Id);

                return new QuoteResponseDto
                {
                    Id = createdQuote.Id,
                    CustomerId = customer.Id,
                    CustomerName = $"{customer.FirstName} {customer.LastName}",
                    StaffId = staff.Id,
                    StaffName = staff.FullName,
                    VehicleId = vehicle.Id,
                    VehicleModel = vehicle.ModelName,
                    QuotedPrice = createdQuote.QuotedPrice,
                    FinalPrice = createdQuote.FinalPrice,
                    Status = createdQuote.Status,
                    ValidUntil = createdQuote.ValidUntil,
                    Remarks = createdQuote.Remarks
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create quote. Exception: {Message}", ex.Message);
                throw new Exception("An error occurred while creating the quote. Please try again later.");
            }
        }

        public async Task<QuoteResponseDto?> GetQuoteByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Fetching quote with ID: {QuoteId}", id);

                var quote = await _unitOfWork.Quotes
                    .GetByIdAsync(id, q => q.Customer, q => q.Staff, q => q.Vehicle);

                if (quote == null || quote.IsDeleted)
                {
                    _logger.LogWarning("Quote with ID {QuoteId} not found or is deleted", id);
                    return null;
                }

                _logger.LogInformation("Successfully retrieved quote with ID: {QuoteId}", id);

                return new QuoteResponseDto
                {
                    Id = quote.Id,
                    CustomerId = quote.CustomerId,
                    CustomerName = $"{quote.Customer.FirstName} {quote.Customer.LastName}",
                    StaffId = quote.StaffId,
                    StaffName = quote.Staff.FullName,
                    VehicleId = quote.VehicleId,
                    VehicleModel = quote.Vehicle.ModelName,
                    QuotedPrice = quote.QuotedPrice,
                    FinalPrice = quote.FinalPrice,
                    Status = quote.Status,
                    ValidUntil = quote.ValidUntil,
                    Remarks = quote.Remarks
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve quote with ID {QuoteId}. Exception: {Message}",
                    id, ex.Message);
                throw new Exception("An error occurred while retrieving the quote. Please try again later.");
            }
        }

        public async Task<Pagination<QuoteListDto>> GetAllQuotesAsync(int pageNumber, int pageSize, string searchTerm = null)
        {
            try
            {
                _logger.LogInformation("Fetching quotes with pagination. Page: {PageNumber}, Size: {PageSize}, SearchTerm: {SearchTerm}",
                    pageNumber, pageSize, searchTerm ?? "none");

                // Input validation
                if (pageNumber <= 0)
                    throw new ArgumentException("Page number must be greater than zero.");
                if (pageSize <= 0 || pageSize > 100)
                    throw new ArgumentException("Page size must be between 1 and 100.");

                var baseQuery = _unitOfWork.Quotes.GetQueryable()
                    .Where(q => !q.IsDeleted);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLower();
                    baseQuery = baseQuery.Where(q =>
                        (!string.IsNullOrEmpty(q.Customer.FirstName) && q.Customer.FirstName.ToLower().Contains(searchLower)) ||
                        (!string.IsNullOrEmpty(q.Customer.LastName) && q.Customer.LastName.ToLower().Contains(searchLower)) ||
                        (!string.IsNullOrEmpty(q.Vehicle.ModelName) && q.Vehicle.ModelName.ToLower().Contains(searchLower))
                    );
                }

                var query = baseQuery
                    .Include(q => q.Customer)
                    .Include(q => q.Staff)
                    .Include(q => q.Vehicle);

                var totalCount = await query.CountAsync();
                var quotes = await query
                    .OrderByDescending(q => q.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(q => new QuoteListDto
                    {
                        Id = q.Id,
                        CustomerName = $"{q.Customer.FirstName} {q.Customer.LastName}",
                        VehicleModel = q.Vehicle.ModelName,
                        Status = q.Status,
                        ValidUntil = q.ValidUntil,
                    })
                    .ToListAsync();

                _logger.LogInformation("Successfully retrieved {Count} quotes out of {Total} total",
                    quotes.Count, totalCount);

                return new Pagination<QuoteListDto>(quotes, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve quotes. Exception: {Message}", ex.Message);
                throw new Exception("An error occurred while retrieving quotes. Please try again later.");
            }
        }

        public async Task<Pagination<QuoteListDto>> GetQuotesByCustomerIdAsync(Guid customerId, int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Fetching quotes for staff with ID: {StaffId}. Page: {PageNumber}, Size: {PageSize}",
                    customerId, pageNumber, pageSize);

                // Input validation
                if (customerId == Guid.Empty)
                    throw new ArgumentException("Staff ID is required.");
                if (pageNumber <= 0)
                    throw new ArgumentException("Page number must be greater than zero.");
                if (pageSize <= 0 || pageSize > 100)
                    throw new ArgumentException("Page size must be between 1 and 100.");

                // Check if staff exists
                var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
                if (customer == null || customer.IsDeleted)
                {
                    _logger.LogWarning("Staff with ID {StaffId} not found or is deleted", customerId);
                    throw new KeyNotFoundException($"Staff with ID {customerId} not found");
                }

                var query = _unitOfWork.Quotes.GetQueryable()
                    .Where(q => q.StaffId == customerId && !q.IsDeleted)
                    .Include(q => q.Customer)
                    .Include(q => q.Vehicle);

                var totalCount = await query.CountAsync();
                var quotes = await query
                    .OrderByDescending(q => q.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(q => new QuoteListDto
                    {
                        Id = q.Id,
                        CustomerName = $"{q.Customer.FirstName} {q.Customer.LastName}",
                        VehicleModel = q.Vehicle.ModelName,
                        Status = q.Status,
                        ValidUntil = q.ValidUntil,
                    })
                    .ToListAsync();

                _logger.LogInformation("Successfully retrieved {Count} quotes for staff {StaffId} out of {Total} total",
                    quotes.Count, customerId, totalCount);

                return new Pagination<QuoteListDto>(quotes, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve quotes for staff with ID: {StaffId}. Exception: {Message}",
                    customerId, ex.Message);
                throw new Exception("An error occurred while retrieving quotes for staff. Please try again later.");
            }
        }

        public async Task<QuoteResponseDto> UpdateQuoteAsync(Guid id, QuoteDto quoteDto)
        {
            try
            {
                _logger.LogInformation("Updating quote with ID: {QuoteId}", id);

                var existingOrder = await _unitOfWork.Orders.FirstOrDefaultAsync(e => e.QuoteId == id);
                if (existingOrder != null)
                {
                    _logger.LogWarning("Cannot update quote with ID {QuoteId} because it is associated with an existing order", id);
                    throw new InvalidOperationException("Cannot update quote because it is associated with an existing order.");
                }

                var staffId = _claimsService.GetCurrentUserId;

                // Input validation
                if (quoteDto.CustomerId == Guid.Empty)
                    throw new ArgumentException("Customer ID is required.");
                if (quoteDto.VehicleId == Guid.Empty)
                    throw new ArgumentException("Vehicle ID is required.");
                if (quoteDto.QuotedPrice <= 0)
                    throw new ArgumentException("Quoted price must be greater than zero.");
                if (quoteDto.FinalPrice.HasValue && quoteDto.FinalPrice <= 0)
                    throw new ArgumentException("Final price must be greater than zero if provided.");

                var quote = await _unitOfWork.Quotes.GetByIdAsync(id);
                if (quote == null || quote.IsDeleted)
                {
                    _logger.LogWarning("Quote with ID {QuoteId} not found or is deleted", id);
                    throw new KeyNotFoundException("Quote not found.");
                }

                bool isUpdated = false;

                // Check if customer exists and update if different
                if (quote.CustomerId != quoteDto.CustomerId)
                {
                    var customer = await _unitOfWork.Customers.GetByIdAsync(quoteDto.CustomerId);
                    if (customer == null || customer.IsDeleted)
                    {
                        _logger.LogWarning("Customer with ID {CustomerId} not found or is deleted", quoteDto.CustomerId);
                        throw new KeyNotFoundException($"Customer with ID {quoteDto.CustomerId} not found");
                    }
                    quote.CustomerId = quoteDto.CustomerId;
                    isUpdated = true;
                }

                // Check if staff exists and update if different
                if (quote.StaffId != staffId)
                {
                    var staff = await _unitOfWork.Users.GetByIdAsync(staffId);
                    if (staff == null || staff.IsDeleted || !staff.IsActive)
                    {
                        _logger.LogWarning("Staff with ID {StaffId} not found, is deleted, or inactive", staffId);
                        throw new KeyNotFoundException($"Staff with ID {staffId} not found or is inactive");
                    }
                    quote.StaffId = staffId;
                    isUpdated = true;
                }

                // Check if vehicle exists and update if different
                if (quote.VehicleId != quoteDto.VehicleId)
                {
                    var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(quoteDto.VehicleId);
                    if (vehicle == null || vehicle.IsDeleted)
                    {
                        _logger.LogWarning("Vehicle with ID {VehicleId} not found or is deleted", quoteDto.VehicleId);
                        throw new KeyNotFoundException($"Vehicle with ID {quoteDto.VehicleId} not found");
                    }
                    quote.VehicleId = quoteDto.VehicleId;
                    isUpdated = true;
                }

                // Update other properties if different
                if (quote.QuotedPrice != quoteDto.QuotedPrice)
                {
                    quote.QuotedPrice = quoteDto.QuotedPrice;
                    isUpdated = true;
                }

                if (quote.FinalPrice != quoteDto.FinalPrice)
                {
                    quote.FinalPrice = quoteDto.FinalPrice;
                    isUpdated = true;
                }

                if (quote.Status != quoteDto.Status)
                {
                    quote.Status = quoteDto.Status;
                    isUpdated = true;
                }

                if (quote.ValidUntil != quoteDto.ValidUntil)
                {
                    if (quoteDto.ValidUntil.HasValue && quoteDto.ValidUntil < DateTime.UtcNow.Date)
                        throw new ArgumentException("Valid until date cannot be in the past.");
                    quote.ValidUntil = quoteDto.ValidUntil;
                    isUpdated = true;
                }

                if (!string.IsNullOrWhiteSpace(quoteDto.Remarks) && quote.Remarks != quoteDto.Remarks)
                {
                    quote.Remarks = quoteDto.Remarks;
                    isUpdated = true;
                }

                if (!isUpdated)
                {
                    _logger.LogWarning("[UpdateQuote] No changes detected for QuoteId: {QuoteId}", id);

                    // Load navigation properties for response
                    var existingQuote = await _unitOfWork.Quotes
                        .GetByIdAsync(id, q => q.Customer, q => q.Staff, q => q.Vehicle);

                    return new QuoteResponseDto
                    {
                        Id = existingQuote.Id,
                        CustomerId = existingQuote.CustomerId,
                        CustomerName = $"{existingQuote.Customer.FirstName} {existingQuote.Customer.LastName}",
                        StaffId = existingQuote.StaffId,
                        StaffName = existingQuote.Staff.FullName,
                        VehicleId = existingQuote.VehicleId,
                        VehicleModel = existingQuote.Vehicle.ModelName,
                        QuotedPrice = existingQuote.QuotedPrice,
                        FinalPrice = existingQuote.FinalPrice,
                        Status = existingQuote.Status,
                        ValidUntil = existingQuote.ValidUntil,
                        Remarks = existingQuote.Remarks
                    };
                }

                // Add audit fields
                quote.UpdatedAt = DateTime.UtcNow;
                quote.UpdatedBy = _claimsService.GetCurrentUserId;

                await _unitOfWork.Quotes.Update(quote);
                await _unitOfWork.SaveChangesAsync();

                // Load updated quote with navigation properties
                var updatedQuote = await _unitOfWork.Quotes
                    .GetByIdAsync(id, q => q.Customer, q => q.Staff, q => q.Vehicle);

                _logger.LogInformation("Successfully updated quote with ID: {QuoteId}", id);

                return new QuoteResponseDto
                {
                    Id = updatedQuote.Id,
                    CustomerId = updatedQuote.CustomerId,
                    CustomerName = $"{updatedQuote.Customer.FirstName} {updatedQuote.Customer.LastName}",
                    StaffId = updatedQuote.StaffId,
                    StaffName = updatedQuote.Staff.FullName,
                    VehicleId = updatedQuote.VehicleId,
                    VehicleModel = updatedQuote.Vehicle.ModelName,
                    QuotedPrice = updatedQuote.QuotedPrice,
                    FinalPrice = updatedQuote.FinalPrice,
                    Status = updatedQuote.Status,
                    ValidUntil = updatedQuote.ValidUntil,
                    Remarks = updatedQuote.Remarks
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update quote with ID {QuoteId}. Exception: {Message}",
                    id, ex.Message);
                throw new Exception("An error occurred while updating the quote. Please try again later.");
            }
        }

        public async Task<bool> DeleteQuoteAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting quote with ID: {QuoteId}", id);

                var quote = await _unitOfWork.Quotes.GetByIdAsync(id);
                if (quote == null || quote.IsDeleted)
                {
                    _logger.LogWarning("Quote with ID {QuoteId} not found or is deleted", id);
                    return false;
                }

                // Add audit fields
                quote.IsDeleted = true;
                quote.DeletedAt = DateTime.UtcNow;
                quote.DeletedBy = _claimsService.GetCurrentUserId;

                await _unitOfWork.Quotes.Update(quote);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted quote with ID: {QuoteId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting quote: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateQuoteStatusAsync(Guid id, QuoteStatus status)
        {
            try
            {
                _logger.LogInformation("Updating status to {Status} for quote with ID: {QuoteId}", status, id);

                var quote = await _unitOfWork.Quotes.GetByIdAsync(id);
                if (quote == null || quote.IsDeleted)
                {
                    _logger.LogWarning("Quote with ID {QuoteId} not found or is deleted", id);
                    return false;
                }

                bool isUpdated = false;

                // Update status if different
                if (quote.Status != status)
                {
                    quote.Status = status;
                    isUpdated = true;
                }

                // Update final price if quote is accepted
                if (status == QuoteStatus.Accepted && quote.FinalPrice == null)
                {
                    quote.FinalPrice = quote.QuotedPrice;
                    isUpdated = true;
                }

                if (!isUpdated)
                {
                    _logger.LogWarning("[UpdateQuoteStatus] No changes detected for QuoteId: {QuoteId}", id);
                    return true;
                }

                // Add audit fields
                quote.UpdatedAt = DateTime.UtcNow;
                quote.UpdatedBy = _claimsService.GetCurrentUserId;

                await _unitOfWork.Quotes.Update(quote);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully updated status to {Status} for quote with ID: {QuoteId}", status, id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating status for quote with ID: {QuoteId}. Exception: {Message}",
                    id, ex.Message);
                return false;
            }
        }

        public async Task<bool> ProcessExpiredQuotesAsync()
        {
            try
            {
                _logger.LogInformation("Processing expired quotes");

                var today = DateTime.UtcNow.Date;
                var expiredQuotes = await _unitOfWork.Quotes.GetAllAsync(
                    q => q.Status == QuoteStatus.Pending && q.ValidUntil.HasValue && q.ValidUntil.Value.Date < today && !q.IsDeleted);

                if (expiredQuotes == null || !expiredQuotes.Any())
                {
                    _logger.LogInformation("No expired quotes found");
                    return false;
                }

                // Add audit fields for each expired quote
                var currentUserId = _claimsService.GetCurrentUserId;
                var now = DateTime.UtcNow;

                foreach (var quote in expiredQuotes)
                {
                    quote.Status = QuoteStatus.Expired;
                    quote.UpdatedAt = now;
                    quote.UpdatedBy = currentUserId;
                }

                await _unitOfWork.Quotes.UpdateRange(expiredQuotes);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully processed {Count} expired quotes", expiredQuotes.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process expired quotes. Exception: {Message}", ex.Message);
                throw new Exception("An error occurred while processing expired quotes. Please try again later.");
            }
        }
    }
}