using KostyaBankApp.Models;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// Создаёт CardStorageViewModel.
/// </summary>
public static class CardStorageViewModelFactory
{
    public static CardStorageViewModel Create(AppViewModel app)
    {
        return new CardStorageViewModel(app);
    }
}
