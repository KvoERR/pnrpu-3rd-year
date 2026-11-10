namespace KostyaBankApp.Models;

/// <summary>
/// Репозиторий для управления заблокированными картами.
/// </summary>
public interface ICardStorageRepository
{
    /// <summary>
    /// Количество карт в хранилище.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Коллекция карт в хранилище.
    /// </summary>
    IEnumerable<Card> Cards { get; }

    /// <summary>
    /// Находит карту по номеру.
    /// </summary>
    Card? GetCardByNumber(string cardNumber);

    /// <summary>
    /// Добавляет карту в хранилище.
    /// </summary>
    bool AddCard(Card card);

    /// <summary>
    /// Удаляет карту по номеру.
    /// </summary>
    bool RemoveCard(string cardNumber);

    /// <summary>
    /// Очищает хранилище.
    /// </summary>
    void Clear();
}
