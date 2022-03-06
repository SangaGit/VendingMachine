namespace VendingMachine
{
    public interface IMachineManager
    {
        public decimal DepositCoin(decimal amount);
        public bool IsAcceptableCoin(decimal amount);
        public void Run();
    }
}