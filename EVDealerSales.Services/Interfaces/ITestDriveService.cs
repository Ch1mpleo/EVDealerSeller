using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EVDealerSales.Models.Commons;
using EVDealerSales.Models.DTOs.TestDriveDTOs;
using EVDealerSales.Models.Enums;

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
