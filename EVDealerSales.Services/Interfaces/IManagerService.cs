using EVDealerSales.BO.DTOs.UserDTOs;
using EVDealerSales.Services.Utils;

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
        Task<List<ListNameEmployeeDto>> EmployeeListNameAsync();
    }
}
