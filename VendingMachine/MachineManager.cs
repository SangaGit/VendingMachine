using VendingMachine.Models;
using Microsoft.Extensions.Configuration;
using VendingMachine.Repositories;
using VendingMachine.Services;

namespace VendingMachine
{
    class MachineManager : IMachineManager
    {
        private readonly IProductRepository _productRepository;
        private readonly ITranslateService _translateService;
        private readonly IConfiguration _config;
        private readonly List<double> _validCoins; //= new List<double>(){0.05, 0.10, 0.20, 0.50, 1.00, 2.00};
        private Dictionary<double, int> availableCoins = new Dictionary<double, int>();
        private double depositedAmount;
        private double currentAmount;
        private List<Product> products;
        private string _currency = "C2";
        public MachineManager(ITranslateService translateService, IProductRepository productRepository, IConfiguration config)
        {
            _productRepository = productRepository;
            products = productRepository.Products;
            _translateService = translateService;
            _config = config;
            _validCoins = _config.GetValue<string>("ValidCoins").Split(",").Select(x => double.Parse(x)).ToList();
            availableCoins = _config.GetSection("AvailableCoins").GetChildren().AsEnumerable().ToDictionary(x => double.Parse(x.Key), x => int.Parse(x.Value));
        }

        public bool IsAcceptableCoin(double amount)
        {
            // return amount >= 0.05 && amount <=2.00m
            return _validCoins.Contains(amount);
        }
        public double DepositCoin(double amount)
        {
            depositedAmount += amount;
            return depositedAmount;
        }
        private void InserCoin()
        {
            currentAmount = 0;
            // if(IsExactChangeOnly()){
            //     Console.Write("EXACT CHANGE ONLY");
            //     Console.ReadKey();
            // }
            Console.WriteLine($"{_translateService.Translate("INSERT COIN(Accepted coins")} {string.Join(", ", _validCoins.Select(x => x.ToString(_currency)))})");
            Console.Write("ENTER AMOUNT ");
            var coin = Console.ReadLine();
            try
            {
                if (!IsAcceptableCoin(double.Parse(coin ?? "0")))
                {
                    Console.WriteLine(_translateService.Translate("Please insert valid coin."));
                    Console.WriteLine(_translateService.Translate("Collect your coin..."));
                    InserCoin();
                }
            }
            catch (Exception)
            {
                Console.WriteLine(_translateService.Translate("Please insert valid coin."));
                InserCoin();
            }
            currentAmount = double.Parse(coin ?? "0");
            DepositCoin(currentAmount);
            Console.WriteLine($"{_translateService.Translate("Amount entered:")} { depositedAmount.ToString(_currency)}");
            Console.Write($"{_translateService.Translate("Do you want to insert more?(Y/N)")}");
            var answer = Console.ReadLine();
            if (answer != null && answer.ToUpper() == "Y")
            {
                InserCoin();
            }
            else
            {
                _productRepository.DisplayProducts();
                SelectProduct();
            }
        }

        private void SelectProduct()
        {
            // DisplayProducts();
            Console.Write("SELECT PRODUCT ");
            try
            {
                var productNumber = int.Parse(Console.ReadLine() ?? "0");
                Console.WriteLine();
                if (productNumber == 4)
                {
                    if (depositedAmount > 0)
                    {
                        Console.WriteLine($"{_translateService.Translate("Please collect your balance:")} {depositedAmount.ToString(_currency)}");
                    }
                    depositedAmount = 0;
                    Console.WriteLine($"{_translateService.Translate("Thank you. Have a good day!!!")}");
                    Run();
                    return;
                }
                var product = products.FirstOrDefault(prod => prod.Id == productNumber);
                if (product != null && product.Quantity > 0)
                {
                    if (depositedAmount >= product.UnitPrice)
                    {
                        _productRepository.ReduceProductQuantity(product);
                        Console.WriteLine(_translateService.Translate("Dispensing....."));
                        // Console.WriteLine($"Please take out the product {product.Name}");
                        if ((depositedAmount - product.UnitPrice) > 0)
                        {
                            Console.WriteLine($"{_translateService.Translate("Please collect your balance:")} {(depositedAmount - product.UnitPrice).ToString(_currency)}");
                        }
                        depositedAmount = 0;
                        Console.WriteLine($"{_translateService.Translate("Thank you. Have a good day!!!")}");
                        Run();

                    }
                    else
                    {
                        Console.WriteLine($"-----###### {_translateService.Translate("Selected Product")} ######-----");
                        Console.WriteLine();
                        Console.WriteLine($"{product.Id}. {product.Name} {product.UnitPrice.ToString(_currency)} - {product.Quantity} {_translateService.Translate("Items Left")}");
                        Console.WriteLine();
                        Console.WriteLine($"{_translateService.Translate("Insufficient balance")} {depositedAmount.ToString(_currency)}");

                        Console.Write($"{_translateService.Translate("Do you want to insert more?(Y/N)")}");
                        if ((Console.ReadLine() ?? "N").ToUpper() == "Y")
                        {
                            InserCoin();
                        }
                        else
                        {
                            //may be want to select different product which is available for existing amount
                            if (_productRepository.DisplayAvailableProductsByAmount(depositedAmount))
                            {
                                SelectProduct();
                            }
                            else
                            {
                                if (depositedAmount > 0)
                                {
                                    Console.WriteLine($"{_translateService.Translate("Please collect your balance:")} {depositedAmount.ToString(_currency)}");
                                }
                                depositedAmount = 0;
                                Console.WriteLine($"{_translateService.Translate("Thank you. Have a good day!!!")}");
                                Run();
                            }

                        }
                    }
                }
                else if (product != null && product.Quantity == 0)
                {
                    Console.WriteLine($"-----{_translateService.Translate("Selected Product is SOLD OUT")}!!!-----");
                    _productRepository.DisplayProducts();
                    Console.Write(_translateService.Translate("Do you want to select another product?(Y/N)"));
                    if ((Console.ReadLine() ?? "N").ToUpper() == "Y")
                    {
                        SelectProduct();
                    }
                    else
                    {
                        // Console.WriteLine("INSERT COIN");
                        if (depositedAmount > 0)
                        {
                            Console.WriteLine($"{_translateService.Translate("Please collect your balance:")} {depositedAmount.ToString(_currency)}");
                        }
                        Console.WriteLine($"{_translateService.Translate("Thank you. Have a good day!!!")}");
                        depositedAmount = 0;
                        Run();
                    }
                }
                else
                {
                    Console.WriteLine(_translateService.Translate("Invalid input. Please select available Item from the menu"));
                    _productRepository.DisplayProducts();
                    SelectProduct();
                }
            }
            catch (Exception)
            {
                Console.WriteLine(_translateService.Translate("Invalid input. Please select available Item from the menu"));
                _productRepository.DisplayProducts();
                SelectProduct();
            }
        }
        private void WelcomeMessage()
        {
            Console.WriteLine();
            Console.WriteLine($"****** {_translateService.Translate("Welcome to Dennmayer Vending Machine")} ******");
            Console.WriteLine();
        }
        public void Run()
        {
            WelcomeMessage();
            _productRepository.DisplayProducts();
            foreach (var change in calculatePossibleChanges())
            {
                if (IsExactChangeOnly(change))
                {
                    //check available coins in appsettings.json file. change the counts to 0 in there and check accordingly
                    Console.Write($"{_translateService.Translate("EXACT CHANGE ONLY")}");
                    Console.ReadKey();
                    break;
                }
            }
            InserCoin();
        }

        private List<decimal> calculatePossibleChanges()
        {

            var possibleChanges = new List<decimal>();
            //possible changes could be 0.05, 0.10, 0.15, 0.20, .... 1.55, 2.00
            foreach (var prod in products)
            {
                if (prod.UnitPrice >= 1 && prod.UnitPrice <= 2)
                {
                    possibleChanges.Add(2 - (decimal)prod.UnitPrice);
                }
                else if (prod.UnitPrice > 0 && prod.UnitPrice < 1)
                {
                    possibleChanges.Add(1 - (decimal)prod.UnitPrice);
                }
            }
            // for(var i = 0.05m; i <= 2.00m; i+=0.05m){
            //     possibleChanges.Add(i);
            // }
            return possibleChanges;
        }
        private bool IsExactChangeOnly(decimal changeAmount)
        {
            //check available coins in appsettings.json file. change the counts to 0 in there and check accordingly
            int twoEuro, oneEuro, half, quarters, dimes, nickels;
            int tempTwo, tempOne, tempHalf, tempQuater, tempDim, tempNickle;


            twoEuro = (int)Math.Abs((changeAmount / 2));
            tempTwo = checkAvailableCoinCount(2, twoEuro);
            twoEuro -= tempTwo;
            changeAmount %= 2;
            changeAmount += (2 * tempTwo);


            changeAmount = Math.Round(changeAmount, 2);
            oneEuro = (int)Math.Abs((changeAmount / 1));
            tempOne = checkAvailableCoinCount(1, oneEuro);
            oneEuro -= tempOne;
            changeAmount %= 1;
            changeAmount += (1 * tempOne);


            changeAmount = Math.Round(changeAmount, 2);
            half = (int)Math.Abs((changeAmount / .50m));
            tempHalf = checkAvailableCoinCount(0.50, half);
            half -= tempHalf;
            changeAmount %= .50m;
            changeAmount += (0.50m * tempHalf);


            changeAmount = Math.Round(changeAmount, 2);
            quarters = (int)Math.Abs((changeAmount / .20m));
            tempQuater = checkAvailableCoinCount(0.20, quarters);
            quarters -= tempQuater;
            changeAmount %= .20m;
            changeAmount += (0.20m * tempQuater);


            changeAmount = Math.Round(changeAmount, 2);
            dimes = (int)Math.Abs((changeAmount / .10m));
            tempDim = checkAvailableCoinCount(0.10, dimes);
            dimes -= tempDim;
            changeAmount %= .10m;
            changeAmount += (0.10m * tempDim);

            changeAmount = Math.Round(changeAmount, 2);
            nickels = (int)Math.Abs((changeAmount / 0.05m));
            tempNickle = checkAvailableCoinCount(0.05, nickels);
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
            return changeAmount > 0;

        }
        private int checkAvailableCoinCount(double coin, int coinCount)
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