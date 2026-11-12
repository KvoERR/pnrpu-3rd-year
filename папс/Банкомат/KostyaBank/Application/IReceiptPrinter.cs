using System;

namespace KostyaBankApp.Models;

/// <summary>
/// Принтер для формирования и сохранения чеков/справок.
/// </summary>
public interface IReceiptPrinter
{
    /// <summary>
    /// Формирует чек снятия наличных и сохраняет в файл.
    /// </summary>
    string SaveWithdrawDoc(decimal amount, Card card, DateTime transactionDate);

    /// <summary>
    /// Формирует справку о балансе и сохраняет в файл.
    /// </summary>
    string SaveBalanceDoc(Card card, DateTime transactionDate);
}
