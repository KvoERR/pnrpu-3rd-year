using System.Windows;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// Навигатор экранов. Реализует INavigationService (DIP).
/// Создание VMs делегировано фабрикам (SRP).
/// </summary>
public class AppViewModel : BaseViewModel, INavigationService
{
    private object _currentView;
    public object CurrentView
    {
        get => _currentView;
        private set => SetProperty(ref _currentView, value);
    }

    public AppViewModel()
    {
        CurrentView = CardEntryViewModelFactory.Create(this);
    }

    public void ShowCardEntry()
    {
        CurrentView = CardEntryViewModelFactory.Create(this);
    }

    public void ShowMainMenu(Card card)
    {
        CurrentView = MainMenuViewModelFactory.Create(card, this);
    }

    public void ShowGetForm(Card card)
    {
        CurrentView = GetFormViewModelFactory.Create(card, this);
    }

    public void ShowShowForm(Card card)
    {
        CurrentView = ShowFormViewModelFactory.Create(card, this);
    }

    public void ShowServiceView()
    {
        CurrentView = ServiceViewModelFactory.Create(this);
    }

    public void ShowMoneyStorage()
    {
        CurrentView = MoneyStorageViewModelFactory.Create(this);
    }

    public void ShowCardStorage()
    {
        CurrentView = CardStorageViewModelFactory.Create(this);
    }

    public void ExitApp()
    {
        var result = MessageBox.Show(
            "Вы уверены, что хотите выйти?",
            "Выход",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
            System.Windows.Application.Current.Shutdown();
    }
}
