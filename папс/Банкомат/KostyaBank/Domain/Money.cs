using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KostyaBankApp.Models;

/// <summary>
/// Денежная купюра. В хранилище — одна штука (Count = 0).
/// В ViewModel — модель ввода: Amount = номинал, Count = сколько штук.
/// </summary>
public class Money : INotifyPropertyChanged
{
    private readonly INominalProvider _nominalProvider;
    private int _amount;
    private int _count;

    public Money(INominalProvider nominalProvider)
    {
        _nominalProvider = nominalProvider ?? throw new ArgumentNullException(nameof(nominalProvider));
    }

    public int Amount
    {
        get => _amount;
        set
        {
            if (!_nominalProvider.Nominals.Contains(value))
                throw new ArgumentException($"Недопустимый номинал: {value}. Допустимые: {string.Join(", ", _nominalProvider.Nominals)}");
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

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
