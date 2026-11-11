using System.IO;
using KostyaBankApp.Models;
using KostyaBankApp.Services;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// ViewModel for the balance display form.
/// </summary>
public class ShowFormViewModel : CardFormViewModel
{
    private readonly IReceiptPrinter _printer;

    public ShowFormViewModel(Card card, INavigationService navigation)
        : base(card, navigation)
    {
        _printer = new ExcelPrinter();
    }

    public override string ActionButtonText => "Обновить баланс";

    public override bool CanExecuteAction => true;

    protected override void ExecuteAction()
    {
        ShowMessage($"Баланс: {Balance:C} — актуально на текущий момент.");
    }

    protected override void OnPrintReceipt()
    {
        var filePath = _printer.SaveBalanceDoc(Card, DateTime.Now);
        ShowMessage($"Справка сохранена в Output: {Path.GetFileName(filePath)}");
    }
}
