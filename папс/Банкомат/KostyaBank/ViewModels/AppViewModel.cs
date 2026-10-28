using KostyaBankApp.Models;
using KostyaBankApp.ViewModels;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// Manages navigation between views in the single MainWindow.
/// </summary>
public class AppViewModel : BaseViewModel
{
    private object _currentView;

    public object CurrentView
    {
        get => _currentView;
        private set => SetProperty(ref _currentView, value);
    }

    public AppViewModel()
    {
        // Start with card entry view
        CurrentView = new CardEntryViewModel(this);
    }

    public void ShowCardEntry()
    {
        CurrentView = new CardEntryViewModel(this);
    }

    public void ShowMainMenu(Card card)
    {
        CurrentView = new MainMenuViewModel(card, this);
    }

    public void ShowServiceView()
    {
        CurrentView = new ServiceViewModel(this);
    }

    public void ExitApp()
    {
        var result = System.Windows.MessageBox.Show(
            "Вы уверены, что хотите выйти?",
            "Выход",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
