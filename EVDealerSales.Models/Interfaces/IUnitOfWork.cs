using EVDealerSales.Models.Entities;

namespace EVDealerSales.Models.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> Users { get; }
        IGenericRepository<Customer> Customers { get; }
        Task<int> SaveChangesAsync();
    }
}
