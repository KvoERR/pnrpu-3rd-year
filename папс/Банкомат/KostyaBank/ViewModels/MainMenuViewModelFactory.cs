using KostyaBankApp.Models;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// Создаёт MainMenuViewModel.
/// </summary>
public static class MainMenuViewModelFactory
{
    public static MainMenuViewModel Create(Card card, INavigationService navigation)
    {
        return new MainMenuViewModel(card, navigation);
    }
}
