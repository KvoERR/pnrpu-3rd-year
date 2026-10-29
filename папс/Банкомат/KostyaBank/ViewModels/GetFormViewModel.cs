using System.Globalization;
using System.IO;
using System.Windows.Input;
using KostyaBankApp.Models;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// ViewModel for the cash withdrawal form.
/// </summary>
public class GetFormViewModel : CardFormViewModel
{
    private readonly ICommand _withdrawCommand;
    private readonly Printer _printer;
    private string _withdrawAmount = string.Empty;

    public GetFormViewModel(Card card, AppViewModel appViewModel, Printer? printer = null)
        : base(card, appViewModel)
    {
        _printer = printer ?? new Printer();
        _withdrawCommand = new SimpleCommand(() => Withdraw(), () => CanWithdraw);
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
                ((SimpleCommand)_withdrawCommand).RaiseCanExecuteChanged();
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

    public ICommand WithdrawCommand => _withdrawCommand;

    protected override void ExecuteAction()
    {
        if (!decimal.TryParse(WithdrawAmount, out var amount))
            return;
        ShowMessage($"Заявка на снятие {amount:C} принята. Ожидайте выдачи купюр.");
    }

    private void Withdraw()
    {
        if (!decimal.TryParse(WithdrawAmount, out var amount))
            return;

        System.Windows.MessageBox.Show(
            $"Снятие {amount:C}\n\nЭта функция в разработке.",
            "Снятие наличных",
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);
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
