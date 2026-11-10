using KostyaBankApp.Models;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// Создаёт ShowFormViewModel.
/// </summary>
public static class ShowFormViewModelFactory
{
    public static ShowFormViewModel Create(Card card, AppViewModel app)
    {
        return new ShowFormViewModel(card, app);
    }
}
