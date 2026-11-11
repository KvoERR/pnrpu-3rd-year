using KostyaBankApp.Models;
using KostyaBankApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace KostyaBankApp.Services;

/// <summary>
/// Навигатор экранов. ОСП-совместим: один метод Navigate, маршруты регистрируются вне класса.
/// Зависимости VMs создаются DI-контейнером.
/// </summary>
public class NavigationService : BaseViewModel, INavigationService
{
    private readonly Dictionary<string, Type> _routes = new();
    private readonly IServiceProvider _serviceProvider;
    private Card? _currentCard;
    private object? _currentView;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public object? CurrentView
    {
        get => _currentView;
        private set => SetProperty(ref _currentView, value);
    }

    /// <summary>
    /// Регистрирует маршрут (вызывается один раз в Composition Root).
    /// </summary>
    public void RegisterRoute(string route, Type viewModelType)
    {
        if (string.IsNullOrWhiteSpace(route))
            throw new ArgumentException("Route cannot be null or empty", nameof(route));
        if (viewModelType == null)
            throw new ArgumentNullException(nameof(viewModelType));

        _routes[route] = viewModelType;
    }

    /// <summary>
    /// Переход на экран по маршруту.
    /// </summary>
    public void Navigate(string route, object? parameter = null)
    {
        if (!_routes.TryGetValue(route, out var viewModelType))
            throw new InvalidOperationException($"Route '{route}' is not registered.");

        // Сохраняем карту при входе в mainMenu
        if (route == "mainMenu" && parameter is Card card)
            _currentCard = card;

        // Если маршрут mainMenu и нет параметра — подставляем текущую карту
        if (route == "mainMenu" && parameter == null && _currentCard != null)
            parameter = _currentCard;

        CurrentView = parameter != null
            ? ActivatorUtilities.CreateInstance(_serviceProvider, viewModelType, parameter)
            : ActivatorUtilities.CreateInstance(_serviceProvider, viewModelType);
    }
}
