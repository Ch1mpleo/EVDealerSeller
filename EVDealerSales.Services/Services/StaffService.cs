using EVDealerSales.Models.Commons;
using EVDealerSales.Models.DTOs.CustomerDTOs;
using EVDealerSales.Models.Interfaces;
using EVDealerSales.Services.Interfaces;
using EVDealerSales.Services.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVDealerSales.Services.Services
{
    public class StaffService : IStaffService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public StaffService(IUnitOfWork unitOfWork, ILogger<StaffService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        #region Customer Management

        public async Task<Pagination<GetCustomerDto>> GetCustomersAsync(int pageNumber, int pageSize, string searchTerm = null)
        {
            try
            {
                _logger.LogInformation("Fetching customers with pagination. Page: {PageNumber}, Size: {PageSize}, SearchTerm: {SearchTerm}",
                    pageNumber, pageSize, searchTerm ?? "none");

                var query = _unitOfWork.Customers.GetQueryable().Where(c => !c.IsDeleted);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(c =>
                        c.FirstName.ToLower().Contains(searchTerm) ||
                        c.LastName.ToLower().Contains(searchTerm) ||
                        c.Email.ToLower().Contains(searchTerm) ||
                        c.Phone.Contains(searchTerm) ||
                        c.Address.ToLower().Contains(searchTerm));
                }

                var totalCount = await query.CountAsync();
                var customers = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new GetCustomerDto
                    {
                        Id = c.Id,
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        Email = c.Email,
                        Phone = c.Phone,
                        Address = c.Address
                    })
                    .ToListAsync();

                _logger.LogInformation("Successfully retrieved {Count} customers out of {Total} total",
                    customers.Count, totalCount);

                return new Pagination<GetCustomerDto>(customers, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching customers. Message: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<GetCustomerDto> GetCustomerByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Fetching customer with ID: {CustomerId}", id);

                var customer = await _unitOfWork.Customers.GetByIdAsync(id);
                if (customer == null || customer.IsDeleted)
                {
                    _logger.LogWarning("Customer with ID {CustomerId} not found or is deleted", id);
                    throw ErrorHelper.NotFound($"Customer with ID {id} not found");
                }

                _logger.LogInformation("Successfully retrieved customer with ID: {CustomerId}", id);

                return new GetCustomerDto
                {
                    Id = customer.Id,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email,
                    Phone = customer.Phone,
                    Address = customer.Address
                };
            }
            catch (Exception ex) when (!(ex.Data.Contains("StatusCode") && (int)ex.Data["StatusCode"] == 404))
            {
                _logger.LogError(ex, "Error occurred while fetching customer with ID: {CustomerId}. Message: {Message}",
                    id, ex.Message);
                throw;
            }
        }

        public async Task<bool> UpdateCustomerAsync(Guid id, GetCustomerDto customerDto)
        {
            try
            {
                _logger.LogInformation("Updating customer with ID: {CustomerId}", id);

                var customer = await _unitOfWork.Customers.GetByIdAsync(id);
                if (customer == null || customer.IsDeleted)
                {
                    _logger.LogWarning("Customer with ID {CustomerId} not found or is deleted", id);
                    throw ErrorHelper.NotFound($"Customer with ID {id} not found");
                }

                customer.FirstName = customerDto.FirstName;
                customer.LastName = customerDto.LastName;
                customer.Email = customerDto.Email;
                customer.Phone = customerDto.Phone;
                customer.Address = customerDto.Address;

                await _unitOfWork.Customers.Update(customer);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully updated customer with ID: {CustomerId}", id);
                return true;
            }
            catch (Exception ex) when (!(ex.Data.Contains("StatusCode") && (int)ex.Data["StatusCode"] == 404))
            {
                _logger.LogError(ex, "Error occurred while updating customer with ID: {CustomerId}. Message: {Message}",
                    id, ex.Message);
                throw;
            }
        }

        public async Task<bool> DeleteCustomerAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting customer with ID: {CustomerId}", id);

                var customer = await _unitOfWork.Customers.GetByIdAsync(id);
                if (customer == null || customer.IsDeleted)
                {
                    _logger.LogWarning("Customer with ID {CustomerId} not found or is deleted", id);
                    throw ErrorHelper.NotFound($"Customer with ID {id} not found");
                }

                await _unitOfWork.Customers.SoftRemove(customer);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted customer with ID: {CustomerId}", id);
                return true;
            }
            catch (Exception ex) when (!(ex.Data.Contains("StatusCode") && (int)ex.Data["StatusCode"] == 404))
            {
                _logger.LogError(ex, "Error occurred while deleting customer with ID: {CustomerId}. Message: {Message}",
                    id, ex.Message);
                throw;
            }
        }

        #endregion
    }
}
