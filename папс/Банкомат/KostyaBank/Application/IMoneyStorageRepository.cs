namespace KostyaBankApp.Models;

/// <summary>
/// Чтение состояния хранилища наличных.
/// </summary>
public interface IMoneyStorageReader
{
    /// <summary>
    /// Общая сумма наличных в хранилище.
    /// </summary>
    int SumMoney();

    /// <summary>
    /// Получает количество купюр указанного номинала.
    /// </summary>
    int GetCountByNominal(int nominal);
}

/// <summary>
/// Запись в хранилище наличных.
/// </summary>
public interface IMoneyStorageWriter
{
    /// <summary>
    /// Выдаёт наличные указанной суммы (жадный алгоритм по номиналам).
    /// </summary>
    void TakeMoney(int amount);

    /// <summary>
    /// Добавляет купюры в хранилище.
    /// </summary>
    void AddMoney(int nominal, int count);

    /// <summary>
    /// Удаляет купюры из хранилища.
    /// </summary>
    bool RemoveMoney(int nominal, int count);

    /// <summary>
    /// Очищает хранилище, возвращая общую сумму.
    /// </summary>
    int ClearStorage();
}

/// <summary>
/// Перезагрузка данных хранилища из внешнего источника.
/// </summary>
public interface IMoneyStorageReloader
{
    /// <summary>
    /// Перезагружает данные из источника.
    /// </summary>
    void Reload();
}

/// <summary>
/// Объединённый интерфейс для удобства регистрации в DI.
/// </summary>
public interface IMoneyStorageRepository : IMoneyStorageReader, IMoneyStorageWriter, IMoneyStorageReloader
{
}
