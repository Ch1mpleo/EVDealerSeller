using EVDealerSales.BO.DTOs.CustomerDTOs;
using EVDealerSales.Models.Entities;
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
        private readonly IClaimsService _claimsService;

        public StaffService(IUnitOfWork unitOfWork, ILogger<StaffService> logger, IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _claimsService = claimsService;
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

                // Audit
                customer.UpdatedAt = DateTime.UtcNow;
                customer.UpdatedBy = _claimsService.GetCurrentUserId;

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

                // Soft delete with audit
                customer.IsDeleted = true;
                customer.DeletedAt = DateTime.UtcNow;
                customer.DeletedBy = _claimsService.GetCurrentUserId;

                await _unitOfWork.Customers.Update(customer);
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

        public async Task<List<ListNameCustomerDto>> CustomerListNameAsync()
        {
            try
            {
                _logger.LogInformation("Fetching list of customer names");
                var customers = await _unitOfWork.Customers
                    .GetQueryable()
                    .Where(c => !c.IsDeleted)
                    .Select(c => new ListNameCustomerDto
                    {
                        Id = c.Id,
                        Name = $"{c.FirstName} {c.LastName}"
                    })
                    .ToListAsync();
                _logger.LogInformation("Successfully retrieved {Count} customer names", customers.Count);
                return customers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching customer names. Message: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<GetCustomerDto> AddCustomerAsync(CreateCustomerDto customerDto)
        {
            try
            {
                //Validation
                if (string.IsNullOrWhiteSpace(customerDto.Email))
                {
                    throw new ArgumentException("Email is required");
                }
                if (string.IsNullOrWhiteSpace(customerDto.FirstName) || string.IsNullOrWhiteSpace(customerDto.LastName))
                {
                    throw new ArgumentException("First name and last name are required");
                }

                _logger.LogInformation("Adding new customer with email: {Email}", customerDto.Email);

                //Check if customer with this email already exists
                var existingCustomer = await _unitOfWork.Customers
                    .GetQueryable()
                    .FirstOrDefaultAsync(c => c.Email.ToLower() == customerDto.Email.ToLower() && !c.IsDeleted);

                if (existingCustomer != null)
                {
                    _logger.LogWarning("Customer with email {Email} already exists", customerDto.Email);
                    throw ErrorHelper.Conflict($"Customer with email {customerDto.Email} already exists");
                }

                //Create the new customer entity with audit fields
                var currentUserId = _claimsService.GetCurrentUserId;
                var now = DateTime.UtcNow;

                var customer = new Customer
                {
                    FirstName = customerDto.FirstName,
                    LastName = customerDto.LastName,
                    Email = customerDto.Email,
                    Phone = customerDto.Phone,
                    Address = customerDto.Address,
                    //Aduit fields
                    CreatedAt = now,
                    CreatedBy = currentUserId,
                };

                var createdCustomer = await _unitOfWork.Customers.AddAsync(customer);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully added new customer with ID: {CustomerId}", createdCustomer.Id);

                return new GetCustomerDto
                {
                    Id = createdCustomer.Id,
                    FirstName = createdCustomer.FirstName,
                    LastName = createdCustomer.LastName,
                    Email = createdCustomer.Email,
                    Phone = createdCustomer.Phone,
                    Address = createdCustomer.Address
                };
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create customer with email: {Email}. Exception: {Message}",
                     customerDto.Email, ex.Message);
                throw new Exception("An error occurred while creating the customer. Please try again later.");
            }
        }

    }

    #endregion
}

