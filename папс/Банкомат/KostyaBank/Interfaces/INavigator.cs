namespace KostyaBankApp.Models;

public interface INavigator
{
    object? CurrentView { get; }
    void Navigate<TViewModel>(params object[] args) where TViewModel : class;
}