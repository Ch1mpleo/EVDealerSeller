using EVDealerSales.Models.Commons;
using EVDealerSales.Models.DTOs.CustomerDTOs;
using EVDealerSales.Models.DTOs.UserDTOs;

namespace EVDealerSales.Services.Interfaces
{
    public interface IManagerService
    {
        // Employee (User) Management
        Task<GetEmployeeDto> AddEmployeeAsync(CreateEmployeeDto createEmployeeDto);
        Task<Pagination<GetEmployeeDto>> GetEmployeesAsync(int pageNumber, int pageSize, string searchTerm = null);
        Task<GetEmployeeDto> GetEmployeeByIdAsync(Guid id);
        Task<bool> UpdateEmployeeAsync(Guid id, GetEmployeeDto employeeDto);
        Task<bool> EmployeeIsActiveAsync(Guid id);

        // Customer Management
        Task<Pagination<GetCustomerDto>> GetCustomersAsync(int pageNumber, int pageSize, string searchTerm = null);
        Task<GetCustomerDto> GetCustomerByIdAsync(Guid id);
        Task<bool> UpdateCustomerAsync(Guid id, GetCustomerDto customerDto);
        Task<bool> DeleteCustomerAsync(Guid id);
    }
}
