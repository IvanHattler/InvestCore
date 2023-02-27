namespace InvestCore.PercentCalculateConsole.Domain
{
    public class ReplenishmentModel
    {
        public decimal ReplenishmentAmount { get; set; }
        public decimal CurrentSum { get; set; }
        public decimal SavePercent { get; set; } = 0.01m;
        public decimal SumForBuy => (ReplenishmentAmount + CurrentSum) * (1 - SavePercent);
    }
}
