using KostyaBankApp.Models;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// Интерфейс навигации между экранами.
/// VMs зависят от этого интерфейса (DIP), а не от AppViewModel.
/// </summary>
public interface INavigationService
{
    void ShowCardEntry();
    void ShowMainMenu(Card card);
    void ShowGetForm(Card card);
    void ShowShowForm(Card card);
    void ShowServiceView();
    void ShowMoneyStorage();
    void ShowCardStorage();
}
