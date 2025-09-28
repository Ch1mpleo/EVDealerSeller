using EVDealerSales.Models.Entities;
using EVDealerSales.Models.Interfaces;

namespace EVDealerSales.Models
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EVDealerSalesDbContext _dbContext;

        public UnitOfWork(EVDealerSalesDbContext dbContext,
            IGenericRepository<User> userRepository,
            IGenericRepository<Customer> customerRepository
            )
        {
            _dbContext = dbContext;
            Users = userRepository;
            Customers = customerRepository;
        }

        public IGenericRepository<User> Users { get; }
        public IGenericRepository<Customer> Customers { get; }
        public void Dispose()
        {
            _dbContext.Dispose();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}
