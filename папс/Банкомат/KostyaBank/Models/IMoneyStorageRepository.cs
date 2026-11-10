namespace KostyaBankApp.Models;

/// <summary>
/// Репозиторий для управления наличными в банкомате.
/// </summary>
public interface IMoneyStorageRepository
{
    /// <summary>
    /// Общая сумма наличных в хранилище.
    /// </summary>
    int SumMoney();

    /// <summary>
    /// Получает количество купюр указанного номинала.
    /// </summary>
    int GetCountByNominal(int nominal);

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

    /// <summary>
    /// Перезагружает данные из источника (Excel).
    /// </summary>
    void ReloadFromExcel();
}
