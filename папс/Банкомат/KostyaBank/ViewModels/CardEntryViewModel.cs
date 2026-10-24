using System.Windows.Input;
using KostyaBankApp.Models;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// ViewModel for the card entry window.
/// </summary>
public class CardEntryViewModel : BaseViewModel
{
    private readonly Card _card = new();
    private string _cardNumber = string.Empty;
    private string _statusText = string.Empty;
    private bool _canLogin;
    private string _errorText = string.Empty;
    private bool _showError;
    private readonly ICommand _loginCommand;

    public CardEntryViewModel()
    {
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
            _card.CardNumber = value;
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

    public bool CanLogin => _canLogin;

    public ICommand LoginCommand => _loginCommand;

    private void ExecuteLogin()
    {
        if (_card.IsValid)
        {
            var mainViewModel = new MainMenuViewModel(_card);
            var mainWindow = new Views.MainWindow(mainViewModel);
            mainWindow.Show();
            return;
        }

        ShowError = true;
        _canLogin = false;
        ErrorText = _card.ErrorMessage ?? "Неверный номер карты";
        StatusText = "✗ Неверный формат";
        (_loginCommand as SimpleCommand)?.RaiseCanExecuteChanged();
    }

    private void UpdateStatus()
    {
        ShowError = false;
        _canLogin = false;

        if (string.IsNullOrEmpty(CardNumber))
        {
            StatusText = "0 / 16 цифр";
            ErrorText = string.Empty;
        }
        else
        {
            StatusText = $"{CardNumber.Length} / 16 цифр";
            ErrorText = string.Empty;
        }

        (_loginCommand as SimpleCommand)?.RaiseCanExecuteChanged();
    }
}
