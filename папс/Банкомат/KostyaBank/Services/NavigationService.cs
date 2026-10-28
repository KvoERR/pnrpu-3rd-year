using KostyaBankApp.Models;
using KostyaBankApp.ViewModels;

namespace KostyaBankApp.Services;

public class NavigationService
{
    private readonly Card _card;

    public NavigationService(Card card)
    {
        _card = card;
    }

    public void ShowMainMenu()
    {
        var viewModel = new MainMenuViewModel(_card);
        var window = new Views.MainWindow(viewModel);
        window.Show();
    }

    public void ShowServiceView()
    {
        var viewModel = new ServiceViewModel(_card);
        var window = new Views.ServiceView(viewModel);
        window.Show();
    }
}
