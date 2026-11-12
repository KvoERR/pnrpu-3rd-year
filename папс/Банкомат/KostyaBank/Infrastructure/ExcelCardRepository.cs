using System.Linq;
using OfficeOpenXml;

namespace KostyaBankApp.Models;

/// <summary>
/// Репозиторий для поиска карт в основной базе (Excel).
/// </summary>
public class ExcelCardRepository : ICardRepository
{
    private readonly string _filePath;

    public ExcelCardRepository(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    public Card? GetCardByNumber(string cardNumber)
    {
        var allCards = LoadFromExcel();
        return allCards.FirstOrDefault(c => c.CardNumber == cardNumber);
    }

    private List<Card> LoadFromExcel()
    {
        var cards = new List<Card>();

        if (!System.IO.File.Exists(_filePath))
            return cards;

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage(new System.IO.FileInfo(_filePath));
        var worksheet = package.Workbook.Worksheets[0];

        if (worksheet == null) return cards;

        int row = 2;
        while (true)
        {
            var cardNumber = ExcelHelper.GetCellString(worksheet, row, 1);
            if (string.IsNullOrEmpty(cardNumber)) break;

            var holderName = ExcelHelper.GetCellString(worksheet, row, 2) ?? string.Empty;
            var balance = ExcelHelper.GetCellDecimal(worksheet, row, 3);
            var pinCode = ExcelHelper.GetCellString(worksheet, row, 4) ?? string.Empty;

            cards.Add(new Card
            {
                CardNumber = cardNumber,
                HolderName = holderName,
                Balance = balance,
                PinCode = pinCode
            });

            row++;
        }

        return cards;
    }
}
