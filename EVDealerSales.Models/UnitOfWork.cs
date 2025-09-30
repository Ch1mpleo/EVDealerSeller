using EVDealerSales.Models.Entities;
using EVDealerSales.Models.Interfaces;

namespace EVDealerSales.Models
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EVDealerSalesDbContext _dbContext;

        public UnitOfWork(EVDealerSalesDbContext dbContext,
            IGenericRepository<User> userRepository,
            IGenericRepository<Customer> customerRepository,
            IGenericRepository<Vehicle> vehicleRepository,
            IGenericRepository<Quote> quoteRepository,
            IGenericRepository<Order> orderRepository,
            IGenericRepository<OrderItem> orderItemRepository,
            IGenericRepository<Invoice> invoiceRepository,
            IGenericRepository<Payment> paymentRepository,
            IGenericRepository<Delivery> deliveryRepository
            )
        {
            _dbContext = dbContext;
            Users = userRepository;
            Customers = customerRepository;
            Vehicles = vehicleRepository;
            Quotes = quoteRepository;
            Orders = orderRepository;
            OrderItems = orderItemRepository;
            Invoices = invoiceRepository;
            Payments = paymentRepository;
            Deliveries = deliveryRepository;
        }

        public IGenericRepository<User> Users { get; }
        public IGenericRepository<Customer> Customers { get; }
        public IGenericRepository<Vehicle> Vehicles { get; }
        public IGenericRepository<Quote> Quotes { get; }
        public IGenericRepository<Order> Orders { get; }
        public IGenericRepository<OrderItem> OrderItems { get; }
        public IGenericRepository<Invoice> Invoices { get; }
        public IGenericRepository<Payment> Payments { get; }
        public IGenericRepository<Delivery> Deliveries { get; }
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
