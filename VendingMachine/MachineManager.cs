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
        private readonly ICoinService _coinService;
        private decimal depositedAmount;
        private decimal currentAmount;
        private IEnumerable<Product> products;
        private string _currency = "C2";
        public MachineManager(ITranslateService translateService, IProductRepository productRepository, ICoinService coinService, IConfiguration config)
        {
            _productRepository = productRepository;
            products = productRepository.Products;
            _translateService = translateService;
            _config = config;
            _coinService = coinService;
        }

        public bool IsAcceptableCoin(decimal amount)
        {
            // return amount >= 0.05 && amount <=2.00m
            return _coinService.ValidCoins.Contains(amount);
        }
        public decimal DepositCoin(decimal amount)
        {
            depositedAmount += amount;
            return depositedAmount;
        }
        private void InserCoin()
        {
            currentAmount = 0;
            Console.WriteLine($"{_translateService.Translate("INSERT COIN(Accepted coins")} {string.Join(", ", _coinService.ValidCoins.Select(x => x.ToString(_currency)))})");
            Console.Write("ENTER AMOUNT ");
            var coin = Console.ReadLine();
            try
            {
                if (!IsAcceptableCoin(decimal.Parse(coin ?? "0")))
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
            currentAmount = decimal.Parse(coin ?? "0");
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
                            _coinService.CalculateNumberOfCoinsForChange(depositedAmount - product.UnitPrice);
                            _coinService.CalculateAvailableCoins();
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
            _coinService.CheckCoinAvailability();
            InserCoin();
        }

    }
}