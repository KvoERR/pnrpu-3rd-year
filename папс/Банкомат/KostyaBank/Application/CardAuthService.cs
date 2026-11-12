using KostyaBankApp.Models;

namespace KostyaBankApp.Services;

/// <summary>
/// Результат попытки аутентификации карты.
/// </summary>
public class AuthResult
{
    public bool Success { get; init; }
    public Card? Card { get; init; }
    public string? ErrorMessage { get; init; }
    public string? StatusText { get; init; }
    public bool CardWasBlocked { get; init; }

    private AuthResult(bool success, Card? card = null, string? errorMessage = null, string? statusText = null, bool cardWasBlocked = false)
    {
        Success = success;
        Card = card;
        ErrorMessage = errorMessage;
        StatusText = statusText;
        CardWasBlocked = cardWasBlocked;
    }

    public static AuthResult Ok(Card card) => new(true, card, statusText: "Готово");
    public static AuthResult Error(string error, string? statusText = null) => new(false, null, error, statusText ?? error);
    public static AuthResult CardBlocked() => new(false, null, "Карта заблокирована", "Карта заблокирована", true);
}

/// <summary>
/// Сервис аутентификации карт.
/// Отвечает только за проверку PIN и блокировку после 3 неудачных попыток.
/// </summary>
public interface ICardAuthService
{
    AuthResult Authenticate(string cardNumber, string pinCode);
    bool IsCardBlocked(string cardNumber);
}

/// <summary>
/// Реализация сервиса аутентификации.
/// </summary>
public class CardAuthService : ICardAuthService
{
    private const int MaxAttempts = 3;
    private readonly ICardRepository _cardRepository;
    private readonly ICardStorageRepository _cardStorage;
    private readonly ICardValidator _cardValidator;
    private readonly Dictionary<string, int> _failedAttempts;

    public CardAuthService(ICardRepository cardRepository, ICardStorageRepository cardStorage, ICardValidator cardValidator)
    {
        _cardRepository = cardRepository ?? throw new ArgumentNullException(nameof(cardRepository));
        _cardStorage = cardStorage ?? throw new ArgumentNullException(nameof(cardStorage));
        _cardValidator = cardValidator ?? throw new ArgumentNullException(nameof(cardValidator));
        _failedAttempts = new Dictionary<string, int>();
    }

    public bool IsCardBlocked(string cardNumber)
    {
        return _cardStorage.GetCardByNumber(cardNumber) != null;
    }

    public AuthResult Authenticate(string cardNumber, string pinCode)
    {
        // Валидация формата
        var cardValidation = _cardValidator.ValidateCardNumber(cardNumber);
        if (!cardValidation.IsValid)
            return AuthResult.Error(cardValidation.ErrorMessage!, "Неверный формат карты");

        var pinValidation = _cardValidator.ValidatePin(pinCode);
        if (!pinValidation.IsValid)
            return AuthResult.Error(pinValidation.ErrorMessage!, "Неверный формат PIN");

        // Проверка блокировки
        if (IsCardBlocked(cardNumber))
            return AuthResult.CardBlocked();

        // Поиск карты
        var card = _cardRepository.GetCardByNumber(cardNumber);
        if (card == null)
            return AuthResult.Error("Карта не найдена в базе", "Не найдена");

        // Проверка PIN
        if (card.PinCode != pinCode)
        {
            if (!_failedAttempts.ContainsKey(cardNumber))
                _failedAttempts[cardNumber] = 0;

            _failedAttempts[cardNumber]++;
            var remaining = MaxAttempts - _failedAttempts[cardNumber];

            if (_failedAttempts[cardNumber] >= MaxAttempts)
            {
                _cardStorage.AddCard(card);
                _failedAttempts.Remove(cardNumber);
                return AuthResult.Error(
                    $"Карта заблокирована после {MaxAttempts} неверных попыток. Карта помещена в хранилище.",
                    "Карта заблокирована");
            }
            else
            {
                return AuthResult.Error(
                    $"Неверный PIN-код. Осталось попыток: {remaining}",
                    "Неверный PIN");
            }
        }

        // Успех
        _failedAttempts.Remove(cardNumber);
        return AuthResult.Ok(card);
    }
}
