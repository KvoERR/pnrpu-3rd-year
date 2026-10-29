using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace KostyaBankApp.Models;

/// <summary>
/// Читает данные карт из Excel-файла с помощью EPPlus.
/// Формат Excel: Номер карты | Имя владельца | Баланс | PIN-код
/// </summary>
public class Computer
{
    private readonly string _filePath;

    public Computer()
    {
        var exeDir = AppDomain.CurrentDomain.BaseDirectory;
        _filePath = Path.Combine(exeDir, "comp.xlsx");
    }

    public Card? GetCardByNumber(string cardNumber)
    {
        var allCards = LoadFromExcel();
        return allCards.FirstOrDefault(c => c.CardNumber == cardNumber);
    }

    private List<Card> LoadFromExcel()
    {
        var cards = new List<Card>();

        if (!File.Exists(_filePath))
            return cards;

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage(new FileInfo(_filePath));
        var worksheet = package.Workbook.Worksheets[0];

        if (worksheet == null) return cards;

        int row = 2;
        while (true)
        {
            var cardNumber = worksheet.Cells[row, 1].Value?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(cardNumber)) break;

            var holderName = worksheet.Cells[row, 2].Value?.ToString() ?? string.Empty;
            var balanceStr = worksheet.Cells[row, 3].Value?.ToString();
            var pinCode = worksheet.Cells[row, 4].Value?.ToString() ?? string.Empty;

            cards.Add(new Card
            {
                CardNumber = cardNumber,
                HolderName = holderName,
                Balance = decimal.TryParse(balanceStr, out var bal) ? bal : 0,
                PinCode = pinCode
            });

            row++;
        }

        return cards;
    }
}
