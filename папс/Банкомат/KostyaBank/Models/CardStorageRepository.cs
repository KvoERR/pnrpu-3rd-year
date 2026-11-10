using System.Collections.Generic;
using System.IO;
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

    public CardStorageRepository()
    {
        _cards = new List<Card>();
        _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "card_storage.xlsx");
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
        if (card == null || !card.IsValid)
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
        if (!File.Exists(_filePath))
            return;

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage(new FileInfo(_filePath));
        var worksheet = package.Workbook.Worksheets[0];

        if (worksheet == null) return;

        int row = 2;
        while (true)
        {
            var cardNumber = worksheet.Cells[row, 1].Value?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(cardNumber)) break;

            var holderName = worksheet.Cells[row, 2].Value?.ToString() ?? string.Empty;
            var balanceStr = worksheet.Cells[row, 3].Value?.ToString();
            var pinCode = worksheet.Cells[row, 4].Value?.ToString() ?? string.Empty;

            _cards.Add(new Card
            {
                CardNumber = cardNumber,
                HolderName = holderName,
                Balance = decimal.TryParse(balanceStr, out var bal) ? bal : 0,
                PinCode = pinCode
            });

            row++;
        }
    }

    private void SaveToExcel()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage(new FileInfo(_filePath));
        var worksheet = package.Workbook.Worksheets[0];

        int maxRows = Math.Max(_cards.Count + 1, 2);
        for (int i = 2; i <= maxRows; i++)
        {
            worksheet.Cells[i, 1].Value = null;
            worksheet.Cells[i, 2].Value = null;
            worksheet.Cells[i, 3].Value = null;
            worksheet.Cells[i, 4].Value = null;
        }

        for (int i = 0; i < _cards.Count; i++)
        {
            var card = _cards[i];
            worksheet.Cells[i + 1, 1].Value = card.CardNumber;
            worksheet.Cells[i + 1, 2].Value = card.HolderName;
            worksheet.Cells[i + 1, 3].Value = card.Balance;
            worksheet.Cells[i + 1, 4].Value = card.PinCode;
        }

        package.Save();
    }
}
