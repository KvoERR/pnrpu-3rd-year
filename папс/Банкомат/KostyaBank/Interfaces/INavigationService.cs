namespace KostyaBankApp.Services;

/// <summary>
/// Интерфейс навигации между экранами.
/// VMs зависят от этого интерфейса (DIP).
/// Маршруты регистрируются в Composition Root — OCP соблюдён.
/// </summary>
public interface INavigationService
{
    object? CurrentView { get; }

    /// <summary>
    /// Переход на экран по маршруту. Параметр — опциональный аргумент (например, Card).
    /// </summary>
    void Navigate(string route, object? parameter = null);
}
