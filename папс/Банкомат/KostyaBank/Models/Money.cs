namespace KostyaBankApp.Models;

/// <summary>
/// Represents a bank card with validation.
/// </summary>
public class Money
{
    private int _amount;
    public List<int> PossibleAmounts = new() { 50, 100, 200, 500, 1000, 5000 };

    public int Amount
    {
        get => _amount;
        set
        {
            if (!PossibleAmounts.Contains(value))
                throw new ArgumentException($"Недопустимый номинал: {value}. Допустимые: {string.Join(", ", PossibleAmounts)}");
            
            _amount = value;
        }
    }
}
