using System.Windows.Input;
using KostyaBankApp.Models;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// ViewModel для ввода карты и PIN-кода.
/// 3 неверных PIN — карта блокируется и попадает в хранилище.
/// </summary>
public class CardEntryViewModel : BaseViewModel
{
    private readonly Computer _computer;
    private readonly CardStorage _cardStorage;
    private readonly AppViewModel _appViewModel;
    private string _cardNumber = string.Empty;
    private string _pinCode = string.Empty;
    private string _statusText = string.Empty;
    private bool _canLogin;
    private string _errorText = string.Empty;
    private bool _showError;
    private int _failedAttempts;
    private const int MaxAttempts = 3;
    private readonly ICommand _loginCommand;

    public CardEntryViewModel(AppViewModel appViewModel)
    {
        _computer = new Computer();
        _cardStorage = new CardStorage();
        _appViewModel = appViewModel;
        _loginCommand = new SimpleCommand(ExecuteLogin, () => _canLogin);
        UpdateStatus();
    }

    public string CardNumber
    {
        get => _cardNumber;
        set
        {
            if (_cardNumber == value) return;
            
            // Сбрасываем состояние блокировки при вводе нового номера
            if (!string.IsNullOrEmpty(value) && (string.IsNullOrEmpty(_cardNumber) || _cardNumber.Length != value.Length || _cardNumber != value))
            {
                ShowError = false;
                ErrorText = string.Empty;
            }
            
            _cardNumber = value;
            OnPropertyChanged();

            // Если карта заблокирована — блокируем ввод
            if (!string.IsNullOrEmpty(value) && value.Length == 16)
            {
                var blockedCard = _cardStorage.GetCardByNumber(value);
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
        // Проверка на заблокированную карту
        if (CardNumber.Length == 16 && _cardStorage.GetCardByNumber(CardNumber) != null)
        {
            ShowError = true;
            ErrorText = "Карта заблокирована и помещена в хранилище";
            StatusText = "🔒 Карта заблокирована";
            return;
        }

        // Проверка номера карты
        if (CardNumber.Length != 16 || !CardNumber.All(char.IsDigit))
        {
            ShowError = true;
            CanLogin = false;
            ErrorText = "Номер карты должен содержать 16 цифр";
            StatusText = "✗ Неверный формат карты";
            return;
        }

        // Проверка PIN
        if (string.IsNullOrEmpty(PinCode) || PinCode.Length != 4 || !PinCode.All(char.IsDigit))
        {
            ShowError = true;
            CanLogin = false;
            ErrorText = "PIN-код должен содержать 4 цифры";
            StatusText = "✗ Неверный формат PIN";
            return;
        }

        var card = _computer.GetCardByNumber(CardNumber);

        if (card == null)
        {
            ShowError = true;
            CanLogin = false;
            ErrorText = "Карта не найдена в базе";
            StatusText = "✗ Не найдена";
            return;
        }

        // Проверка PIN-кода
        if (card.PinCode != PinCode)
        {
            _failedAttempts++;
            int remaining = MaxAttempts - _failedAttempts;

            if (_failedAttempts >= MaxAttempts)
            {
                // Карта заблокирована — помещаем в хранилище
                _cardStorage.AddCard(card);
                ShowError = true;
                CanLogin = false;
                ErrorText = $"Карта заблокирована после {MaxAttempts} неверных попыток. Карта помещена в хранилище.";
                StatusText = "🔒 Карта заблокирована";
                _failedAttempts = 0;
                // Не очищаем поля — пользователь должен увидеть сообщение
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

        // Успешный вход
        _failedAttempts = 0;
        _appViewModel.ShowMainMenu(card);
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
