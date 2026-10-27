using System.Collections.Generic;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace KostyaBankApp.Models;

/// <summary>
/// Данные карты из Excel-файла.
/// </summary>
public class CardRecord
{
    public string CardNumber { get; set; } = string.Empty;
    public string HolderName { get; set; } = string.Empty;
    public decimal Balance { get; set; }

    public string MaskedNumber
    {
        get
        {
            if (CardNumber.Length < 4) return "****";
            var last4 = CardNumber.Substring(CardNumber.Length - 4);
            return $"**** **** **** {last4}";
        }
    }
}

/// <summary>
/// Читает данные карт из Excel-файла или использует встроенную базу.
/// </summary>
public class CardRepository
{
    private readonly string _filePath;
    private readonly List<CardRecord> _fallbackCards = new()
    {
        new CardRecord { CardNumber = "4276590123456789", HolderName = "Иван Иванов", Balance = 50000 },
        new CardRecord { CardNumber = "4276590123456790", HolderName = "Мария Петрова", Balance = 120000 }
    };

    public CardRepository(string filePath)
    {
        _filePath = filePath;
    }

    public CardRecord? GetCardByNumber(string cardNumber)
    {
        var allCards = LoadAllCards();
        return allCards.FirstOrDefault(c => c.CardNumber == cardNumber);
    }

    private List<CardRecord> LoadAllCards()
    {
        try
        {
            return LoadFromExcel();
        }
        catch
        {
            return _fallbackCards;
        }
    }

    private List<CardRecord> LoadFromExcel()
    {
        var cards = new List<CardRecord>();
        var excelApp = new Excel.Application();
        Excel.Workbook workbook = null;

        try
        {
            workbook = excelApp.Workbooks.Open(_filePath);
            Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Sheets[1];

            int row = 2;
            while (true)
            {
                Excel.Range cell = (Excel.Range)worksheet.Cells[row, 1];
                if (cell.Value2 == null) break;

                var cardNumber = cell.Value2.ToString() ?? string.Empty;
                Excel.Range holderCell = (Excel.Range)worksheet.Cells[row, 2];
                Excel.Range balanceCell = (Excel.Range)worksheet.Cells[row, 3];
                var holderName = holderCell.Value2?.ToString() ?? string.Empty;
                var balance = balanceCell.Value2?.ToString();

                if (!string.IsNullOrEmpty(cardNumber))
                {
                    cards.Add(new CardRecord
                    {
                        CardNumber = cardNumber,
                        HolderName = holderName,
                        Balance = decimal.TryParse(balance, out var bal) ? bal : 0
                    });
                }

                row++;
            }
        }
        finally
        {
            workbook?.Close(false);
            excelApp?.Quit();
        }

        return cards;
    }
}
