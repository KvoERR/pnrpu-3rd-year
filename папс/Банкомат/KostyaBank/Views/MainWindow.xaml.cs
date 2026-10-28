using System.Windows;
using System.Windows.Input;
using KostyaBankApp.ViewModels;

namespace KostyaBankApp.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public AppViewModel? AppViewModel => DataContext as AppViewModel;

    public MainWindow()
    {
        InitializeComponent();
        
        // Use AddHandler to capture even when other elements handle the event
        this.AddHandler(Window.PreviewKeyDownEvent, new KeyEventHandler(MainWindow_PreviewKeyDown), true);
    }

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.S)
        {
            try
            {
                var appVm = AppViewModel;
                appVm.ShowServiceView();
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
    }
}
