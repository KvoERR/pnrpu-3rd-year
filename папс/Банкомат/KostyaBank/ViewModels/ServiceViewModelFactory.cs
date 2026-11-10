namespace KostyaBankApp.ViewModels;

/// <summary>
/// Создаёт ServiceViewModel.
/// </summary>
public static class ServiceViewModelFactory
{
    public static ServiceViewModel Create(AppViewModel app)
    {
        return new ServiceViewModel(app);
    }
}
