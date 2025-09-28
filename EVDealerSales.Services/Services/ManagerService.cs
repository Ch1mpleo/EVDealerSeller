using EVDealerSales.Models.Commons;
using EVDealerSales.Models.DTOs.CustomerDTOs;
using EVDealerSales.Models.DTOs.UserDTOs;
using EVDealerSales.Models.Entities;
using EVDealerSales.Models.Interfaces;
using EVDealerSales.Services.Interfaces;
using EVDealerSales.Services.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVDealerSales.Services.Services
{
    public class ManagerService : IManagerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public ManagerService(IUnitOfWork unitOfWork, ILogger<ManagerService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        #region Employee Management

        public async Task<GetEmployeeDto> AddEmployeeAsync(CreateEmployeeDto createEmployeeDto)
        {
            try
            {
                _logger.LogInformation("Adding new employee with email: {Email}", createEmployeeDto.Email);

                // Check if employee with this email already exists
                var existingEmployee = await _unitOfWork.Users.FirstOrDefaultAsync(
                    u => u.Email.ToLower() == createEmployeeDto.Email.ToLower() && !u.IsDeleted);

                if (existingEmployee != null)
                {
                    _logger.LogWarning("Employee with email {Email} already exists", createEmployeeDto.Email);
                    throw ErrorHelper.Conflict($"Employee with email {createEmployeeDto.Email} already exists");
                }

                // Hash the password
                var passwordHasher = new PasswordHasher();
                var hashedPassword = passwordHasher.HashPassword(createEmployeeDto.Password);
                if (string.IsNullOrEmpty(hashedPassword))
                {
                    _logger.LogError("Failed to hash password for new employee");
                    throw ErrorHelper.Internal("Failed to process password");
                }

                // Create the new employee
                var employee = new User
                {
                    FullName = createEmployeeDto.FullName,
                    Email = createEmployeeDto.Email,
                    Phone = createEmployeeDto.Phone,
                    PasswordHash = hashedPassword,
                    Role = createEmployeeDto.Role,
                    IsActive = createEmployeeDto.IsActive
                };

                await _unitOfWork.Users.AddAsync(employee);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully added new employee with ID: {EmployeeId}", employee.Id);

                return new GetEmployeeDto
                {
                    Id = employee.Id,
                    FullName = employee.FullName,
                    Email = employee.Email,
                    Phone = employee.Phone,
                    Role = employee.Role,
                    IsActive = employee.IsActive
                };
            }
            catch (Exception ex) when (!(ex.Data.Contains("StatusCode") &&
                                       ((int)ex.Data["StatusCode"] == 404 || (int)ex.Data["StatusCode"] == 409)))
            {
                _logger.LogError(ex, "Error occurred while adding employee with email: {Email}. Message: {Message}",
                    createEmployeeDto.Email, ex.Message);
                throw;
            }
        }

        public async Task<Pagination<GetEmployeeDto>> GetEmployeesAsync(int pageNumber, int pageSize, string searchTerm = null)
        {
            try
            {
                _logger.LogInformation("Fetching employees with pagination. Page: {PageNumber}, Size: {PageSize}, SearchTerm: {SearchTerm}",
                    pageNumber, pageSize, searchTerm ?? "none");

                var query = _unitOfWork.Users.GetQueryable().Where(u => !u.IsDeleted);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(u =>
                        u.FullName.ToLower().Contains(searchTerm) ||
                        u.Email.ToLower().Contains(searchTerm) ||
                        u.Phone.Contains(searchTerm));
                }

                var totalCount = await query.CountAsync();
                var employees = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new GetEmployeeDto
                    {
                        Id = u.Id,
                        FullName = u.FullName,
                        Email = u.Email,
                        Phone = u.Phone,
                        Role = u.Role,
                        IsActive = u.IsActive
                    })
                    .ToListAsync();

                _logger.LogInformation("Successfully retrieved {Count} employees out of {Total} total",
                    employees.Count, totalCount);

                return new Pagination<GetEmployeeDto>(employees, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching employees. Message: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<GetEmployeeDto> GetEmployeeByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Fetching employee with ID: {EmployeeId}", id);

                var employee = await _unitOfWork.Users.GetByIdAsync(id);
                if (employee == null || employee.IsDeleted)
                {
                    _logger.LogWarning("Employee with ID {EmployeeId} not found or is deleted", id);
                    throw ErrorHelper.NotFound($"Employee with ID {id} not found");
                }

                _logger.LogInformation("Successfully retrieved employee with ID: {EmployeeId}", id);

                return new GetEmployeeDto
                {
                    Id = employee.Id,
                    FullName = employee.FullName,
                    Email = employee.Email,
                    Phone = employee.Phone,
                    Role = employee.Role,
                    IsActive = employee.IsActive
                };
            }
            catch (Exception ex) when (!(ex.Data.Contains("StatusCode") && (int)ex.Data["StatusCode"] == 404))
            {
                _logger.LogError(ex, "Error occurred while fetching employee with ID: {EmployeeId}. Message: {Message}",
                    id, ex.Message);
                throw;
            }
        }

        public async Task<bool> UpdateEmployeeAsync(Guid id, GetEmployeeDto employeeDto)
        {
            try
            {
                _logger.LogInformation("Updating employee with ID: {EmployeeId}", id);

                var employee = await _unitOfWork.Users.GetByIdAsync(id);
                if (employee == null || employee.IsDeleted)
                {
                    _logger.LogWarning("Employee with ID {EmployeeId} not found or is deleted", id);
                    throw ErrorHelper.NotFound($"Employee with ID {id} not found");
                }

                employee.FullName = employeeDto.FullName;
                employee.Email = employeeDto.Email;
                employee.Phone = employeeDto.Phone;
                employee.Role = employeeDto.Role;
                employee.IsActive = employeeDto.IsActive;

                await _unitOfWork.Users.Update(employee);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully updated employee with ID: {EmployeeId}", id);
                return true;
            }
            catch (Exception ex) when (!(ex.Data.Contains("StatusCode") && (int)ex.Data["StatusCode"] == 404))
            {
                _logger.LogError(ex, "Error occurred while updating employee with ID: {EmployeeId}. Message: {Message}",
                    id, ex.Message);
                throw;
            }
        }

        public async Task<bool> EmployeeIsActiveAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deactivating employee with ID: {EmployeeId}", id);

                var employee = await _unitOfWork.Users.GetByIdAsync(id);
                if (employee == null || employee.IsDeleted)
                {
                    _logger.LogWarning("Employee with ID {EmployeeId} not found or is deleted", id);
                    throw ErrorHelper.NotFound($"Employee with ID {id} not found");
                }

                employee.IsActive = !employee.IsActive;
                await _unitOfWork.Users.Update(employee);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully changed active status to {ActiveStatus} for employee with ID: {EmployeeId}", employee.IsActive, id);
                return true;
            }
            catch (Exception ex) when (!(ex.Data.Contains("StatusCode") && (int)ex.Data["StatusCode"] == 404))
            {
                _logger.LogError(ex, "Error occurred while deactivating employee with ID: {EmployeeId}. Message: {Message}",
                    id, ex.Message);
                throw;
            }
        }

        #endregion

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
