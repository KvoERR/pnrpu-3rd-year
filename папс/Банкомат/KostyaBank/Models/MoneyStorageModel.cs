using System.Collections.Generic;
using System.Linq;

namespace KostyaBankApp.Models;

/// <summary>
/// Модель хранилища наличных — только бизнес-логика, без I/O.
/// </summary>
public class MoneyStorageModel
{
    private readonly Dictionary<int, int> _money;

    public MoneyStorageModel()
    {
        _money = new Dictionary<int, int>();
    }

    public int SumMoney()
    {
        return _money.Sum(m => m.Key * m.Value);
    }

    public int GetCountByNominal(int nominal)
    {
        return _money.TryGetValue(nominal, out var count) ? count : 0;
    }

    /// <summary>
    /// Выдаёт наличные указанной суммы жадным алгоритмом (от крупных купюр к мелким).
    /// </summary>
    /// <param name="amount">Сумма для выдачи.</param>
    /// <returns>Фактическая сумма выдачи.</returns>
    /// <exception cref="InvalidOperationException">Недостаточно средств или нет подходящих номиналов.</exception>
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

        return amount;
    }

    public void AddMoney(int nominal, int count)
    {
        if (_money.ContainsKey(nominal))
            _money[nominal] += count;
        else
            _money[nominal] = count;
    }

    public bool RemoveMoney(int nominal, int count)
    {
        int available = GetCountByNominal(nominal);
        if (count > available)
            return false;

        _money[nominal] -= count;
        if (_money[nominal] == 0)
            _money.Remove(nominal);

        return true;
    }

    public int ClearStorage()
    {
        int total = SumMoney();
        _money.Clear();
        return total;
    }

    /// <summary>
    /// Загружает данные из словаря (используется при загрузке из Excel).
    /// </summary>
    public void LoadData(Dictionary<int, int> data)
    {
        _money.Clear();
        foreach (var kvp in data)
            _money[kvp.Key] = kvp.Value;
    }

    /// <summary>
    /// Возвращает текущие данные для сохранения.
    /// </summary>
    public Dictionary<int, int> GetData()
    {
        return new Dictionary<int, int>(_money);
    }
}
