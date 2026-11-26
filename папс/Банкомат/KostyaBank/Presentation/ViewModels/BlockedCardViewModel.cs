using System.ComponentModel;
using KostyaBankApp.Models;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// Представление заблокированной карты для UI.
/// Изолирует ViewModel от доменной модели — мутации здесь не затрагивают репозиторий.
/// </summary>
public class BlockedCardViewModel : BaseViewModel
{
    private string _cardNumber;
    private string _holderName;
    private decimal _balance;
    private string _pinCode;

    public BlockedCardViewModel(Card card)
    {
        _cardNumber = card.CardNumber;
        _holderName = card.HolderName;
        _balance = card.Balance;
        _pinCode = card.PinCode;
    }

    public string CardNumber
    {
        get => _cardNumber;
        set => SetProperty(ref _cardNumber, value);
    }

    public string HolderName
    {
        get => _holderName;
        set => SetProperty(ref _holderName, value);
    }

    public decimal Balance
    {
        get => _balance;
        set => SetProperty(ref _balance, value);
    }

    public string PinCode
    {
        get => _pinCode;
        set => SetProperty(ref _pinCode, value);
    }

    public string MaskedCardNumber => $"**** **** **** {CardNumber.Substring(12)}";
}
