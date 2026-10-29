using System.Windows.Input;
using KostyaBankApp.Models;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// ViewModel для экрана сервисных операций.
/// </summary>
public class ServiceViewModel : BaseViewModel
{
    private readonly AppViewModel _appViewModel;
    private readonly Card _card;
    private string _statusText = "Сервисные операции";
    private readonly ICommand _backCommand;
    private readonly ICommand _moneyStorageCommand;
    private readonly ICommand _cardStorageCommand;

    public ServiceViewModel(AppViewModel appViewModel)
    {
        _appViewModel = appViewModel;
        _card = null!;
        _backCommand = new SimpleCommand(() => _appViewModel.ShowCardEntry());
        _moneyStorageCommand = new SimpleCommand(() => _appViewModel.ShowMoneyStorage());
        _cardStorageCommand = new SimpleCommand(() => _appViewModel.ShowCardStorage());
    }

    public ServiceViewModel(Card card, AppViewModel appViewModel)
    {
        _card = card ?? throw new ArgumentNullException(nameof(card));
        _appViewModel = appViewModel;
        _backCommand = new SimpleCommand(() => _appViewModel.ShowMainMenu(card));
        _moneyStorageCommand = new SimpleCommand(() => _appViewModel.ShowMoneyStorage());
        _cardStorageCommand = new SimpleCommand(() => _appViewModel.ShowCardStorage());
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public ICommand BackCommand => _backCommand;
    public ICommand MoneyStorageCommand => _moneyStorageCommand;
    public ICommand CardStorageCommand => _cardStorageCommand;
}
