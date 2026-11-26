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
    }
    public void AddMoney(int nominal, int count) => _storage.Add(nominal, count);
    public bool RemoveMoney(int nominal, int count) => _storage.Remove(nominal, count);
    public int ClearStorage()
    {
        int total = _storage.Sum;
        _storage.Clear();
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
}
