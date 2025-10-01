using EVDealerSales.BO.DTOs.InvoiceDTOs;
using EVDealerSales.Models.Entities;
using EVDealerSales.Models.Interfaces;
using EVDealerSales.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace EVDealerSales.Services.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly IClaimsService _claimsService;

        public InvoiceService(IUnitOfWork unitOfWork, ILogger<InvoiceService> logger, IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _claimsService = claimsService;
        }

        public async Task<InvoiceResponseDto> CreateInvoiceAsync(InvoiceDto dto)
        {
            try
            {
                _logger.LogInformation("Starting invoice creation process.");

                if (dto.OrderId == Guid.Empty)
                    throw new ArgumentException("Order ID is required.");
                if (dto.TotalAmount < 0)
                    throw new ArgumentException("Total amount must be non-negative.");
                if (dto.TaxAmount.HasValue && dto.TaxAmount < 0)
                    throw new ArgumentException("Tax amount must be non-negative.");
                if (string.IsNullOrWhiteSpace(dto.Notes))
                    throw new ArgumentException("Notes are required.");

                _logger.LogInformation("Creating a new invoice for Order ID: {OrderId}", dto.OrderId);

                var order = await _unitOfWork.Orders.GetByIdAsync(dto.OrderId, o => o.Customer);
                if (order == null || order.IsDeleted)
                    throw new ArgumentException("Associated order not found.");

                _logger.LogInformation("Associated order with ID {OrderId} found.", dto.OrderId);

                var now = DateTime.UtcNow;
                var currentUserId = _claimsService.GetCurrentUserId;
                var listExistingInvoices = await _unitOfWork.Invoices.GetAllAsync(i => i.OrderId == dto.OrderId && !i.IsDeleted);

                int existingCount = listExistingInvoices.Count();
                if (listExistingInvoices.Any())
                    throw new ArgumentException("An invoice for this order already exists.");
                var invoicePrefix = existingCount == 0 ? "INV" : $"INV-{existingCount + 1}";


                var invoice = new Invoice
                {
                    OrderId = order.Id,
                    CustomerId = order.CustomerId,
                    InvoiceNumber = invoicePrefix,
                    DueDate = dto.DueDate,
                    TaxAmount = dto.TaxAmount,
                    TotalAmount = dto.TotalAmount,
                    Status = dto.Status,
                    Notes = dto.Notes,
                    CreatedAt = now,
                    CreatedBy = currentUserId
                };

                var created = await _unitOfWork.Invoices.AddAsync(invoice);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully created invoice with ID {InvoiceId}.", created.Id);

                var customerName = $"{order.Customer.FirstName} {order.Customer.LastName}";

                return new InvoiceResponseDto
                {
                    Id = created.Id,
                    OrderId = created.OrderId,
                    CustomerId = created.CustomerId,
                    CustomerName = customerName,
                    InvoiceNumber = created.InvoiceNumber,
                    DueDate = created.DueDate,
                    TaxAmount = created.TaxAmount,
                    TotalAmount = created.TotalAmount,
                    Status = created.Status,
                    Notes = created.Notes,
                    PaidAmount = 0m
                };
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create invoice. Exception: {Message}", ex.Message);
                throw new Exception("An error occurred while creating the invoice. Please try again later.");
            }
        }

        public async Task<InvoiceResponseDto?> GetInvoiceByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Fetching invoice with ID: {InvoiceId}", id);

                var invoice = await _unitOfWork.Invoices.GetByIdAsync(id,
                    i => i.Customer,
                    i => i.Order,
                    i => i.Payments);

                if (invoice == null || invoice.IsDeleted)
                {
                    _logger.LogWarning("Invoice with ID {InvoiceId} not found or is deleted.", id);
                    return null;
                }

                var paidAmount = invoice.Payments?.Where(p => !p.IsDeleted).Sum(p => p.Amount) ?? 0m;
                var customerName = $"{invoice.Customer.FirstName} {invoice.Customer.LastName}";

                return new InvoiceResponseDto
                {
                    Id = invoice.Id,
                    OrderId = invoice.OrderId,
                    CustomerId = invoice.CustomerId,
                    CustomerName = customerName,
                    InvoiceNumber = invoice.InvoiceNumber,
                    DueDate = invoice.DueDate,
                    TaxAmount = invoice.TaxAmount,
                    TotalAmount = invoice.TotalAmount,
                    Status = invoice.Status,
                    Notes = invoice.Notes,
                    PaidAmount = paidAmount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve invoice with ID {InvoiceId}. Exception: {Message}", id, ex.Message);
                throw new Exception("An error occurred while retrieving the invoice. Please try again later.");
            }
        }

        public async Task<List<ListInvoiceDto>> GetAllInvoicesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all invoices.");

                var invoices = await _unitOfWork.Invoices.GetAllAsync(
                    i => !i.IsDeleted,
                    i => i.Customer,
                    i => i.Order,
                    i => i.Payments);

                var result = new List<ListInvoiceDto>();

                foreach (var invoice in invoices)
                {
                    var customerName = $"{invoice.Customer.FirstName} {invoice.Customer.LastName}";
                    var paidAmount = invoice.Payments?.Where(p => !p.IsDeleted).Sum(p => p.Amount) ?? 0m;

                    result.Add(new ListInvoiceDto
                    {
                        Id = invoice.Id,
                        InvoiceNumber = invoice.InvoiceNumber,
                        OrderId = invoice.OrderId,
                        CustomerName = customerName,
                        DueDate = invoice.DueDate,
                        Status = invoice.Status.ToString(),
                        TotalAmount = invoice.TotalAmount,
                        PaidAmount = paidAmount
                    });
                }

                _logger.LogInformation("Retrieved {Count} invoices successfully.", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve invoices. Exception: {Message}", ex.Message);
                throw new Exception("An error occurred while retrieving invoices. Please try again later.");
            }
        }

        public async Task<bool> DeleteInvoiceAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting invoice with ID: {InvoiceId}", id);

                var invoice = await _unitOfWork.Invoices.GetByIdAsync(id, i => i.Payments);
                if (invoice == null || invoice.IsDeleted)
                {
                    _logger.LogWarning("Invoice with ID {InvoiceId} not found or is deleted.", id);
                    return false;
                }

                var currentUserId = _claimsService.GetCurrentUserId;
                var now = DateTime.UtcNow;

                invoice.IsDeleted = true;
                invoice.DeletedAt = now;
                invoice.DeletedBy = currentUserId;

                if (invoice.Payments?.Any() == true)
                {
                    foreach (var p in invoice.Payments)
                    {
                        p.IsDeleted = true;
                        p.DeletedAt = now;
                        p.DeletedBy = currentUserId;
                    }
                }

                await _unitOfWork.Invoices.Update(invoice);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted invoice with ID {InvoiceId}.", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting invoice with ID: {InvoiceId}. Exception: {Message}", id, ex.Message);
                return false;
            }
        }

        private static string GenerateInvoiceNumber()
        {
            // Simple unique invoice number: INV-YYYYMMDD-xxxx (last 4 of a GUID)
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var suffix = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();
            return $"INV-{date}-{suffix}";
        }
    }
}