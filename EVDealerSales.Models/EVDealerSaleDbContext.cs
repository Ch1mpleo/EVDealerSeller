using EVDealerSales.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EVDealerSales.Models
{
    public class EVDealerDbContext : DbContext
    {
        public EVDealerDbContext()
        { }

        public EVDealerDbContext(DbContextOptions<EVDealerDbContext> options)
            : base(options)
        {
        }

        // -------------------- DbSets --------------------
        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<TestDrive> TestDrives { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -------------------- RELATIONSHIPS --------------------

            // TestDrive ↔ Customer (many-to-one)
            modelBuilder.Entity<TestDrive>()
                .HasOne(td => td.Customer)
                .WithMany(c => c.TestDrives)
                .HasForeignKey(td => td.CustomerId);

            // TestDrive ↔ Vehicle (many-to-one)
            modelBuilder.Entity<TestDrive>()
                .HasOne(td => td.Vehicle)
                .WithMany(v => v.TestDrives)
                .HasForeignKey(td => td.VehicleId);

            // TestDrive ↔ User (staff, optional)
            modelBuilder.Entity<TestDrive>()
                .HasOne(td => td.Staff)
                .WithMany(u => u.TestDrives)
                .HasForeignKey(td => td.StaffId)
                .OnDelete(DeleteBehavior.SetNull);

            // Quote ↔ Customer (many-to-one)
            modelBuilder.Entity<Quote>()
                .HasOne(q => q.Customer)
                .WithMany(c => c.Quotes)
                .HasForeignKey(q => q.CustomerId);

            // Quote ↔ User (staff, many-to-one)
            modelBuilder.Entity<Quote>()
                .HasOne(q => q.Staff)
                .WithMany(u => u.Quotes)
                .HasForeignKey(q => q.StaffId);

            // Quote ↔ Vehicle (many-to-one)
            modelBuilder.Entity<Quote>()
                .HasOne(q => q.Vehicle)
                .WithMany(v => v.Quotes)
                .HasForeignKey(q => q.VehicleId);

            // Order ↔ Customer (many-to-one)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId);

            // Order ↔ User (staff, many-to-one)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Staff)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.StaffId);

            // Order ↔ Quote (optional, one-to-one-ish)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Quote)
                .WithMany()
                .HasForeignKey(o => o.QuoteId)
                .OnDelete(DeleteBehavior.SetNull);

            // OrderItem ↔ Order (many-to-one)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId);

            // OrderItem ↔ Vehicle (many-to-one)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Vehicle)
                .WithMany(v => v.OrderItems)
                .HasForeignKey(oi => oi.VehicleId);

            // Contract ↔ Order (one-to-one)
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Order)
                .WithOne(o => o.Contract)
                .HasForeignKey<Contract>(c => c.OrderId);

            // Invoice ↔ Order (many-to-one)
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Order)
                .WithMany(o => o.Invoices)
                .HasForeignKey(i => i.OrderId);

            // Invoice ↔ Customer (many-to-one)
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Customer)
                .WithMany(c => c.Invoices)
                .HasForeignKey(i => i.CustomerId);

            // Payment ↔ Invoice (many-to-one)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Invoice)
                .WithMany(i => i.Payments)
                .HasForeignKey(p => p.InvoiceId);

            // Delivery ↔ Order (one-to-one)
            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.Order)
                .WithOne(o => o.Delivery)
                .HasForeignKey<Delivery>(d => d.OrderId);

            // Feedback ↔ Customer (many-to-one)
            modelBuilder.Entity<Feedback>()
                .HasOne(fb => fb.Customer)
                .WithMany(c => c.Feedbacks)
                .HasForeignKey(fb => fb.CustomerId);

            // Feedback ↔ Order (optional, many-to-one)
            modelBuilder.Entity<Feedback>()
                .HasOne(fb => fb.Order)
                .WithMany(o => o.Feedbacks)
                .HasForeignKey(fb => fb.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            // Feedback ↔ User (creator, optional)
            modelBuilder.Entity<Feedback>()
                .HasOne(fb => fb.Creator)
                .WithMany(u => u.CreatedFeedbacks)
                .HasForeignKey(fb => fb.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // Feedback ↔ User (resolver, optional)
            modelBuilder.Entity<Feedback>()
                .HasOne(fb => fb.Resolver)
                .WithMany(u => u.ResolvedFeedbacks)
                .HasForeignKey(fb => fb.ResolvedBy)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
