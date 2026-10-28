using System.Windows;
using System.Windows.Threading;
using KostyaBankApp.ViewModels;

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

        // Global error handler
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        
        base.OnStartup(e);

        Log.Write("Creating AppViewModel...");
        var appViewModel = new AppViewModel();
        Log.Write("Creating MainWindow...");
        var mainWindow = new Views.MainWindow();
        mainWindow.DataContext = appViewModel;
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
