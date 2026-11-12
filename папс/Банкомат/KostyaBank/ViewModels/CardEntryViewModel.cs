using System.Windows.Input;
using KostyaBankApp.Models;
using KostyaBankApp.Services;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// ViewModel для ввода карты и PIN-кода.
/// Зависит от ICardAuthService (SRP).
/// </summary>
public class CardEntryViewModel : BaseViewModel
{
    private readonly ICardAuthService _cardAuthService;
    private readonly INavigator _navigation;
    private string _cardNumber = string.Empty;
    private string _pinCode = string.Empty;
    private string _statusText = string.Empty;
    private bool _canLogin;
    private string _errorText = string.Empty;
    private bool _showError;
    private ICommand _loginCommand;

    public CardEntryViewModel(ICardAuthService cardAuthService, INavigator navigation)
    {
        _cardAuthService = cardAuthService ?? throw new ArgumentNullException(nameof(cardAuthService));
        _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
        _loginCommand = new SimpleCommand(ExecuteLogin, () => _canLogin);
        UpdateStatus();
    }

    public string CardNumber
    {
        get => _cardNumber;
        set
        {
            if (_cardNumber == value) return;

            if (!string.IsNullOrEmpty(value) && (string.IsNullOrEmpty(_cardNumber) || _cardNumber.Length != value.Length || _cardNumber != value))
            {
                ShowError = false;
                ErrorText = string.Empty;
            }

            _cardNumber = value;
            OnPropertyChanged();

            if (!string.IsNullOrEmpty(value) && value.Length == 16)
            {
                if (_cardAuthService.IsCardBlocked(value))
                {
                    ShowError = true;
                    CanLogin = false;
                    ErrorText = "Карта заблокирована и помещена в хранилище";
                    StatusText = "🔒 Карта заблокирована";
                    PinCode = string.Empty;
                    return;
                }
            }

            UpdateStatus();
        }
    }

    public string PinCode
    {
        get => _pinCode;
        set
        {
            if (_pinCode == value) return;
            _pinCode = value;
            OnPropertyChanged();
            UpdateStatus();
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public string ErrorText
    {
        get => _errorText;
        private set => SetProperty(ref _errorText, value);
    }

    public bool ShowError
    {
        get => _showError;
        private set => SetProperty(ref _showError, value);
    }

    public bool CanLogin
    {
        get => _canLogin;
        private set
        {
            if (SetProperty(ref _canLogin, value))
                (_loginCommand as SimpleCommand)?.RaiseCanExecuteChanged();
        }
    }

    public ICommand LoginCommand => _loginCommand;

    private void ExecuteLogin()
    {
        var result = _cardAuthService.Authenticate(CardNumber, PinCode);

        ErrorText = result.ErrorMessage ?? string.Empty;
        StatusText = result.StatusText ?? string.Empty;
        ShowError = !result.Success;
        CanLogin = result.Success;

        if (result.Success && result.Card != null)
        {
            _navigation.Navigate<MainMenuViewModel>(result.Card);
        }
    }

    private void UpdateStatus()
    {
        ShowError = false;

        if (string.IsNullOrEmpty(CardNumber) || string.IsNullOrEmpty(PinCode))
        {
            StatusText = $"Карта: {CardNumber.Length}/16  |  PIN: {PinCode.Length}/4";
            ErrorText = string.Empty;
            CanLogin = false;
        }
        else if (CardNumber.Length == 16 && CardNumber.All(char.IsDigit) &&
                 PinCode.Length == 4 && PinCode.All(char.IsDigit))
        {
            StatusText = "✓ Готово";
            ErrorText = string.Empty;
            CanLogin = true;
        }
        else
        {
            StatusText = $"Карта: {CardNumber.Length}/16  |  PIN: {PinCode.Length}/4";
            ErrorText = string.Empty;
            CanLogin = false;
        }
    }
}
