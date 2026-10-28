using System.Windows;
using System.Windows.Threading;
using KostyaBankApp.ViewModels;

namespace KostyaBankApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Global error handler
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        
        base.OnStartup(e);

        var appViewModel = new AppViewModel();
        var mainWindow = new Views.MainWindow();
        mainWindow.DataContext = appViewModel;
        mainWindow.Show();
        Application.Current.MainWindow = mainWindow;
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"Error: {e.Exception.Message}\n\n{e.Exception.StackTrace}");
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}\n\n{ex.StackTrace}");
        }
    }
}
