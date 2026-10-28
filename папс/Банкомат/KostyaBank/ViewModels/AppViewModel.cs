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

    public void ShowGetForm(Card card)
    {
        CurrentView = new GetFormViewModel(card, this);
    }

    public void ShowShowForm(Card card)
    {
        CurrentView = new ShowFormViewModel(card, this);
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
