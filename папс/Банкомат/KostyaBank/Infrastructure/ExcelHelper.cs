using OfficeOpenXml;

namespace KostyaBankApp.Models;

/// <summary>
/// Общие утилиты для чтения Excel-файлов.
/// Убирает дублирование парсинга между репозиториями.
/// </summary>
public static class ExcelHelper
{
    public static string? GetCellString(ExcelWorksheet ws, int row, int col)
        => ws.Cells[row, col].Value?.ToString();

    public static decimal GetCellDecimal(ExcelWorksheet ws, int row, int col)
    {
        var raw = ws.Cells[row, col].Value?.ToString();
        return decimal.TryParse(raw, out var val) ? val : 0;
    }

    public static int GetCellInt(ExcelWorksheet ws, int row, int col)
    {
        var raw = ws.Cells[row, col].Value?.ToString();
        return int.TryParse(raw, out var val) ? val : 0;
    }

    public static void SetCell(this ExcelWorksheet ws, int row, int col, object? value)
        => ws.Cells[row, col].Value = value;

    public static void ClearCell(this ExcelWorksheet ws, int row, int col)
        => ws.Cells[row, col].Value = null;
}
