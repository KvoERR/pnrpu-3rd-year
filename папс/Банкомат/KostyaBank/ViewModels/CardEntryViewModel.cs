using System.Windows.Input;
using KostyaBankApp.Models;
using KostyaBankApp.Services;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// ViewModel for the card entry window.
/// </summary>
public class CardEntryViewModel : BaseViewModel
{
    private readonly Computer _repository;
    private string _cardNumber = string.Empty;
    private string _statusText = string.Empty;
    private bool _canLogin;
    private string _errorText = string.Empty;
    private bool _showError;
    private readonly ICommand _loginCommand;

    public CardEntryViewModel()
    {
        _repository = new Computer("cards.xlsx");
        _loginCommand = new SimpleCommand(ExecuteLogin, () => _canLogin);
        UpdateStatus();
    }

    public string CardNumber
    {
        get => _cardNumber;
        set
        {
            if (_cardNumber == value) return;
            _cardNumber = value;
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
            {
                (_loginCommand as SimpleCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand LoginCommand => _loginCommand;

    private void ExecuteLogin()
    {
        if (CardNumber.Length != 16 || !CardNumber.All(char.IsDigit))
        {
            ShowError = true;
            CanLogin = false;
            ErrorText = "Номер карты должен содержать 16 цифр";
            StatusText = "✗ Неверный формат";
            return;
        }

        var cardRecord = _repository.GetCardByNumber(CardNumber);

        if (cardRecord != null)
        {
            try
            {
                var navigationService = new NavigationService(cardRecord);
                navigationService.ShowMainMenu();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка: {ex.Message}");
            }
            return;
        }

        ShowError = true;
        CanLogin = false;
        ErrorText = "Карта не найдена в базе";
        StatusText = "✗ Не найдена";
    }

    private void UpdateStatus()
    {
        ShowError = false;

        if (string.IsNullOrEmpty(CardNumber))
        {
            StatusText = "0 / 16 цифр";
            ErrorText = string.Empty;
            CanLogin = false;
        }
        else if (CardNumber.Length == 16 && CardNumber.All(char.IsDigit))
        {
            StatusText = "✓ Готово";
            ErrorText = string.Empty;
            CanLogin = true;
        }
        else
        {
            StatusText = $"{CardNumber.Length} / 16 цифр";
            ErrorText = string.Empty;
            CanLogin = false;
        }
    }
}
