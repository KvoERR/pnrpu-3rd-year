using KostyaBankApp.Models;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// ViewModel для экрана сервисных операций.
/// </summary>
public class ServiceViewModel : BaseViewModel
{
    private readonly CardRecord _card;
    private string _statusText = "Сервисные операции";

    public ServiceViewModel(CardRecord card)
    {
        _card = card ?? throw new ArgumentNullException(nameof(card));
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public string CardInfo => $"Клиент: {_card.HolderName} | Карта: **** **** **** {_card.CardNumber[^4..]}";
    public decimal Balance => _card.Balance;
}
