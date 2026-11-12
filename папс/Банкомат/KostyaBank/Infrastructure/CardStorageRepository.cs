using System.Linq;
using OfficeOpenXml;

namespace KostyaBankApp.Models;

/// <summary>
/// Репозиторий для хранения заблокированных карт — отвечает только за I/O (Excel).
/// </summary>
public class CardStorageRepository : ICardStorageRepository
{
    private readonly List<Card> _cards;
    private readonly string _filePath;

    public CardStorageRepository(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _cards = new List<Card>();
        LoadFromExcel();
    }

    public int Count => _cards.Count;
    public IEnumerable<Card> Cards => _cards.AsReadOnly();

    public Card? GetCardByNumber(string cardNumber)
    {
        return _cards.FirstOrDefault(c => c.CardNumber == cardNumber);
    }

    public bool AddCard(Card card)
    {
        if (card == null)
            return false;

        if (string.IsNullOrWhiteSpace(card.CardNumber))
            return false;

        if (_cards.Any(c => c.CardNumber == card.CardNumber))
            return false;

        _cards.Add(card);
        SaveToExcel();
        return true;
    }

    public bool RemoveCard(string cardNumber)
    {
        var removed = _cards.RemoveAll(c => c.CardNumber == cardNumber) > 0;
        if (removed)
            SaveToExcel();
        return removed;
    }

    public void Clear()
    {
        _cards.Clear();
        SaveToExcel();
    }

    private void LoadFromExcel()
    {
        if (!System.IO.File.Exists(_filePath))
            return;

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage(new System.IO.FileInfo(_filePath));
        var worksheet = package.Workbook.Worksheets[0];

        if (worksheet == null) return;

        int row = 2;
        while (true)
        {
            var cardNumber = ExcelHelper.GetCellString(worksheet, row, 1);
            if (string.IsNullOrEmpty(cardNumber)) break;

            var holderName = ExcelHelper.GetCellString(worksheet, row, 2) ?? string.Empty;
            var balance = ExcelHelper.GetCellDecimal(worksheet, row, 3);
            var pinCode = ExcelHelper.GetCellString(worksheet, row, 4) ?? string.Empty;

            _cards.Add(new Card
            {
                CardNumber = cardNumber,
                HolderName = holderName,
                Balance = balance,
                PinCode = pinCode
            });

            row++;
        }
    }

    private void SaveToExcel()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage(new System.IO.FileInfo(_filePath));
        var worksheet = package.Workbook.Worksheets[0];

        int maxRows = Math.Max(_cards.Count + 1, 2);
        for (int i = 2; i <= maxRows; i++)
        {
            worksheet.ClearCell(i, 1);
            worksheet.ClearCell(i, 2);
            worksheet.ClearCell(i, 3);
            worksheet.ClearCell(i, 4);
        }

        for (int i = 0; i < _cards.Count; i++)
        {
            var card = _cards[i];
            worksheet.SetCell(i + 1, 1, card.CardNumber);
            worksheet.SetCell(i + 1, 2, card.HolderName);
            worksheet.SetCell(i + 1, 3, card.Balance);
            worksheet.SetCell(i + 1, 4, card.PinCode);
        }

        package.Save();
    }
}
