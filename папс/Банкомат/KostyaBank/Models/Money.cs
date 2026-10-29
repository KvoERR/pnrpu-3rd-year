using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KostyaBankApp.Models;

/// <summary>
/// Денежная купюра. В хранилище — одна штука (Count = 0).
/// В ViewModel — модель ввода: Amount = номинал, Count = сколько штук.
/// </summary>
public class Money : INotifyPropertyChanged
{
    private int _amount;
    private int _count;
    public List<int> PossibleAmounts = new() { 50, 100, 200, 500, 1000, 5000 };

    public int Amount
    {
        get => _amount;
        set
        {
            if (!PossibleAmounts.Contains(value))
                throw new ArgumentException($"Недопустимый номинал: {value}. Допустимые: {string.Join(", ", PossibleAmounts)}");
            _amount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Subtotal));
        }
    }

    public int Count
    {
        get => _count;
        set { _count = value; OnPropertyChanged(); OnPropertyChanged(nameof(Subtotal)); }
    }

    public int Subtotal => Amount * Count;

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
