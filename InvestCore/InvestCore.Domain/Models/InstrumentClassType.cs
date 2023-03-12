namespace InvestCore.Domain.Models
{
    public enum InstrumentClassType
    {
        Share,
        GosBond,
        CorpBond,
        Gold,
    }

    public static class InstrumentClassTypeExtensions
    {
        public static string GetDisplayText(this InstrumentClassType classType)
        {
            return classType switch
            {
                InstrumentClassType.Share => "Акции",
                InstrumentClassType.GosBond => "Гос. облигации",
                InstrumentClassType.CorpBond => "Корп. облигации",
                InstrumentClassType.Gold => "Золото",
                _ => throw new NotImplementedException(),
            };
        }
    }
}
