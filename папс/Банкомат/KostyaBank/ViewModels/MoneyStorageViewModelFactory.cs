using KostyaBankApp.Models;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// Создаёт MoneyStorageViewModel.
/// </summary>
public static class MoneyStorageViewModelFactory
{
    public static MoneyStorageViewModel Create(AppViewModel app)
    {
        return new MoneyStorageViewModel(app);
    }
}
