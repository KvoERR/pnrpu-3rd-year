using System.Globalization;
using System.IO;
using System.Text;
using System;

namespace KostyaBankApp.Models;

/// <summary>
/// Принтер для формирования и сохранения чеков/справок.
/// Реализует IReceiptPrinter.
/// </summary>
public class ExcelPrinter : IReceiptPrinter
{
    private readonly string _outputPath;

    public ExcelPrinter(string outputPath)
    {
        _outputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
    }

    public string SaveWithdrawDoc(decimal amount, Card card, DateTime transactionDate)
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

        Directory.CreateDirectory(_outputPath);
        var fileName = $"withdraw_{card.CardNumber}_{transactionDate:yyyyMMdd_HHmmss}.txt";
        var filePath = Path.Combine(_outputPath, fileName);
        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

        return filePath;
    }

    public string SaveBalanceDoc(Card card, DateTime transactionDate)
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

        Directory.CreateDirectory(_outputPath);
        var fileName = $"balance_{card.CardNumber}_{transactionDate:yyyyMMdd_HHmmss}.txt";
        var filePath = Path.Combine(_outputPath, fileName);
        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

        return filePath;
    }
}
