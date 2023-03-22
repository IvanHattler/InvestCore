namespace InvestCore.TinkoffApi.Infrastructure
{
    internal class InstrumentNotFoundException : Exception
    {
        public InstrumentNotFoundException(string message)
            : base(message)
        {
        }
    }
}
