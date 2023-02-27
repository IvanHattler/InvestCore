namespace InvestCore.PercentCalculateConsole.Domain
{
    public class BuyModel
    {
        public int ShareCounts;
        public int GosBondCounts;
        public int CorpBondCounts;
        public decimal SumDifference;
        public decimal SharePercentDeviation;
        public decimal GosBondPercentDeviation;
        public decimal CorpBondPercentDeviation;

        public string GetBuyMessage()
        {
            var message = $"{ShareCounts} акций, " +
                $"{GosBondCounts} гос. облигаций, " +
                $"{CorpBondCounts} корп. облигаций." + Environment.NewLine + Environment.NewLine +
                $"Отстаток средств: {SumDifference:F}." + Environment.NewLine +
                $"Отклонение по акциям: {SharePercentDeviation:P8}, " + Environment.NewLine +
                $"Отклонение по гос. облигациям: {GosBondPercentDeviation:P8}, " + Environment.NewLine +
                $"Отклонение по корп. облигациям: {CorpBondPercentDeviation:P8}";
            return message;
        }
    }
}
