using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace KostyaBankApp.Models;

/// <summary>
/// Репозиторий для хранения наличных — отвечает только за I/O (Excel).
/// </summary>
public class MoneyStorageRepository : IMoneyStorageRepository
{
    private readonly MoneyStorageModel _model;
    private readonly INominalProvider _nominalProvider;
    private readonly string _filePath;

    public MoneyStorageRepository(MoneyStorageModel model, INominalProvider nominalProvider)
    {
        _model = model;
        _nominalProvider = nominalProvider ?? throw new ArgumentNullException(nameof(nominalProvider));
        _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "money_storage.xlsx");
        LoadFromExcel();
    }

    public int SumMoney() => _model.SumMoney();
    public int GetCountByNominal(int nominal) => _model.GetCountByNominal(nominal);
    public void TakeMoney(int amount) => _model.TakeMoney(amount);
    public void AddMoney(int nominal, int count) => _model.AddMoney(nominal, count);
    public bool RemoveMoney(int nominal, int count) => _model.RemoveMoney(nominal, count);
    public int ClearStorage() => _model.ClearStorage();

    public void Reload()
    {
        LoadFromExcel();
    }

    public void ReloadFromExcel()
    {
        Reload();
    }

    private void LoadFromExcel()
    {
        if (!File.Exists(_filePath))
            return;

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage(new FileInfo(_filePath));
        var worksheet = package.Workbook.Worksheets[0];

        if (worksheet == null) return;

        var data = new Dictionary<int, int>();
        int row = 2;
        while (true)
        {
            var nominalStr = worksheet.Cells[row, 1].Value?.ToString();
            if (string.IsNullOrEmpty(nominalStr)) break;

            var countStr = worksheet.Cells[row, 2].Value?.ToString();

            if (int.TryParse(nominalStr, out var nominal) && int.TryParse(countStr, out var count))
            {
                if (count > 0)
                    data[nominal] = count;
            }

            row++;
        }

        _model.LoadData(data);
    }

    private void SaveToExcel()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage(new FileInfo(_filePath));
        var worksheet = package.Workbook.Worksheets[0];

        int maxRows = Math.Max(_model.GetData().Count + 1, 7);
        for (int i = 2; i <= maxRows; i++)
        {
            worksheet.Cells[i, 1].Value = null;
            worksheet.Cells[i, 2].Value = null;
        }

        var allNominals = _nominalProvider.Nominals;
        var data = _model.GetData();
        for (int i = 0; i < allNominals.Count; i++)
        {
            var nominal = allNominals[i];
            worksheet.Cells[i + 2, 1].Value = nominal;
            worksheet.Cells[i + 2, 2].Value = data.TryGetValue(nominal, out var count) ? count : 0;
        }

        package.Save();
    }
}
