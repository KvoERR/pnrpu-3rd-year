using KostyaBankApp.Models;

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
        Log.Write("AppViewModel created, setting CurrentView to CardEntryViewModel");
        // Start with card entry view
        CurrentView = new CardEntryViewModel(this);
        Log.Write("CardEntryViewModel set as CurrentView");
    }

    public void ShowCardEntry()
    {
        Log.Write("ShowCardEntry called");
        CurrentView = new CardEntryViewModel(this);
        Log.Write("CardEntryViewModel set as CurrentView");
    }

    public void ShowMainMenu(Card card)
    {
        Log.Write($"ShowMainMenu called for card {card?.CardNumber ?? "null"}");
        CurrentView = new MainMenuViewModel(card, this);
        Log.Write("MainMenuViewModel set as CurrentView");
    }

    public void ShowServiceView()
    {
        Log.Write("ShowServiceView called");
        try
        {
            var vm = new ServiceViewModel(this);
            Log.Write("ServiceViewModel created successfully");
            CurrentView = vm;
            Log.Write("ServiceViewModel set as CurrentView");
        }
        catch (Exception ex)
        {
            Log.Write($"Error creating ServiceViewModel: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
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
