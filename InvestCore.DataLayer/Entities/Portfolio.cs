namespace InvestCore.DataLayer.Entities
{
    public class Portfolio : BaseEntity
    {
        public int UserId { get; set; }
        public string? Name { get; set; }
    }
}
