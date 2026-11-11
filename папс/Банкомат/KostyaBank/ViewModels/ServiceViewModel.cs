using System.Windows.Input;
using KostyaBankApp.Models;
using KostyaBankApp.Services;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// ViewModel для экрана сервисных операций.
/// </summary>
public class ServiceViewModel : BaseViewModel
{
    private readonly INavigationService _navigation;
    private string _statusText = "Сервисные операции";
    private readonly ICommand _backCommand;
    private readonly ICommand _moneyStorageCommand;
    private readonly ICommand _cardStorageCommand;

    public ServiceViewModel(INavigationService navigation)
    {
        _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
        _backCommand = new SimpleCommand(() => _navigation.Navigate("cardEntry"));
        _moneyStorageCommand = new SimpleCommand(() => _navigation.Navigate("moneyStorage"));
        _cardStorageCommand = new SimpleCommand(() => _navigation.Navigate("cardStorage"));
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
