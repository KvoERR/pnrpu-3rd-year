using System.Windows;
using System.Windows.Threading;
using KostyaBankApp.Models;
using KostyaBankApp.Services;
using KostyaBankApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace KostyaBankApp;

public static class Log
{
    private static readonly string _path = System.IO.Path.Combine(
        System.IO.Path.GetTempPath(), "KostyaBank.log");

    public static void Write(string msg)
    {
        try
        {
            System.IO.File.AppendAllText(_path, $"{DateTime.Now:HH:mm:ss.fff} {msg}\n");
        }
        catch { }
    }
}

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        Log.Write("=== App started ===");

        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        base.OnStartup(e);

        var baseDir = AppContext.BaseDirectory;

        // --- Зависимости (DIP) ---
        var cardRepository         = new ExcelCardRepository(Path.Combine(baseDir, "comp.xlsx"));
        var cardStorageRepository  = new CardStorageRepository(Path.Combine(baseDir, "card_storage.xlsx"));
        var moneyStorageModel      = new MoneyStorageModel();
        var nominalProvider        = new DefaultNominalProvider();
        var moneyStorageRepository = new MoneyStorageRepository(moneyStorageModel, nominalProvider, Path.Combine(baseDir, "money_storage.xlsx"));
        var cardValidator          = new CardValidator();
        var outputDir              = Path.Combine(baseDir, "Output");
        var excelPrinter           = new ExcelPrinter(outputDir);

        // --- DI-контейнер ---
        var services = new ServiceCollection();
        services.AddSingleton<ICardRepository>(cardRepository);
        services.AddSingleton<ICardStorageRepository>(cardStorageRepository);
        services.AddSingleton<ICardStorageReader>(cardStorageRepository);
        services.AddSingleton<ICardStorageWriter>(cardStorageRepository);
        services.AddSingleton<IMoneyStorageRepository>(moneyStorageRepository);
        services.AddSingleton<IMoneyStorageReader>(moneyStorageRepository);
        services.AddSingleton<IMoneyStorageWriter>(moneyStorageRepository);
        services.AddSingleton<IMoneyStorageReloader>(moneyStorageRepository);
        services.AddSingleton<IReceiptPrinter>(excelPrinter);
        services.AddSingleton<INominalProvider, DefaultNominalProvider>();
        services.AddSingleton<ICardValidator, CardValidator>();
        services.AddSingleton<ICardAuthService, CardAuthService>();
        services.AddSingleton<IViewModelFactory, ViewModelFactory>();
        services.AddSingleton<INavigator, Navigator>();
        services.AddTransient<CardEntryViewModel>();
        services.AddTransient<MainMenuViewModel>();
        services.AddTransient<GetFormViewModel>();
        services.AddTransient<ShowFormViewModel>();
        services.AddTransient<ServiceViewModel>();
        services.AddTransient<MoneyStorageViewModel>();
        services.AddTransient<CardStorageViewModel>();

        var serviceProvider = services.BuildServiceProvider();

        // --- Навигатор как DataContext окна ---
        var navigator = serviceProvider.GetRequiredService<INavigator>();

        var mainWindow = new Views.MainWindow();
        mainWindow.DataContext = navigator;
        mainWindow.Show();
        Application.Current.MainWindow = mainWindow;

        // --- Стартовый экран: одна строка, без регистрации ---
        navigator.Navigate<CardEntryViewModel>();

        Log.Write("MainWindow shown");
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Write($"Dispatcher Error: {e.Exception.Message}\n{e.Exception.StackTrace}");
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            Log.Write($"AppDomain Error: {ex.Message}\n{ex.StackTrace}");
    }
}
