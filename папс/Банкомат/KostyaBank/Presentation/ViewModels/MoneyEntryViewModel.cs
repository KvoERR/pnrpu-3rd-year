using KostyaBankApp.Models;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// ViewModel-обёртка для ввода количества купюр конкретного номинала.
/// Содержит UI-контракт (INotifyPropertyChanged) — находится в Presentation, а не в Domain.
/// </summary>
public class MoneyEntryViewModel : BaseViewModel
{
    private readonly INominalProvider _nominalProvider;
    private int _amount;
    private int _count;

    public MoneyEntryViewModel(INominalProvider nominalProvider)
    {
        _nominalProvider = nominalProvider ?? throw new ArgumentNullException(nameof(nominalProvider));
    }

    /// <summary>
    /// Номинал купюры (50, 100, 200, …). Устанавливается один раз при инициализации.
    /// </summary>
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

    /// <summary>
    /// Подсумок: номинал × количество.
    /// </summary>
    public int Subtotal => Amount * Count;
}
