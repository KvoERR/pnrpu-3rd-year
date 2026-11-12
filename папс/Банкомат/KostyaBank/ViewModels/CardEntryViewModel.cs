using System.Windows.Input;
using KostyaBankApp.Models;
using KostyaBankApp.Services;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// ViewModel для ввода карты и PIN-кода.
/// Зависит от INavigationService (DIP).
/// </summary>
public class CardEntryViewModel : BaseViewModel
{
    private readonly ICardRepository _cardRepository;
    private readonly ICardStorageRepository _cardStorageRepository;
    private readonly INavigator _navigation;
    private string _cardNumber = string.Empty;
    private string _pinCode = string.Empty;
    private string _statusText = string.Empty;
    private bool _canLogin;
    private string _errorText = string.Empty;
    private bool _showError;
    private int _failedAttempts;
    private const int MaxAttempts = 3;
    private ICommand _loginCommand;

    public CardEntryViewModel(ICardRepository cardRepository, ICardStorageRepository cardStorageRepository, INavigator navigation)
    {
        _cardRepository = cardRepository ?? throw new ArgumentNullException(nameof(cardRepository));
        _cardStorageRepository = cardStorageRepository ?? throw new ArgumentNullException(nameof(cardStorageRepository));
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
                var blockedCard = _cardStorageRepository.GetCardByNumber(value);
                if (blockedCard != null)
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
        if (CardNumber.Length == 16 && _cardStorageRepository.GetCardByNumber(CardNumber) != null)
        {
            ShowError = true;
            ErrorText = "Карта заблокирована и помещена в хранилище";
            StatusText = "🔒 Карта заблокирована";
            return;
        }

        if (CardNumber.Length != 16 || !CardNumber.All(char.IsDigit))
        {
            ShowError = true;
            CanLogin = false;
            ErrorText = "Номер карты должен содержать 16 цифр";
            StatusText = "✗ Неверный формат карты";
            return;
        }

        if (string.IsNullOrEmpty(PinCode) || PinCode.Length != 4 || !PinCode.All(char.IsDigit))
        {
            ShowError = true;
            CanLogin = false;
            ErrorText = "PIN-код должен содержать 4 цифры";
            StatusText = "✗ Неверный формат PIN";
            return;
        }

        var card = _cardRepository.GetCardByNumber(CardNumber);

        if (card == null)
        {
            ShowError = true;
            CanLogin = false;
            ErrorText = "Карта не найдена в базе";
            StatusText = "✗ Не найдена";
            return;
        }

        if (card.PinCode != PinCode)
        {
            _failedAttempts++;
            int remaining = MaxAttempts - _failedAttempts;

            if (_failedAttempts >= MaxAttempts)
            {
                _cardStorageRepository.AddCard(card);
                ShowError = true;
                CanLogin = false;
                ErrorText = $"Карта заблокирована после {MaxAttempts} неверных попыток. Карта помещена в хранилище.";
                StatusText = "🔒 Карта заблокирована";
                _failedAttempts = 0;
            }
            else
            {
                ShowError = true;
                CanLogin = false;
                ErrorText = $"Неверный PIN-код. Осталось попыток: {remaining}";
                StatusText = "✗ Неверный PIN";
            }
            return;
        }

        _failedAttempts = 0;
        _navigation.Navigate<MainMenuViewModel>(card);
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
