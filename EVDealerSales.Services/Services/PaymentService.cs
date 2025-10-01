using EVDealerSales.BO.DTOs.PaymentDTOs;
using EVDealerSales.BO.Enums;
using EVDealerSales.Models.Entities;
using EVDealerSales.Models.Interfaces;
using EVDealerSales.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace EVDealerSales.Services.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly IClaimsService _claimsService;

        public PaymentService(IUnitOfWork unitOfWork, ILogger<PaymentService> logger, IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _claimsService = claimsService;
        }

        public async Task<PaymentResponseDto> CreatePaymentAsync(PaymentDto dto)
        {
            try
            {
                _logger.LogInformation("Starting payment creation process.");

                if (dto.InvoiceId == Guid.Empty)
                    throw new ArgumentException("Invoice ID is required.");
                if (dto.PaymentDate == default)
                    throw new ArgumentException("Payment date is required.");
                if (dto.Amount <= 0)
                    throw new ArgumentException("Amount must be greater than zero.");

                _logger.LogInformation("Creating a new payment for Invoice ID: {InvoiceId}", dto.InvoiceId);

                var invoice = await _unitOfWork.Invoices.GetByIdAsync(dto.InvoiceId, i => i.Customer, i => i.Payments);
                if (invoice == null || invoice.IsDeleted)
                    throw new ArgumentException("Associated invoice not found.");

                if (invoice.Status == InvoiceStatus.Canceled)
                    throw new ArgumentException("Cannot add payment to a canceled invoice.");

                var now = DateTime.UtcNow;
                var currentUserId = _claimsService.GetCurrentUserId;

                var payment = new Payment
                {
                    InvoiceId = invoice.Id,
                    PaymentDate = dto.PaymentDate,
                    Amount = dto.Amount,
                    Status = dto.Status,
                    CreatedAt = now,
                    CreatedBy = currentUserId
                };

                var created = await _unitOfWork.Payments.AddAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                // After saving, update invoice aggregates/status based on PAID payments
                await UpdateInvoiceAggregatesAsync(invoice.Id);

                // Reload invoice with payments to compute response values
                invoice = await _unitOfWork.Invoices.GetByIdAsync(invoice.Id, i => i.Customer, i => i.Payments);
                var paidAmount = invoice!.Payments?.Where(p => !p.IsDeleted && p.Status == PaymentStatus.Paid).Sum(p => p.Amount) ?? 0m;
                var customerName = $"{invoice.Customer.FirstName} {invoice.Customer.LastName}";

                _logger.LogInformation("Successfully created payment with ID {PaymentId}.", created.Id);

                return new PaymentResponseDto
                {
                    Id = created.Id,
                    InvoiceId = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    CustomerId = invoice.CustomerId,
                    CustomerName = customerName,
                    PaymentDate = created.PaymentDate,
                    Amount = created.Amount,
                    Status = created.Status,
                    InvoiceTotalAmount = invoice.TotalAmount,
                    PaidAmount = paidAmount
                };
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create payment. Exception: {Message}", ex.Message);
                throw new Exception("An error occurred while creating the payment. Please try again later.");
            }
        }

        public async Task<PaymentResponseDto?> GetPaymentByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Fetching payment with ID: {PaymentId}", id);

                var payment = await _unitOfWork.Payments.GetByIdAsync(id,
                    p => p.Invoice,
                    p => p.Invoice.Customer,
                    p => p.Invoice.Payments);

                if (payment == null || payment.IsDeleted)
                {
                    _logger.LogWarning("Payment with ID {PaymentId} not found or is deleted.", id);
                    return null;
                }

                var invoice = payment.Invoice!;
                var paidAmount = invoice.Payments?.Where(px => !px.IsDeleted && px.Status == PaymentStatus.Paid).Sum(px => px.Amount) ?? 0m;
                var customerName = $"{invoice.Customer.FirstName} {invoice.Customer.LastName}";

                return new PaymentResponseDto
                {
                    Id = payment.Id,
                    InvoiceId = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    CustomerId = invoice.CustomerId,
                    CustomerName = customerName,
                    PaymentDate = payment.PaymentDate,
                    Amount = payment.Amount,
                    Status = payment.Status,
                    InvoiceTotalAmount = invoice.TotalAmount,
                    PaidAmount = paidAmount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve payment with ID {PaymentId}. Exception: {Message}", id, ex.Message);
                throw new Exception("An error occurred while retrieving the payment. Please try again later.");
            }
        }

        public async Task<List<ListPaymentDto>> GetAllPaymentsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all payments.");

                var payments = await _unitOfWork.Payments.GetAllAsync(
                    p => !p.IsDeleted,
                    p => p.Invoice,
                    p => p.Invoice.Customer);

                var result = new List<ListPaymentDto>();

                foreach (var p in payments)
                {
                    var customerName = $"{p.Invoice.Customer.FirstName} {p.Invoice.Customer.LastName}";
                    result.Add(new ListPaymentDto
                    {
                        Id = p.Id,
                        InvoiceNumber = p.Invoice.InvoiceNumber,
                        CustomerName = customerName,
                        PaymentDate = p.PaymentDate,
                        Amount = p.Amount,
                        Status = p.Status
                    });
                }

                _logger.LogInformation("Retrieved {Count} payments successfully.", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve payments. Exception: {Message}", ex.Message);
                throw new Exception("An error occurred while retrieving payments. Please try again later.");
            }
        }

        public async Task<bool> DeletePaymentAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting payment with ID: {PaymentId}", id);

                var payment = await _unitOfWork.Payments.GetByIdAsync(id);
                if (payment == null || payment.IsDeleted)
                {
                    _logger.LogWarning("Payment with ID {PaymentId} not found or is deleted.", id);
                    return false;
                }

                var currentUserId = _claimsService.GetCurrentUserId;
                var now = DateTime.UtcNow;

                payment.IsDeleted = true;
                payment.DeletedAt = now;
                payment.DeletedBy = currentUserId;

                await _unitOfWork.Payments.Update(payment);
                await _unitOfWork.SaveChangesAsync();

                // After deletion, update invoice aggregates/status
                await UpdateInvoiceAggregatesAsync(payment.InvoiceId);

                _logger.LogInformation("Successfully deleted payment with ID {PaymentId}.", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting payment with ID: {PaymentId}. Exception: {Message}", id, ex.Message);
                return false;
            }
        }

        private async Task UpdateInvoiceAggregatesAsync(Guid invoiceId)
        {
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId, i => i.Payments);
            if (invoice == null || invoice.IsDeleted) return;

            var paidAmount = invoice.Payments?.Where(p => !p.IsDeleted && p.Status == PaymentStatus.Paid).Sum(p => p.Amount) ?? 0m;

            if (invoice.Status != InvoiceStatus.Canceled)
            {
                var now = DateTime.UtcNow.Date;
                if (paidAmount >= invoice.TotalAmount)
                {
                    invoice.Status = InvoiceStatus.Paid;
                }
                else if (invoice.DueDate.HasValue && now > invoice.DueDate.Value.Date)
                {
                    invoice.Status = InvoiceStatus.Overdue;
                }
                else
                {
                    invoice.Status = InvoiceStatus.Unpaid;
                }
            }

            invoice.UpdatedAt = DateTime.UtcNow;
            invoice.UpdatedBy = _claimsService.GetCurrentUserId;

            await _unitOfWork.Invoices.Update(invoice);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}