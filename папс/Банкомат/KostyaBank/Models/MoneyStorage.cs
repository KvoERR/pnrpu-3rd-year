using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace KostyaBankApp.Models;

/// <summary>
/// Хранилище денег банкомата (наличные в купюрах), загружаемое из Excel-файла.
/// Формат: Номинал | Количество
/// </summary>
public class MoneyStorage
{
    private readonly Dictionary<int, int> _money;
    private readonly string _filePath;

    public MoneyStorage()
    {
        _money = new Dictionary<int, int>();
        var exeDir = AppDomain.CurrentDomain.BaseDirectory;
        _filePath = Path.Combine(exeDir, "money_storage.xlsx");
        LoadFromExcel();
    }

    public List<Money> Money => _money.Keys.Select(n => new Money { Amount = n, Count = _money[n] }).ToList();

    public int SumMoney()
    {
        return _money.Sum(m => m.Key * m.Value);
    }

    public int GetCash()
    {
        int total = SumMoney();
        ClearStorage();
        return total;
    }

    public int ClearStorage()
    {
        int total = SumMoney();
        _money.Clear();
        SaveToExcel();
        return total;
    }

    public int TakeMoney(int amount)
    {
        if (amount > SumMoney())
            throw new InvalidOperationException("Недостаточно средств в хранилище");

        int remaining = amount;
        var billsToTake = new List<(int nominal, int count)>();

        foreach (var nominal in _money.Keys.OrderByDescending(n => n))
        {
            if (remaining <= 0) break;

            int available = _money[nominal];
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
        {
            _money[nominal] -= count;
            if (_money[nominal] == 0)
                _money.Remove(nominal);
        }

        SaveToExcel();
        return amount;
    }

    /// <summary>
    /// Перезагружает данные из Excel (для синхронизации UI).
    /// </summary>
    public void ReloadFromExcel()
    {
        LoadFromExcel();
    }

    /// <summary>
    /// Возвращает количество купюр указанного номинала.
    /// </summary>
    public int GetCountByNominal(int nominal)
    {
        return _money.TryGetValue(nominal, out var count) ? count : 0;
    }

    /// <summary>
    /// Добавляет указанное количество купюр заданного номинала.
    /// </summary>
    public void AddMoney(int nominal, int count)
    {
        if (_money.ContainsKey(nominal))
            _money[nominal] += count;
        else
            _money[nominal] = count;

        SaveToExcel();
    }

    /// <summary>
    /// Удаляет указанное количество купюр заданного номинала.
    /// Возвращает true, если удалось удалить; false — если недостаточно купюр.
    /// </summary>
    public bool RemoveMoney(int nominal, int count)
    {
        int available = GetCountByNominal(nominal);
        if (count > available)
            return false;

        _money[nominal] -= count;
        if (_money[nominal] == 0)
            _money.Remove(nominal);

        SaveToExcel();
        return true;
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
            var nominalStr = worksheet.Cells[row, 1].Value?.ToString();
            if (string.IsNullOrEmpty(nominalStr)) break;

            var countStr = worksheet.Cells[row, 2].Value?.ToString();

            if (int.TryParse(nominalStr, out var nominal) && int.TryParse(countStr, out var count))
            {
                if (count > 0)
                    _money[nominal] = count;
            }

            row++;
        }
    }

    private void SaveToExcel()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage(new FileInfo(_filePath));
        var worksheet = package.Workbook.Worksheets[0];

        // Очищаем данные (оставляем заголовок в строке 1)
        int maxRows = Math.Max(_money.Count + 1, 7); // минимум 7 строк (1 заголовок + 6 номиналов)
        for (int i = 2; i <= maxRows; i++)
        {
            worksheet.Cells[i, 1].Value = null;
            worksheet.Cells[i, 2].Value = null;
        }

        // Записываем все возможные номиналы
        var allNominals = new[] { 50, 100, 200, 500, 1000, 5000 };
        for (int i = 0; i < allNominals.Length; i++)
        {
            var nominal = allNominals[i];
            worksheet.Cells[i + 2, 1].Value = nominal;
            worksheet.Cells[i + 2, 2].Value = _money.TryGetValue(nominal, out var count) ? count : 0;
        }

        package.Save();
    }
}
