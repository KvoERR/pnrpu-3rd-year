using System.Linq;
using OfficeOpenXml;

namespace KostyaBankApp.Models;

/// <summary>
/// Репозиторий для хранения наличных — отвечает только за I/O (Excel).
/// </summary>
public class MoneyStorageRepository : IMoneyStorageRepository
{
    private readonly IMoneyStorageData _storage;
    private readonly INominalProvider _nominalProvider;
    private readonly string _filePath;

    public MoneyStorageRepository(IMoneyStorageData storage, INominalProvider nominalProvider, string filePath)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _nominalProvider = nominalProvider ?? throw new ArgumentNullException(nameof(nominalProvider));
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        LoadFromExcel();
    }

    public int SumMoney() => _storage.Sum;
    public int GetCountByNominal(int nominal) => _storage.GetCount(nominal);
    public void TakeMoney(int amount)
    {
        if (amount > SumMoney())
            throw new InvalidOperationException("Недостаточно средств в хранилище");

        int remaining = amount;
        var billsToTake = new List<(int nominal, int count)>();

        foreach (var (nominal, _) in _storage.GetAll().OrderByDescending(x => x.Nominal))
        {
            if (remaining <= 0) break;

            int available = _storage.GetCount(nominal);
            int canTake = Math.Min(remaining / nominal, available);

            if (canTake > 0)
            {
                billsToTake.Add((nominal, canTake));
                remaining -= nominal * canTake;
            }
        }

        if (remaining > 0)
            throw new InvalidOperationException("Невозможно выдать указанную сумму доступными номиналами");

        foreach (var (nominal, count) in billsToTake)
            _storage.Remove(nominal, count);

        SaveToExcel();
    }

    public void AddMoney(int nominal, int count)
    {
        _storage.Add(nominal, count);
        SaveToExcel();
    }

    public bool RemoveMoney(int nominal, int count)
    {
        var result = _storage.Remove(nominal, count);
        if (result) SaveToExcel();
        return result;
    }

    public int ClearStorage()
    {
        int total = _storage.Sum;
        _storage.Clear();
        SaveToExcel();
        return total;
    }

    public void Reload() => LoadFromExcel();
    public void ReloadFromExcel() => Reload();

    private void LoadFromExcel()
    {
        if (!System.IO.File.Exists(_filePath))
            return;

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage(new System.IO.FileInfo(_filePath));
        var worksheet = package.Workbook.Worksheets[0];

        if (worksheet == null) return;

        _storage.Clear();

        int row = 2;
        while (true)
        {
            var nominalStr = ExcelHelper.GetCellString(worksheet, row, 1);
            if (string.IsNullOrEmpty(nominalStr)) break;

            var countStr = ExcelHelper.GetCellString(worksheet, row, 2);

            if (int.TryParse(nominalStr, out var nominal) && int.TryParse(countStr, out var count))
            {
                if (count > 0)
                    _storage.Add(nominal, count);
            }

            row++;
        }
    }

    private void SaveToExcel()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage(new System.IO.FileInfo(_filePath));
        var worksheet = package.Workbook.Worksheets[0];

        int maxRows = Math.Max(_storage.GetAll().Count() + 1, 7);
        for (int i = 2; i <= maxRows; i++)
        {
            worksheet.ClearCell(i, 1);
            worksheet.ClearCell(i, 2);
        }

        var allNominals = _nominalProvider.Nominals;
        int row = 2;

        foreach (var nominal in allNominals)
        {
            worksheet.SetCell(row, 1, nominal);
            worksheet.SetCell(row, 2, _storage.GetCount(nominal));
            row++;
        }

        package.Save();
    }
}
