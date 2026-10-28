using System.Collections.Generic;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace KostyaBankApp.Models;

/// <summary>
/// Читает данные карт из Excel-файла или использует встроенную базу.
/// </summary>
public class Computer
{
    private readonly string _filePath = "comp.xlsx";

    public Card? GetCardByNumber(string cardNumber)
    {
        var allCards = LoadFromExcel();
        return allCards.FirstOrDefault(c => c.CardNumber == cardNumber);
    }

    private List<Card> LoadFromExcel()
    {
        var cards = new List<Card>();
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
                    cards.Add(new Card
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
