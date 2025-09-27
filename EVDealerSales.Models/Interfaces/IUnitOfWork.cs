using EVDealerSales.Models.Entities;

namespace EVDealerSales.Models.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> Users { get; }
        IGenericRepository<Vehicle> Vehicles { get; }
        Task<int> SaveChangesAsync();
    }
}
