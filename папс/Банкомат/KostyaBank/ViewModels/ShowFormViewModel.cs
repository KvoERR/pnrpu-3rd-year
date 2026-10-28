using KostyaBankApp.Models;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// ViewModel for the balance display form.
/// </summary>
public class ShowFormViewModel : CardFormViewModel
{
    public ShowFormViewModel(Card card, AppViewModel appViewModel)
        : base(card, appViewModel)
    {
    }

    public override string ActionButtonText => "Обновить баланс";

    public override bool CanExecuteAction => true;

    protected override void ExecuteAction()
    {
        ShowMessage($"Баланс: {Balance:C} — актуально на текущий момент.");
    }
}
