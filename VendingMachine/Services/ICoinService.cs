namespace VendingMachine.Services
{
    public interface ICoinService
    {
        public IEnumerable<decimal> ValidCoins {get;}
        void CheckCoinAvailability();
        public void CalculateNumberOfCoinsForChange(decimal changeAmount);
        public void CalculateAvailableCoins();
    }
}