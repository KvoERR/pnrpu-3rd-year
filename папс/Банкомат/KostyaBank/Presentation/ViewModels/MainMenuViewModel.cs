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
    private readonly INavigator _navigation;
    private readonly ICommand _showCommand;
    private readonly ICommand _getCommand;
    private readonly ICommand _exitCommand;

    public MainMenuViewModel(Card card, INavigator navigation)
    {
        _card = card ?? throw new ArgumentNullException(nameof(card));
        _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
        
        _showCommand = new SimpleCommand(() => _navigation.Navigate<ShowFormViewModel>(card));
        _getCommand = new SimpleCommand(() => _navigation.Navigate<GetFormViewModel>(card));
        _exitCommand = new SimpleCommand(() => _navigation.Navigate<CardEntryViewModel>());
    }

    public string CardNumber => _card.CardNumber;
    public string MaskedCardNumber => $"**** **** **** {CardNumber.Substring(12)}";
    public string HolderName => _card.HolderName;
    public decimal Balance => _card.Balance;

    public ICommand ShowCommand => _showCommand;
    public ICommand GetCommand => _getCommand;
    public ICommand ExitCommand => _exitCommand;
}
