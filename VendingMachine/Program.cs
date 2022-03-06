using System.Globalization;
using VendingMachine;
using VendingMachine.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using VendingMachine.Repositories;
using VendingMachine.Factories;
using VendingMachine.Services;
using VendingMachine.Data;

var host = CreateHostBuilder(args).Build();

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
// host.Services.GetRequiredService<IDataContext>().SeedProductsDataCSV("./Data/ProductStock.csv"); //uncomment this line & line# 20, if need to seed product stock from attached CSV file

//run vending machine
host.Services.GetRequiredService<IMachineManager>().Run();

static IHostBuilder CreateHostBuilder(string[] args){
    return Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services)=>{
                services.AddLogging(fs => fs.AddConsole());
                services.AddScoped<IDataContext,DataContext>();
                services.AddTransient<IMachineManager,MachineManager>();
                services.AddTransient<ICoinService,CoinService>();
                services.AddSingleton<IProductRepository,ProductRepository>();
                services.AddSingleton<IResourceManagerFactory,ResourceManagerFactory>();
                services.AddSingleton<ITranslateService,TranslateService>();
            });
}
