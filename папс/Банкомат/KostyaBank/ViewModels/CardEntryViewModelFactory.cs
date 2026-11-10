using KostyaBankApp.Models;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// Создаёт CardEntryViewModel.
/// </summary>
public static class CardEntryViewModelFactory
{
    public static CardEntryViewModel Create(INavigationService navigation)
    {
        return new CardEntryViewModel(
            new ExcelCardRepository(),
            new CardStorageRepository(),
            navigation);
    }
}
