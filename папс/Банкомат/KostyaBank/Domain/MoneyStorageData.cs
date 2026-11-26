using System.Collections.Generic;

namespace KostyaBankApp.Models;

/// <summary>
/// Контракт хранилища наличных.
/// </summary>
public interface IMoneyStorageData
{
    int Sum { get; }
    void Add(int nominal, int count);
    int GetCount(int nominal);
    bool Remove(int nominal, int count);
    void Clear();
    IEnumerable<(int Nominal, int Count)> GetAll();
}

/// <summary>
/// Реализация на Dictionary — замена на SortedDictionary/другую структуру без изменения контракта.
/// </summary>
public class MoneyStorageData : IMoneyStorageData
{
    private readonly Dictionary<int, int> _data = new();

    public int Sum
    {
        get
        {
            int total = 0;
            foreach (var kvp in _data) total += kvp.Key * kvp.Value;
            return total;
        }
    }

    public void Add(int nominal, int count)
    {
        if (_data.TryGetValue(nominal, out var existing))
            _data[nominal] = existing + count;
        else
            _data[nominal] = count;
    }

    public int GetCount(int nominal) => _data.TryGetValue(nominal, out var c) ? c : 0;

    public bool Remove(int nominal, int count)
    {
        if (!_data.TryGetValue(nominal, out var available)) return false;
        if (count > available) return false;
        _data[nominal] -= count;
        if (_data[nominal] == 0) _data.Remove(nominal);
        return true;
    }

    public void Clear() => _data.Clear();

    public IEnumerable<(int Nominal, int Count)> GetAll()
    {
        foreach (var kvp in _data) yield return (kvp.Key, kvp.Value);
    }
}
