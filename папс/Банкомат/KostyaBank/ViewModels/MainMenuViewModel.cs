using System.Windows;
using System.Windows.Input;
using KostyaBankApp.Models;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// ViewModel for the main menu view.
/// </summary>
public class MainMenuViewModel : BaseViewModel
{
    private readonly Card _card;
    private readonly AppViewModel _appViewModel;
    private readonly ICommand _showCommand;
    private readonly ICommand _getCommand;
    private readonly ICommand _exitCommand;

    public MainMenuViewModel(Card card, AppViewModel appViewModel)
    {
        _card = card ?? throw new ArgumentNullException(nameof(card));
        _appViewModel = appViewModel;
        
        _showCommand = new SimpleCommand(() => _appViewModel.ShowShowForm(_card));
        _getCommand = new SimpleCommand(() => _appViewModel.ShowGetForm(_card));
        _exitCommand = new SimpleCommand(() => ExitApp());
    }

    public string CardNumber => _card.CardNumber;
    public string MaskedCardNumber => $"**** **** **** {CardNumber.Substring(12)}";
    public string HolderName => _card.HolderName;
    public decimal Balance => _card.Balance;

    public ICommand ShowCommand => _showCommand;
    public ICommand GetCommand => _getCommand;
    public ICommand ExitCommand => _exitCommand;

    private void ShowSection(string sectionName)
    {
        System.Windows.MessageBox.Show(
            $"{sectionName}\n\nКлиент: {_card.HolderName}\nБаланс: {_card.Balance:C}\n\nЭтот раздел в разработке.",
            sectionName,
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);
    }

    private void ExitApp()
    {
        var result = System.Windows.MessageBox.Show(
            "Вы уверены, что хотите выйти?",
            "Выход",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            _appViewModel.ShowCardEntry();
        }
    }
}

/// <summary>
/// Simple ICommand implementation for MVVM.
/// </summary>
public class SimpleCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public SimpleCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
