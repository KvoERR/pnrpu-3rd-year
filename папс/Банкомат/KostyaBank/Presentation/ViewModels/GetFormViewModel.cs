using System.Globalization;
using System.IO;
using System.Windows.Input;
using KostyaBankApp.Models;
using KostyaBankApp.Services;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// ViewModel for the cash withdrawal form.
/// </summary>
public class GetFormViewModel : CardFormViewModel
{
    private readonly IReceiptPrinter _printer;
    private readonly IMoneyStorageRepository _moneyStorage;
    private string _withdrawAmount = string.Empty;
    private ICommand _actionCommand;

    public GetFormViewModel(Card card, INavigator navigation, IReceiptPrinter printer, IMoneyStorageRepository moneyStorage)
        : base(card, navigation)
    {
        _printer = printer ?? throw new ArgumentNullException(nameof(printer));
        _moneyStorage = moneyStorage ?? throw new ArgumentNullException(nameof(moneyStorage));
        _actionCommand = base.ActionCommand;
    }

    public override string ActionButtonText => "Снять наличные";

    public override bool CanExecuteAction => CanWithdraw;

    public string WithdrawAmount
    {
        get => _withdrawAmount;
        set
        {
            if (SetProperty(ref _withdrawAmount, value))
            {
                OnPropertyChanged(nameof(CanWithdraw));
                OnPropertyChanged(nameof(CanExecuteAction));
                (_actionCommand as SimpleCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanWithdraw
    {
        get
        {
            if (string.IsNullOrWhiteSpace(WithdrawAmount))
                return false;
            if (!decimal.TryParse(WithdrawAmount, out var amount))
                return false;
            return amount > 0 && amount <= Balance;
        }
    }

    protected override void ExecuteAction()
    {
        if (!decimal.TryParse(WithdrawAmount, out var amount))
            return;

        try
        {
            _moneyStorage.TakeMoney((int)amount);
            Card.Balance -= amount;

            ShowMessage($"Выдано {amount:C}. Остаток на карте: {Card.Balance:C}");
        }
        catch (InvalidOperationException ex)
        {
            ShowMessage(ex.Message);
        }
    }

    protected override void OnPrintReceipt()
    {
        if (decimal.TryParse(WithdrawAmount, out var amount))
        {
            var filePath = _printer.SaveWithdrawDoc(amount, Card, DateTime.Now);
            ShowMessage($"Чек сохранён в Output: {Path.GetFileName(filePath)}");
        }
    }
}
