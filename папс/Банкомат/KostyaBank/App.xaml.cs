using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Input;
using KostyaBankApp.Models;
using KostyaBankApp.ViewModels;

namespace KostyaBankApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        EventManager.RegisterClassHandler(
            typeof(Window),
            Window.PreviewKeyDownEvent,
            new KeyEventHandler(App_PreviewKeyDown),
            handledEventsToo: true);
    }

    private void App_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.S)
        {
            var repo = new CardRepository("cards.xlsx");
            var card = repo.GetCardByNumber("4276590123456789");

            if (card != null)
            {
                var viewModel = new ServiceViewModel(card);
                var window = new Views.ServiceView(viewModel)
                {
                    Owner = Application.Current.MainWindow
                };
                window.Show();
                e.Handled = true;
            }
        }
    }
}
