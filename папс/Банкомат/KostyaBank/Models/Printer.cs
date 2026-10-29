using System.Globalization;
using System.Text;
using System.Windows;

namespace KostyaBankApp.Models;

/// <summary>
/// Принтер для формирования и отображения чека/справки.
/// </summary>
public class Printer
{
    /// <summary>
    /// Формирует чек операции снятия наличных.
    /// </summary>
    public void ShowWithdrawDoc(decimal amount, Card card, DateTime transactionDate)
    {
        var sb = new StringBuilder();

        sb.AppendLine("========================================");
        sb.AppendLine("   KostyaBank - Снятие наличных");
        sb.AppendLine("========================================");
        sb.AppendLine();
        sb.AppendLine($"Дата и время:  {transactionDate:dd.MM.yyyy HH:mm:ss}");
        sb.AppendLine($"Номер карты:   {card.CardNumber}");
        sb.AppendLine($"Владелец:      {card.HolderName}");
        sb.AppendLine();
        sb.AppendLine("----------------------------------------");
        sb.AppendLine($"Операция:      Снятие наличных");
        sb.AppendLine($"Сумма:         {amount:C2}");
        sb.AppendLine("----------------------------------------");
        sb.AppendLine();
        sb.AppendLine($"Баланс после:  {card.Balance:C2}");
        sb.AppendLine();
        sb.AppendLine("========================================");
        sb.AppendLine("  Спасибо за использование KostyaBank!");
        sb.AppendLine("========================================");

        MessageBox.Show(
            sb.ToString(),
            "Чек: Снятие наличных",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    /// <summary>
    /// Формирует справку о балансе карты.
    /// </summary>
    public void ShowBalanceDoc(Card card, DateTime transactionDate)
    {
        var sb = new StringBuilder();

        sb.AppendLine("========================================");
        sb.AppendLine("   KostyaBank - Справка о балансе");
        sb.AppendLine("========================================");
        sb.AppendLine();
        sb.AppendLine($"Дата и время:  {transactionDate:dd.MM.yyyy HH:mm:ss}");
        sb.AppendLine($"Номер карты:   {card.CardNumber}");
        sb.AppendLine($"Владелец:      {card.HolderName}");
        sb.AppendLine();
        sb.AppendLine("----------------------------------------");
        sb.AppendLine($"Операция:      Запрос баланса");
        sb.AppendLine($"Баланс:        {card.Balance:C2}");
        sb.AppendLine("----------------------------------------");
        sb.AppendLine();
        sb.AppendLine("Справка сформирована автоматически.");
        sb.AppendLine();
        sb.AppendLine("========================================");
        sb.AppendLine("  Спасибо за использование KostyaBank!");
        sb.AppendLine("========================================");

        MessageBox.Show(
            sb.ToString(),
            "Справка: Баланс карты",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
