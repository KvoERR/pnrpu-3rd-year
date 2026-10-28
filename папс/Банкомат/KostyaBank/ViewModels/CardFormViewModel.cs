using System.Windows;
using System.Windows.Input;
using KostyaBankApp.Models;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// Base class for card operation forms.
/// Provides card info, action button, print checkbox, and message area.
/// </summary>
public abstract class CardFormViewModel : BaseViewModel
{
    protected readonly Card Card;
    protected readonly AppViewModel AppViewModel;
    private readonly ICommand _backCommand;
    private readonly ICommand _actionCommand;
    private string _messageText = string.Empty;
    private Visibility _messageVisibility = Visibility.Collapsed;

    protected CardFormViewModel(Card card, AppViewModel appViewModel)
    {
        Card = card ?? throw new ArgumentNullException(nameof(card));
        AppViewModel = appViewModel;

        _backCommand = new SimpleCommand(() => AppViewModel.ShowMainMenu(Card));
        _actionCommand = new SimpleCommand(() => ExecuteAction(), () => CanExecuteAction);
    }

    // Card info
    public string CardNumber => Card.CardNumber;
    public string MaskedCardNumber => $"**** **** **** {CardNumber.Substring(12)}";
    public string HolderName => Card.HolderName;
    public decimal Balance => Card.Balance;

    // Navigation
    public ICommand BackCommand => _backCommand;

    // Action button
    public abstract string ActionButtonText { get; }
    public abstract bool CanExecuteAction { get; }
    public ICommand ActionCommand => _actionCommand;
    protected abstract void ExecuteAction();

    // Message area
    public string MessageText
    {
        get => _messageText;
        protected set => SetProperty(ref _messageText, value);
    }

    public Visibility MessageVisibility
    {
        get => _messageVisibility;
        protected set => SetProperty(ref _messageVisibility, value);
    }

    protected void ShowMessage(string text)
    {
        MessageText = text;
        MessageVisibility = Visibility.Visible;
    }

    protected void ClearMessage()
    {
        MessageText = string.Empty;
        MessageVisibility = Visibility.Collapsed;
    }
}
