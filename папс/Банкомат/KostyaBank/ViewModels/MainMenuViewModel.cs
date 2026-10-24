using System.Windows;
using System.Windows.Input;
using KostyaBankApp.Models;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// ViewModel for the main menu window.
/// </summary>
public class MainMenuViewModel : BaseViewModel
{
    private readonly Card _card;
    private readonly ICommand _balanceCommand;
    private readonly ICommand _depositCommand;
    private readonly ICommand _withdrawCommand;
    private readonly ICommand _historyCommand;
    private readonly ICommand _settingsCommand;
    private readonly ICommand _exitCommand;

    public MainMenuViewModel(Card card)
    {
        _card = card ?? throw new ArgumentNullException(nameof(card));
        
        _balanceCommand = new SimpleCommand(() => ShowSection("Баланс"));
        _depositCommand = new SimpleCommand(() => ShowSection("Пополнение"));
        _withdrawCommand = new SimpleCommand(() => ShowSection("Снятие"));
        _historyCommand = new SimpleCommand(() => ShowSection("История"));
        _settingsCommand = new SimpleCommand(() => ShowSection("Настройки"));
        _exitCommand = new SimpleCommand(() => ExitApp());
    }

    public string MaskedCardNumber => _card.MaskedNumber;

    public ICommand BalanceCommand => _balanceCommand;
    public ICommand DepositCommand => _depositCommand;
    public ICommand WithdrawCommand => _withdrawCommand;
    public ICommand HistoryCommand => _historyCommand;
    public ICommand SettingsCommand => _settingsCommand;
    public ICommand ExitCommand => _exitCommand;

    private void ShowSection(string sectionName)
    {
        System.Windows.MessageBox.Show(
            $"{sectionName} для карты {_card.MaskedNumber}\n\nЭтот раздел в разработке.",
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
            System.Windows.Application.Current.Shutdown();
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
