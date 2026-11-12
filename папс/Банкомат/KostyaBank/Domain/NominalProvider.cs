using System.Collections.ObjectModel;

namespace KostyaBankApp.Models;

/// <summary>
/// Поставщик доступных номиналов купюр.
/// Открывает для расширения: новый номинал = добавить новый класс-провайдер.
/// </summary>
public interface INominalProvider
{
    ReadOnlyCollection<int> Nominals { get; }
}

/// <summary>
/// Стандартный набор номиналов для банкомата.
/// </summary>
public class DefaultNominalProvider : INominalProvider
{
    private readonly ReadOnlyCollection<int> _nominals;

    public DefaultNominalProvider()
    {
        _nominals = new ReadOnlyCollection<int>(new[] { 50, 100, 200, 500, 1000, 5000 });
    }

    public ReadOnlyCollection<int> Nominals => _nominals;
}
