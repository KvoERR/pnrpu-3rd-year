namespace KostyaBankApp.Services;

/// <summary>
/// Factory для создания ViewModel через DI.
/// Заменяет Service Locator (IServiceProvider) паттерн.
/// </summary>
public interface IViewModelFactory
{
    T Create<T>(params object[] args) where T : class;
}

/// <summary>
/// Реализация factory на основе IServiceProvider.
/// </summary>
public class ViewModelFactory : IViewModelFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ViewModelFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public T Create<T>(params object[] args) where T : class
    {
        return Microsoft.Extensions.DependencyInjection.ActivatorUtilities.CreateInstance<T>(_serviceProvider, args);
    }
}
