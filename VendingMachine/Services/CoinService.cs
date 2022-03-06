using Microsoft.Extensions.Configuration;

namespace VendingMachine.Services
{
    public class CoinService : ICoinService
    {
        private readonly ITranslateService _translateService;
        private readonly IConfiguration _config;
        private Dictionary<decimal, int> availableCoins = new Dictionary<decimal, int>();
        private List<decimal> posibleChanges;
        private bool isExactChangeOnly;
        public IEnumerable<decimal> ValidCoins {get;}
        public CoinService(ITranslateService translateService, IConfiguration config)
        {
            _translateService = translateService;
            _config = config;
            availableCoins = _config.GetSection("AvailableCoins").GetChildren().AsEnumerable().ToDictionary(x => decimal.Parse(x.Key), x => int.Parse(x.Value));
            ValidCoins = _config.GetValue<string>("ValidCoins").Split(",").Select(x => decimal.Parse(x)).ToList().AsReadOnly();
            posibleChanges = calculatePossibleChanges();
        }

        public void CheckCoinAvailability()
        {
            // foreach (var coin in availableCoins)
            // {
            //     Console.WriteLine($"coin - {coin.Key} ==> {coin.Value}");
            // }
            foreach (var change in posibleChanges)
            {
                CalculateNumberOfCoinsForChange(change);
                if (isExactChangeOnly)
                {
                    //check available coins in appsettings.json file. change the counts to 0 in there and check accordingly
                    Console.Write($"{_translateService.Translate("EXACT CHANGE ONLY")}");
                    Console.ReadKey();
                    break;
                }
            }
        }
        private List<decimal> calculatePossibleChanges()
        {

            var possibleChanges = new List<decimal>();
            //possible changes could be 0.05, 0.10, 0.15, 0.20, .... 1.55, 2.00
            // foreach (var prod in products)
            // {
            //     if (prod.UnitPrice >= 1 && prod.UnitPrice <= 2)
            //     {
            //         possibleChanges.Add(2 - (decimal)prod.UnitPrice);
            //     }
            //     else if (prod.UnitPrice > 0 && prod.UnitPrice < 1)
            //     {
            //         possibleChanges.Add(1 - (decimal)prod.UnitPrice);
            //     }
            // }
            for (var i = 0.05m; i <= 2.00m; i += 0.05m)
            {
                possibleChanges.Add(i);
            }
            return possibleChanges;
        }
        int twoEuro, oneEuro, half, quarters, dimes, nickels;
        public void CalculateNumberOfCoinsForChange(decimal changeAmount)
        {
            //check available coins in appsettings.json file. change the counts to 0 in there and check accordingly
            int tempTwo, tempOne, tempHalf, tempQuater, tempDim, tempNickle;


            twoEuro = (int)Math.Abs((changeAmount / 2));
            tempTwo = CheckAvailableCoinCount(2, twoEuro);
            twoEuro -= tempTwo;
            changeAmount %= 2;
            changeAmount += (2 * tempTwo);


            changeAmount = Math.Round(changeAmount, 2);
            oneEuro = (int)Math.Abs((changeAmount / 1));
            tempOne = CheckAvailableCoinCount(1, oneEuro);
            oneEuro -= tempOne;
            changeAmount %= 1;
            changeAmount += (1 * tempOne);


            changeAmount = Math.Round(changeAmount, 2);
            half = (int)Math.Abs((changeAmount / .50m));
            tempHalf = CheckAvailableCoinCount(0.50m, half);
            half -= tempHalf;
            changeAmount %= .50m;
            changeAmount += (0.50m * tempHalf);


            changeAmount = Math.Round(changeAmount, 2);
            quarters = (int)Math.Abs((changeAmount / .20m));
            tempQuater = CheckAvailableCoinCount(0.20m, quarters);
            quarters -= tempQuater;
            changeAmount %= .20m;
            changeAmount += (0.20m * tempQuater);


            changeAmount = Math.Round(changeAmount, 2);
            dimes = (int)Math.Abs((changeAmount / .10m));
            tempDim = CheckAvailableCoinCount(0.10m, dimes);
            dimes -= tempDim;
            changeAmount %= .10m;
            changeAmount += (0.10m * tempDim);

            changeAmount = Math.Round(changeAmount, 2);
            nickels = (int)Math.Abs((changeAmount / 0.05m));
            tempNickle = CheckAvailableCoinCount(0.05m, nickels);
            nickels -= tempNickle;
            changeAmount %= .05m;
            changeAmount += (0.05m * tempNickle);

            // Console.WriteLine($"-----Total amount ---{changeAmount}");
            // Console.WriteLine($"Two Euro: {twoEuro}");
            // Console.WriteLine($"One Euro: {oneEuro}");
            // Console.WriteLine($"Half: {half}");
            // Console.WriteLine($"Quarters: {quarters}");
            // Console.WriteLine($"Dimes: {dimes}");
            // Console.WriteLine($"Nickels: {nickels}");

            isExactChangeOnly = changeAmount > 0;

        }

        public void CalculateAvailableCoins()
        {
            availableCoins[2.00m] = availableCoins.GetValueOrDefault(2.00m) - twoEuro;
            availableCoins[1.00m] = availableCoins.GetValueOrDefault(1.00m) - oneEuro;
            availableCoins[0.50m] = availableCoins.GetValueOrDefault(0.50m) - half;
            availableCoins[0.20m] = availableCoins.GetValueOrDefault(0.20m) - quarters;
            availableCoins[0.10m] = availableCoins.GetValueOrDefault(0.10m) - dimes;
            availableCoins[0.05m] = availableCoins.GetValueOrDefault(0.05m) - nickels;
        }
        private int CheckAvailableCoinCount(decimal coin, int coinCount)
        {

            var availableCoinCount = availableCoins.GetValueOrDefault(coin);
            if (availableCoinCount > 0 && availableCoinCount >= coinCount)
            {
                return 0;
            }
            else if (availableCoinCount > 0 && availableCoinCount < coinCount)
            {
                return coinCount - availableCoinCount;
            }
            return coinCount;
        }
    }
}