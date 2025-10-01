using EVDealerSales.BO.DTOs.TestDriveDTOs;
using EVDealerSales.BO.Enums;
using EVDealerSales.Services.Utils;

namespace EVDealerSales.Services.Interfaces
{
    public interface ITestDriveService
    {
        Task<List<TestDriveDto>> CreateTestDriveAsync(CreateTestDriveDto createTestDriveDto);
        Task<Pagination<TestDriveDto>> GetTestDrivesByCustomerAsync(Guid customerId, int page, int pageSize);
        Task<Pagination<TestDriveDto>> GetAllTestDrivesAsync(string? search, string? sortBy, bool isDescending, int page, int pageSize);
        Task<bool> UpdateTestDriveAsync(UpdateTestDriveDto updateTestDriveDto);
        Task<bool> UpdateTestDriveStatusAsync(Guid testDriveId, TestDriveStatus newStatus);
    }
}
