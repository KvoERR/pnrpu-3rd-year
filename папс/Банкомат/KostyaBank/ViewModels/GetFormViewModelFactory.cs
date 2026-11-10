using KostyaBankApp.Models;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// Создаёт GetFormViewModel.
/// </summary>
public static class GetFormViewModelFactory
{
    public static GetFormViewModel Create(Card card, INavigationService navigation)
    {
        return new GetFormViewModel(card, navigation);
    }
}
