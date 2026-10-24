namespace KostyaBankApp.Models;

/// <summary>
/// Represents a bank card with validation.
/// </summary>
public class Card
{
    public string CardNumber { get; set; } = string.Empty;

    public bool IsValid
    {
        get
        {
            if (string.IsNullOrWhiteSpace(CardNumber) || CardNumber.Length != 16 || !CardNumber.All(char.IsDigit))
                return false;

            return true;
        }
    }

    public string MaskedNumber
    {
        get
        {
            if (CardNumber.Length < 4)
                return "****";
            var last4 = CardNumber.Substring(CardNumber.Length - 4);
            return $"**** **** **** {last4}";
        }
    }

    public string? ErrorMessage
    {
        get
        {
            if (string.IsNullOrWhiteSpace(CardNumber))
                return "Введите номер карты";
            if (CardNumber.Length < 16)
                return $"Введено {CardNumber.Length} из 16 цифр";
            if (!CardNumber.All(char.IsDigit))
                return "Номер карты должен содержать только цифры";
            return null;
        }
    }
}
