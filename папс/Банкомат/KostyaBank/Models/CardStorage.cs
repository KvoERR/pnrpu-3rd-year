using System.Collections.Generic;
using System.Linq;

namespace KostyaBankApp.Models;

/// <summary>
/// Хранилище карт.
/// </summary>
public class CardStorage
{
    private readonly List<Card> _cards;

    public CardStorage()
    {
        _cards = new List<Card>();
    }

    public IEnumerable<Card> Cards => _cards.AsReadOnly();

    public int Count => _cards.Count;

    /// <summary>
    /// Находит карту по номеру.
    /// </summary>
    public Card? GetCardByNumber(string cardNumber)
    {
        return _cards.FirstOrDefault(c => c.CardNumber == cardNumber);
    }

    /// <summary>
    /// Добавляет карту в хранилище.
    /// </summary>
    public bool AddCard(Card card)
    {
        if (card == null || !card.IsValid)
            return false;

        if (_cards.Any(c => c.CardNumber == card.CardNumber))
            return false;

        _cards.Add(card);
        return true;
    }

    /// <summary>
    /// Удаляет карту по номеру.
    /// </summary>
    public bool RemoveCard(string cardNumber)
    {
        return _cards.RemoveAll(c => c.CardNumber == cardNumber) > 0;
    }

    /// <summary>
    /// Очищает хранилище.
    /// </summary>
    public void Clear()
    {
        _cards.Clear();
    }
}
