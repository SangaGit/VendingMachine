using VendingMachine.Models;
using System.Resources;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace VendingMachine
{
    class Machine
    {
        private readonly ProductManager _productManager;
        private readonly ResourceManager _rm;
        private readonly ILogger<Machine> _logger;
        private readonly IConfiguration _config;
        private readonly List<double> _validCoins; //= new List<double>(){0.05, 0.10, 0.20, 0.50, 1.00, 2.00};
        private Dictionary<double,int> availableCoins = new Dictionary<double, int>();
        private double depositedAmount;
        private double currentAmount;
        private List<Product> products;
        private string _currency = "C2";
        public Machine(ResourceManager rm, ProductManager prodManager, IConfiguration config, ILogger<Machine> logger)
        {
            _productManager = prodManager;
            products = prodManager.Products;
            _rm = rm;
            _logger = logger;
            _config = config;
            _validCoins = _config.GetValue<string>("ValidCoins").Split(",").Select(x => double.Parse(x)).ToList();
            availableCoins = _config.GetSection("AvailableCoins").GetChildren().AsEnumerable().ToDictionary(x => double.Parse(x.Key),x=> int.Parse(x.Value));
        }

        public bool IsAcceptableCoin(double amount){
            // return amount >= 0.05 && amount <=2.00m
            return _validCoins.Contains(amount);
        }
        public double DepositCoin(double amount){
            depositedAmount += amount;
            return depositedAmount;
        }
        private void InserCoin(){
            currentAmount = 0;
            // if(IsExactChangeOnly()){
            //     Console.Write("EXACT CHANGE ONLY");
            //     Console.ReadKey();
            // }
            Console.WriteLine($"{_rm.GetString("INSERT COIN(Accepted coins")} {string.Join(", ",_validCoins.Select(x => x.ToString(_currency)))})");
            Console.Write("ENTER AMOUNT ");
            var coin = Console.ReadLine();
            try
            {
                if(!IsAcceptableCoin(double.Parse(coin?? "0"))){
                    Console.WriteLine(_rm.GetString("Please insert valid coin."));
                    Console.WriteLine(_rm.GetString("Collect your coin..."));
                    InserCoin();
                }
            }
            catch (Exception)
            {
                Console.WriteLine(_rm.GetString("Please insert valid coin."));
                InserCoin();
            }
            currentAmount = double.Parse(coin??"0");
            DepositCoin(currentAmount);
            Console.WriteLine($"{_rm.GetString("Amount entered:")} { depositedAmount.ToString(_currency)}");
            Console.Write($"{_rm.GetString("Do you want to insert more?(Y/N)")}");
            var answer = Console.ReadLine();
            if(answer != null && answer.ToUpper() == "Y"){
                InserCoin();
            }
            else{
                _productManager.DisplayProducts();
                SelectProduct();
            }
        }

        private void SelectProduct(){
            // DisplayProducts();
            Console.Write("SELECT PRODUCT ");
            try
            {
                var productNumber = int.Parse(Console.ReadLine() ?? "0");
                Console.WriteLine();
                if(productNumber == 4){
                    if(depositedAmount > 0){
                        Console.WriteLine($"{_rm.GetString("Please collect your balance:")} {depositedAmount.ToString(_currency)}");
                    }
                    depositedAmount = 0;
                    Console.WriteLine($"{_rm.GetString("Thank you. Have a good day!!!")}");
                    Run();
                    return;
                }
                var product = products.FirstOrDefault(prod => prod.Id == productNumber);
                if(product != null && product.Quantity > 0){
                    if(depositedAmount >= product.UnitPrice){
                        _productManager.ReduceProductQuantity(product);
                        Console.WriteLine(_rm.GetString("Dispensing....."));
                        // Console.WriteLine($"Please take out the product {product.Name}");
                        if((depositedAmount - product.UnitPrice)>0){
                            Console.WriteLine($"{_rm.GetString("Please collect your balance:")} {(depositedAmount - product.UnitPrice).ToString(_currency)}");
                        }
                        depositedAmount = 0;
                        Console.WriteLine($"{_rm.GetString("Thank you. Have a good day!!!")}");
                        Run();
                        
                    }
                    else{
                        Console.WriteLine($"-----###### {_rm.GetString("Selected Product")} ######-----");
                        Console.WriteLine();
                        Console.WriteLine($"{product.Id}. {product.Name} {product.UnitPrice.ToString(_currency)} - {product.Quantity} {_rm.GetString("Items Left")}");
                        Console.WriteLine();
                        Console.WriteLine($"{_rm.GetString("Insufficient balance")} {depositedAmount.ToString(_currency)}");

                        Console.Write($"{_rm.GetString("Do you want to insert more?(Y/N)")}");
                        if((Console.ReadLine() ?? "N").ToUpper() == "Y"){
                            InserCoin();
                        }
                        else{
                            //may be want to select different product which is available for existing amount
                            if(_productManager.DisplayAvailableProductsByAmount(depositedAmount)){
                                SelectProduct();
                            }
                            else{
                                if(depositedAmount > 0){
                                    Console.WriteLine($"{_rm.GetString("Please collect your balance:")} {depositedAmount.ToString(_currency)}");
                                }
                                depositedAmount = 0;
                                Console.WriteLine($"{_rm.GetString("Thank you. Have a good day!!!")}");
                                Run();
                            }
                            
                        }
                    }
                }
                else if(product != null && product.Quantity == 0){
                    Console.WriteLine($"-----{_rm.GetString("Selected Product is SOLD OUT")}!!!-----");
                    _productManager.DisplayProducts();
                    Console.Write(_rm.GetString("Do you want to select another product?(Y/N)"));
                    if((Console.ReadLine()?? "N").ToUpper() == "Y"){
                        SelectProduct();
                    }
                    else{
                        // Console.WriteLine("INSERT COIN");
                        if(depositedAmount > 0){
                            Console.WriteLine($"{_rm.GetString("Please collect your balance:")} {depositedAmount.ToString(_currency)}");
                        }
                        Console.WriteLine($"{_rm.GetString("Thank you. Have a good day!!!")}");
                        depositedAmount = 0;
                        Run();
                    }
                }
                else{
                    Console.WriteLine(_rm.GetString("Invalid input. Please select available Item from the menu"));
                    _productManager.DisplayProducts();
                    SelectProduct();
                }
            }
            catch (Exception)
            {
                Console.WriteLine(_rm.GetString("Invalid input. Please select available Item from the menu"));
                _productManager.DisplayProducts();
                SelectProduct();
            }
        }
        private void WelcomeMessage(){
            Console.WriteLine();
            Console.WriteLine($"****** {_rm.GetString("Welcome to Dennmayer Vending Machine")} ******");
            Console.WriteLine();
        }
        public void Run(){
            WelcomeMessage();
            _productManager.DisplayProducts();
            foreach (var change in calculatePossibleChanges())
            {
                if(IsExactChangeOnly(change)){
                    //check available coins in appsettings.json file. change the counts to 0 in there and check accordingly
                    Console.Write($"{_rm.GetString("EXACT CHANGE ONLY")}");
                    Console.ReadKey();
                    break;
                }
            }
            InserCoin();
        }

        private List<decimal> calculatePossibleChanges(){

            var possibleChanges = new List<decimal>(); 
            foreach (var prod in products)
            {
                if(prod.UnitPrice >= 1 && prod.UnitPrice <= 2){
                    possibleChanges.Add(2 - (decimal)prod.UnitPrice);
                }
                else if(prod.UnitPrice > 0 && prod.UnitPrice < 1){
                    possibleChanges.Add(1 - (decimal)prod.UnitPrice);
                }
            }
            return possibleChanges;
        }
        private bool IsExactChangeOnly(decimal changeAmount){
            //check available coins in appsettings.json file. change the counts to 0 in there and check accordingly
            int twoEuro,oneEuro,half,quarters,dimes,nickels;
            int tempTwo, tempOne , tempHalf, tempQuater, tempDim, tempNickle;


                twoEuro = (int)Math.Abs((changeAmount / 2));
                tempTwo = checkAvailableCoinCount(2,twoEuro);
                twoEuro -= tempTwo;
                changeAmount %= 2;
                changeAmount +=  (2* tempTwo);
                

                changeAmount = Math.Round(changeAmount,2);
                oneEuro = (int)Math.Abs((changeAmount / 1));
                tempOne = checkAvailableCoinCount(1,oneEuro);
                oneEuro -= tempOne;
                changeAmount %= 1;
                changeAmount +=  (1* tempOne);
                

                changeAmount = Math.Round(changeAmount,2);
                half = (int)Math.Abs((changeAmount / .50m));
                tempHalf = checkAvailableCoinCount(0.50,half);
                half -= tempHalf;
                changeAmount %= .50m;
                changeAmount +=  (0.50m* tempHalf);
                

                changeAmount = Math.Round(changeAmount,2);
                quarters = (int)Math.Abs((changeAmount / .20m));
                tempQuater = checkAvailableCoinCount(0.20,quarters);
                quarters -= tempQuater;
                changeAmount %= .20m;
                changeAmount +=  (0.20m* tempQuater);
                

                changeAmount = Math.Round(changeAmount,2);
                dimes = (int)Math.Abs((changeAmount / .10m));
                tempDim = checkAvailableCoinCount(0.10,dimes);
                dimes -= tempDim;
                changeAmount %= .10m;
                changeAmount +=  (0.10m* tempDim);

                changeAmount = Math.Round(changeAmount,2);
                nickels = (int)Math.Abs((changeAmount / 0.05m));
                tempNickle = checkAvailableCoinCount(0.05,nickels);
                nickels -= tempNickle;
                changeAmount %= .05m;
                changeAmount +=  (0.05m* tempNickle);

                // Console.WriteLine($"-----Total amount ---{changeAmount}");
                // Console.WriteLine($"Two Euro: {twoEuro}");
                // Console.WriteLine($"One Euro: {oneEuro}");
                // Console.WriteLine($"Half: {half}");
                // Console.WriteLine($"Quarters: {quarters}");
                // Console.WriteLine($"Dimes: {dimes}");
                // Console.WriteLine($"Nickels: {nickels}");
            return changeAmount > 0;
            
        }
        private int checkAvailableCoinCount(double coin, int coinCount){

            var availableCoinCount = availableCoins.GetValueOrDefault(coin);
            if(availableCoinCount > 0 && availableCoinCount >= coinCount){
                return 0;
            }
            else if(availableCoinCount > 0 && availableCoinCount < coinCount){
                return coinCount - availableCoinCount;
            }
            return coinCount;
        }


    }
}