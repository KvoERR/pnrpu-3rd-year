namespace KostyaBankApp.Models;

/// <summary>
/// Бизнес-модель банковской карты. Чистая POCO — без валидации и UI-логики.
/// </summary>
public class Card
{
    public string CardNumber { get; set; } = string.Empty;
    public string HolderName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string PinCode { get; set; } = string.Empty;
}
