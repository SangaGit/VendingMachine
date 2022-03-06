namespace VendingMachine
{
    public interface IMachineManager
    {
        public double DepositCoin(double amount);
        public bool IsAcceptableCoin(double amount);
        public void Run();
    }
}