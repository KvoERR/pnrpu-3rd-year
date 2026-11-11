using System.Windows;
using System.Windows.Threading;
using KostyaBankApp.Models;
using KostyaBankApp.Services;
using KostyaBankApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace KostyaBankApp;

/// <summary>
/// Simple file logger for debugging
/// </summary>
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

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        Log.Write("=== App started ===");

        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        
        base.OnStartup(e);

        // Создаём зависимости (DIP)
        var cardRepository = new ExcelCardRepository();
        var cardStorageRepository = new CardStorageRepository();
        var moneyStorageModel = new MoneyStorageModel();
        var moneyStorageRepository = new MoneyStorageRepository(moneyStorageModel);

        // DI-контейнер: регистрируем сервисы и VMs
        var services = new ServiceCollection();
        services.AddSingleton<ICardRepository>(cardRepository);
        services.AddSingleton<ICardStorageRepository>(cardStorageRepository);
        services.AddSingleton<IMoneyStorageRepository>(moneyStorageRepository);
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddTransient<CardEntryViewModel>();
        services.AddTransient<MainMenuViewModel>();
        services.AddTransient<GetFormViewModel>();
        services.AddTransient<ShowFormViewModel>();
        services.AddTransient<ServiceViewModel>();
        services.AddTransient<MoneyStorageViewModel>();
        services.AddTransient<CardStorageViewModel>();

        var serviceProvider = services.BuildServiceProvider();

        // Создаём навигатор (он же DataContext)
        var navigation = serviceProvider.GetRequiredService<INavigationService>() as NavigationService
            ?? throw new InvalidOperationException("NavigationService not found");
        navigation.RegisterRoute("cardEntry", typeof(CardEntryViewModel));
        navigation.RegisterRoute("mainMenu", typeof(MainMenuViewModel));
        navigation.RegisterRoute("getForm", typeof(GetFormViewModel));
        navigation.RegisterRoute("showForm", typeof(ShowFormViewModel));
        navigation.RegisterRoute("service", typeof(ServiceViewModel));
        navigation.RegisterRoute("moneyStorage", typeof(MoneyStorageViewModel));
        navigation.RegisterRoute("cardStorage", typeof(CardStorageViewModel));

        navigation.Navigate("cardEntry");

        var mainWindow = new Views.MainWindow();
        mainWindow.DataContext = navigation;
        mainWindow.Show();
        Application.Current.MainWindow = mainWindow;
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
        {
            Log.Write($"AppDomain Error: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
