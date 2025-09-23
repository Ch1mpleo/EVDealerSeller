namespace EVDealerSales.Models.Entities
{
    public class Contract : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; }
        public string ContractNumber { get; set; }
        public DateTime ContractDate { get; set; }
        public string Terms { get; set; }
    }
}
