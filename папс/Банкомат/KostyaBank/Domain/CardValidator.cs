namespace KostyaBankApp.Models;

/// <summary>
/// Результат валидации.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }

    private ValidationResult(bool isValid, string? errorMessage = null)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public static ValidationResult Ok() => new(true);
    public static ValidationResult Fail(string error) => new(false, error);
}

/// <summary>
/// Валидатор номера карты и PIN-кода.
/// Отвечает только за форматную проверку (SRP).
/// </summary>
public interface ICardValidator
{
    ValidationResult ValidateCardNumber(string cardNumber);
    ValidationResult ValidatePin(string pinCode);
}

/// <summary>
/// Стандартная реализация валидатора.
/// </summary>
public class CardValidator : ICardValidator
{
    public ValidationResult ValidateCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
            return ValidationResult.Fail("Введите номер карты");
        if (cardNumber.Length < 16)
            return ValidationResult.Fail($"Введено {cardNumber.Length} из 16 цифр");
        if (!cardNumber.All(char.IsDigit))
            return ValidationResult.Fail("Номер карты должен содержать только цифры");
        return ValidationResult.Ok();
    }

    public ValidationResult ValidatePin(string pinCode)
    {
        if (string.IsNullOrEmpty(pinCode) || pinCode.Length != 4 || !pinCode.All(char.IsDigit))
            return ValidationResult.Fail("PIN-код должен содержать 4 цифры");
        return ValidationResult.Ok();
    }
}
