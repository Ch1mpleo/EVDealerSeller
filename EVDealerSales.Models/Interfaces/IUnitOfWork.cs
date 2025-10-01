using EVDealerSales.Models.Entities;

namespace EVDealerSales.Models.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> Users { get; }
        IGenericRepository<Customer> Customers { get; }
        IGenericRepository<Vehicle> Vehicles { get; }
        IGenericRepository<Quote> Quotes { get; }
        IGenericRepository<Order> Orders { get; }
        IGenericRepository<OrderItem> OrderItems { get; }
        IGenericRepository<Invoice> Invoices { get; }
        IGenericRepository<Payment> Payments { get; }
        IGenericRepository<Delivery> Deliveries { get; }
        IGenericRepository<TestDrive> TestDrives { get; }
        
        Task<int> SaveChangesAsync();
    }
}
