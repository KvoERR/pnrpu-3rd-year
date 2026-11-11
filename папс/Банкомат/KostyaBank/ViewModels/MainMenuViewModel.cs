using System.Windows.Input;
using KostyaBankApp.Models;
using KostyaBankApp.Services;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// ViewModel for the main menu view.
/// </summary>
public class MainMenuViewModel : BaseViewModel
{
    private readonly Card _card;
    private readonly INavigationService _navigation;
    private readonly ICommand _showCommand;
    private readonly ICommand _getCommand;
    private readonly ICommand _exitCommand;

    public MainMenuViewModel(Card card, INavigationService navigation)
    {
        _card = card ?? throw new ArgumentNullException(nameof(card));
        _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
        
        _showCommand = new SimpleCommand(() => _navigation.Navigate("showForm", card));
        _getCommand = new SimpleCommand(() => _navigation.Navigate("getForm", card));
        _exitCommand = new SimpleCommand(() => _navigation.Navigate("cardEntry"));
    }

    public string CardNumber => _card.CardNumber;
    public string MaskedCardNumber => $"**** **** **** {CardNumber.Substring(12)}";
    public string HolderName => _card.HolderName;
    public decimal Balance => _card.Balance;

    public ICommand ShowCommand => _showCommand;
    public ICommand GetCommand => _getCommand;
    public ICommand ExitCommand => _exitCommand;
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
