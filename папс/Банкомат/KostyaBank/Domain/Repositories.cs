using System.Collections.Generic;

namespace KostyaBankApp.Models;

/// <summary>
/// Репозиторий поиска карт в основной базе.
/// </summary>
public interface ICardRepository
{
    Card? GetCardByNumber(string cardNumber);
}

/// <summary>
/// Чтение данных из хранилища заблокированных карт.
/// </summary>
public interface ICardStorageReader
{
    int Count { get; }
    IEnumerable<Card> Cards { get; }
    Card? GetCardByNumber(string cardNumber);
}

/// <summary>
/// Запись в хранилище заблокированных карт.
/// </summary>
public interface ICardStorageWriter
{
    bool AddCard(Card card);
    bool RemoveCard(string cardNumber);
    void Clear();
}

/// <summary>
/// Объединённый интерфейс хранилища заблокированных карт.
/// </summary>
public interface ICardStorageRepository : ICardStorageReader, ICardStorageWriter { }

/// <summary>
/// Чтение состояния хранилища наличных.
/// </summary>
public interface IMoneyStorageReader
{
    int SumMoney();
    int GetCountByNominal(int nominal);
}

/// <summary>
/// Запись в хранилище наличных.
/// </summary>
public interface IMoneyStorageWriter
{
    void TakeMoney(int amount);
    void AddMoney(int nominal, int count);
    bool RemoveMoney(int nominal, int count);
    int ClearStorage();
}

/// <summary>
/// Перезагрузка данных хранилища из внешнего источника.
/// </summary>
public interface IMoneyStorageReloader
{
    void Reload();
}

/// <summary>
/// Объединённый интерфейс хранилища наличных.
/// </summary>
public interface IMoneyStorageRepository : IMoneyStorageReader, IMoneyStorageWriter, IMoneyStorageReloader { }
