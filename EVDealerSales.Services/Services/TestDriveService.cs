using EVDealerSales.BO.DTOs.TestDriveDTOs;
using EVDealerSales.BO.Enums;
using EVDealerSales.Models.Entities;
using EVDealerSales.Models.Interfaces;
using EVDealerSales.Services.Interfaces;
using EVDealerSales.Services.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVDealerSales.Services.Services
{
    public class TestDriveService : ITestDriveService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly IClaimsService _claimsService;

        public TestDriveService(IUnitOfWork unitOfWork, ILogger<TestDriveService> logger, IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _claimsService = claimsService;
        }

        public async Task<List<TestDriveDto>> CreateTestDriveAsync(CreateTestDriveDto createTestDriveDto)
        {
            try
            {
                if (createTestDriveDto.CustomerId == Guid.Empty)
                    throw new ArgumentException("CustomerId is required");
                if (createTestDriveDto.VehicleId == Guid.Empty)
                    throw new ArgumentException("VehicleId is required");
                if (createTestDriveDto.ScheduledDates == null || !createTestDriveDto.ScheduledDates.Any())
                    throw new ArgumentException("At least one scheduled date is required");

                var now = DateTime.UtcNow;
                var testDrives = new List<TestDrive>();

                foreach (var scheduledAt in createTestDriveDto.ScheduledDates)
                {
                    if (scheduledAt < now)
                        throw new ArgumentException("Scheduled time must be in the future");

                    var entity = new TestDrive
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = createTestDriveDto.CustomerId,
                        VehicleId = createTestDriveDto.VehicleId,
                        ScheduledAt = scheduledAt,
                        Status = TestDriveStatus.Scheduled,
                        Notes = createTestDriveDto.Notes ?? string.Empty,
                        StaffId = _claimsService.GetCurrentUserId
                    };

                    testDrives.Add(entity);
                }

                await _unitOfWork.TestDrives.AddRangeAsync(testDrives);
                await _unitOfWork.SaveChangesAsync();

                return testDrives.Select(entity => new TestDriveDto
                {
                    Id = entity.Id,
                    CustomerId = entity.CustomerId,
                    VehicleId = entity.VehicleId,
                    ScheduledAt = entity.ScheduledAt,
                    Status = entity.Status,
                    Notes = entity.Notes,
                    StaffId = entity.StaffId
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test drives for customer {CustomerId}", createTestDriveDto.CustomerId);
                throw;
            }
        }


        public async Task<Pagination<TestDriveDto>> GetAllTestDrivesAsync(string? search, string? sortBy, bool isDescending, int page, int pageSize)
        {
            try
            {
                _logger.LogInformation("Fetching all test drives. Page: {Page}, Size: {PageSize}, Search: {Search}", page, pageSize, search ?? "none");

                var query = _unitOfWork.TestDrives.GetQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var lower = search.ToLower();
                    query = query.Where(td =>
                        td.Notes.ToLower().Contains(lower) ||
                        td.Customer.FirstName.ToLower().Contains(lower) ||
                        td.Customer.LastName.ToLower().Contains(lower) ||
                        td.Vehicle.ModelName.ToLower().Contains(lower)
                    );
                }

                // Sorting
                query = sortBy switch
                {
                    "ScheduledAt" => isDescending ? query.OrderByDescending(x => x.ScheduledAt) : query.OrderBy(x => x.ScheduledAt),
                    "Status" => isDescending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                    _ => query.OrderByDescending(x => x.ScheduledAt)
                };

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(td => new TestDriveDto
                    {
                        Id = td.Id,
                        CustomerId = td.CustomerId,
                        VehicleId = td.VehicleId,
                        ScheduledAt = td.ScheduledAt,
                        Status = td.Status,
                        Notes = td.Notes,
                        StaffId = td.StaffId
                    })
                    .ToListAsync();

                _logger.LogInformation("Fetched {Count} test drives out of {Total}", items.Count, totalCount);

                return new Pagination<TestDriveDto>(items, totalCount, page, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all test drives");
                throw;
            }
        }

        public async Task<Pagination<TestDriveDto>> GetTestDrivesByCustomerAsync(Guid customerId, int page, int pageSize)
        {
            try
            {
                _logger.LogInformation("Fetching test drives for customer {CustomerId}. Page: {Page}, Size: {PageSize}", customerId, page, pageSize);

                var query = _unitOfWork.TestDrives.GetQueryable().Where(td => td.CustomerId == customerId);

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(td => td.ScheduledAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(td => new TestDriveDto
                    {
                        Id = td.Id,
                        CustomerId = td.CustomerId,
                        VehicleId = td.VehicleId,
                        ScheduledAt = td.ScheduledAt,
                        Status = td.Status,
                        Notes = td.Notes,
                        StaffId = td.StaffId
                    })
                    .ToListAsync();

                _logger.LogInformation("Fetched {Count} test drives for customer {CustomerId}", items.Count, customerId);

                return new Pagination<TestDriveDto>(items, totalCount, page, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching test drives for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> UpdateTestDriveAsync(UpdateTestDriveDto updateTestDriveDto)
        {
            try
            {
                if (updateTestDriveDto.Id == Guid.Empty)
                    throw new ArgumentException("TestDrive Id is required");

                _logger.LogInformation("Updating test drive {TestDriveId}", updateTestDriveDto.Id);

                var entity = await _unitOfWork.TestDrives.GetByIdAsync(updateTestDriveDto.Id);
                if (entity == null)
                {
                    _logger.LogWarning("Test drive with ID {TestDriveId} not found", updateTestDriveDto.Id);
                    return false;
                }

                entity.ScheduledAt = updateTestDriveDto.ScheduledAt;
                entity.Status = updateTestDriveDto.Status;
                entity.Notes = updateTestDriveDto.Notes ?? string.Empty;
                entity.StaffId = updateTestDriveDto.StaffId;

                await _unitOfWork.TestDrives.Update(entity);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Test drive {TestDriveId} updated successfully", updateTestDriveDto.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating test drive {TestDriveId}", updateTestDriveDto.Id);
                throw;
            }
        }

        public async Task<bool> UpdateTestDriveStatusAsync(Guid testDriveId, TestDriveStatus newStatus)
        {
            try
            {
                if (testDriveId == Guid.Empty)
                    throw new ArgumentException("TestDrive Id is required");

                _logger.LogInformation("Updating status for test drive {TestDriveId} to {Status}", testDriveId, newStatus);

                var entity = await _unitOfWork.TestDrives.GetByIdAsync(testDriveId);
                if (entity == null)
                {
                    _logger.LogWarning("Test drive with ID {TestDriveId} not found", testDriveId);
                    return false;
                }

                entity.Status = newStatus;
                await _unitOfWork.TestDrives.Update(entity);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Test drive {TestDriveId} status updated to {Status}", testDriveId, newStatus);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for test drive {TestDriveId}", testDriveId);
                throw;
            }
        }
    }
}
