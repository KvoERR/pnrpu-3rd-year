namespace KostyaBankApp.Models;

/// <summary>
/// Репозиторий для поиска карт в базе данных банкомата.
/// </summary>
public interface ICardRepository
{
    /// <summary>
    /// Находит карту по номеру.
    /// </summary>
    Card? GetCardByNumber(string cardNumber);
}
