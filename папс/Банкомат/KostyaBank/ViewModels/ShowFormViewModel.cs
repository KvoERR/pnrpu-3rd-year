using KostyaBankApp.Models;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// ViewModel for the balance display form.
/// </summary>
public class ShowFormViewModel : CardFormViewModel
{
    private readonly Printer _printer;

    public ShowFormViewModel(Card card, AppViewModel appViewModel, Printer? printer = null)
        : base(card, appViewModel)
    {
        _printer = printer ?? new Printer();
    }

    public override string ActionButtonText => "Обновить баланс";

    public override bool CanExecuteAction => true;

    protected override void ExecuteAction()
    {
        ShowMessage($"Баланс: {Balance:C} — актуально на текущий момент.");
    }

    protected override void OnPrintReceipt()
    {
        _printer.ShowBalanceDoc(Card, DateTime.Now);
    }
}
