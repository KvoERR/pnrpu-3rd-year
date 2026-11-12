namespace KostyaBankApp.Models;

/// <summary>
/// Чтение данных из хранилища заблокированных карт.
/// </summary>
public interface ICardStorageReader
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
}

/// <summary>
/// Запись в хранилище заблокированных карт.
/// </summary>
public interface ICardStorageWriter
{
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

/// <summary>
/// Объединённый интерфейс для удобства регистрации в DI.
/// </summary>
public interface ICardStorageRepository : ICardStorageReader, ICardStorageWriter
{
}
