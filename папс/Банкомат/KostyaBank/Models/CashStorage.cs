namespace KostyaBankApp.Models;

/// <summary>
/// Хранилище денег банкомата (наличные в купюрах).
/// </summary>
public class CashStorage
{
    public List<Money> Money { get; } = new List<Money>();

    public int SumMoney()
    {
        return Money.Sum(m => m.Amount);
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
        Money.Clear();
        return total;
    }

    public int TakeMoney(int amount)
    {
        if (amount > SumMoney())
            throw new InvalidOperationException("Недостаточно средств в хранилище");

        int remaining = amount;
        var billsToTake = new List<Money>();

        foreach (var bill in Money.OrderByDescending(m => m.Amount))
        {
            if (remaining <= 0) break;

            int takeCount = Math.Min(remaining / bill.Amount, 1);
            for (int i = 0; i < takeCount; i++)
            {
                billsToTake.Add(bill);
                remaining -= bill.Amount;
            }
        }

        if (remaining > 0)
            throw new InvalidOperationException("Невозможно выдать указанную сумму доступными номиналами");

        foreach (var bill in billsToTake)
            Money.Remove(bill);

        return amount;
    }
}
