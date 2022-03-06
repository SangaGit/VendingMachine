// See https://aka.ms/new-console-template for more information
using System.Globalization;
using VendingMachine;
using VendingMachine.Models;
using System.Resources;
using System.Reflection;
using System;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Linq;
using VendingMachine.Repositories;
using VendingMachine.Factories;
using VendingMachine.Services;

var host = CreateHostBuilder(args).Build();

/* ######------ Without Dependency Injection ------######*/

//parameter variables
// var pathToCsv = "./Data/ProductStock.csv";
// CultureInfo culture = new CultureInfo("fr-FR");
// var prod1 = new Product(){Id = 1, Name = "COLA", UnitPrice = 1.00, Quantity = 10};
// var prod2 = new Product(){Id = 2, Name = "Chips", UnitPrice = 0.50, Quantity = 12};
// var prod3 = new Product(){Id = 3, Name = "Candy", UnitPrice = 0.65, Quantity = 0};
// ResourceManager rm = new ResourceManager("VendingMachine.Resources.Translate", Assembly.GetExecutingAssembly());

// var prodManager = new ProductManager(rm,prod1, prod2, prod3);
// prodManager.SeedProductsDataCSV(pathToCsv);
// var machine = new Machine(rm,prodManager);
// machine.Run();
// Console.WriteLine(Thread.CurrentThread.CurrentUICulture.Name);


/* #######------ With Dependency Injection ------#####*/

//change language
IConfiguration configuration = host.Services.GetRequiredService<IConfiguration>();
var languages = configuration.GetSection("Localization").GetChildren().AsEnumerable().ToDictionary(x => x.Key,x=> x.Value);
Console.WriteLine("Please select the preferred language(EN | DE | FR)");
Console.Write("LANGUAGE ");
string? lang = Console.ReadLine();
if(!string.IsNullOrEmpty(lang)){
    lang = lang.ToUpper();
}
Console.WriteLine();

Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(languages.GetValueOrDefault(lang??"EN")??"en-US");
// host.Services.GetRequiredService<ProductManager>().SeedProductsDataCSV(pathToCsv); //uncomment this line & line# 20, if need to seed product stock from attached CSV file

//run vending machine
host.Services.GetRequiredService<IMachineManager>().Run();

static IHostBuilder CreateHostBuilder(string[] args){
    return Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services)=>{
                services.AddLogging(fs => fs.AddConsole());
                services.AddTransient<IMachineManager,MachineManager>();
                services.AddSingleton<IProductRepository,ProductRepository>(x => {
                    return new ProductRepository(
                        x.GetRequiredService<ITranslateService>(),
                        x.GetRequiredService<ILogger<ProductRepository>>(),
                        new Product(){Id = 1, Name = "COLA", UnitPrice = 1.00, Quantity = 10},
                        new Product(){Id = 2, Name = "Chips", UnitPrice = 0.50, Quantity = 12},
                        new Product(){Id = 3, Name = "Candy", UnitPrice = 0.65, Quantity = 0});
                });
                services.AddSingleton<IResourceManagerFactory,ResourceManagerFactory>();
                services.AddSingleton<ITranslateService,TranslateService>();
            });
}
