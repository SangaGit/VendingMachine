using System.Collections.Generic;
using Xunit;

namespace VendingMachine.Test;

public class MachineTests
{
    private readonly Machine machine;
    public MachineTests()
    {
        machine = new Machine();
    }
    
    [Fact]
    public void HasDepositedAmountProperty(){
        machine.DepositedAmount = 1.00;
        Assert.Equal(1.00,machine.DepositedAmount);
    }

    [Theory]
    [InlineData(3,false)]
    [InlineData(1,true)]
    [InlineData(0.5,true)]
    public void InsertedCoinShouldLessThanOrEqualTwo(double coin, bool expected)
    {
        var result = machine.IsAcceptableCoin(coin);
        Assert.Equal(result,expected);
    }

    [Theory]
    [InlineData(0.05,true)]
    [InlineData(0.02,false)]
    [InlineData(0.50,true)]
    public void InsertedCoinShouldMoreThanOrEqualFiveCents(double coin, bool expected)
    {
        var result = machine.IsAcceptableCoin(coin);
        Assert.Equal(result,expected);
    }

    [Theory]
    [InlineData(0.05,true)]
    [InlineData(0.30,false)]
    [InlineData(0.50,true)]
    public void InsertedCoinShouldBeRealValidCoin(double coin, bool expected)
    {
        var result = machine.IsAcceptableCoin(coin);
        Assert.Equal(expected,result);
    }

    [Fact]
    public void ValidCoinShouldBeAddedToTheDepositedAmount()
    {
        machine.DepositCoin(0.05);
        Assert.Equal(0.05,machine.DepositedAmount);
        machine.DepositCoin(0.01);//invalid coin
        Assert.Equal(0.05,machine.DepositedAmount);
        machine.DepositCoin(0.50);
        Assert.Equal(0.55,machine.DepositedAmount);
    }

    [Fact]
    public void InValidCoinShouldNotBeAddedToTheDepositedAmount()
    {
        machine.DepositCoin(0.05);
        Assert.Equal(0.05,machine.DepositedAmount);
        machine.DepositCoin(0.01);//invalid coin
        Assert.Equal(0.05,machine.DepositedAmount);
    }
}

public class Machine
{
    private List<double> _validCoins = new List<double>(){0.05, 0.10, 0.20, 0.50, 1.00, 2.00};
    public double DepositedAmount = 0.00;

    public Machine GetMachine()
    {
        return this;
    }
    public bool IsAcceptableCoin(double coin){
        return _validCoins.Contains(coin);
    }
    public double DepositCoin(double amount){
        if(IsAcceptableCoin(amount)){
            return DepositedAmount+=amount;
        }
        return DepositedAmount;
    }
}