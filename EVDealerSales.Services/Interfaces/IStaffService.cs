using EVDealerSales.Models.Commons;
using EVDealerSales.Models.DTOs.CustomerDTOs;

namespace EVDealerSales.Services.Interfaces
{
    public interface IStaffService
    {
        // Customer Management
        Task<Pagination<GetCustomerDto>> GetCustomersAsync(int pageNumber, int pageSize, string searchTerm = null);
        Task<GetCustomerDto> GetCustomerByIdAsync(Guid id);
        Task<bool> UpdateCustomerAsync(Guid id, GetCustomerDto customerDto);
        Task<bool> DeleteCustomerAsync(Guid id);
        Task<List<ListNameCustomerDto>> CustomerListNameAsync();
    }
}
