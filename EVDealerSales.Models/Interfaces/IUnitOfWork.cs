namespace EVDealerSales.Models.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {

        Task<int> SaveChangesAsync();
    }
}
