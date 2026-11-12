using System.ComponentModel;
using KostyaBankApp.Models;

namespace KostyaBankApp.Services;

public class Navigator : INavigator, INotifyPropertyChanged
{
    private readonly IViewModelFactory _viewModelFactory;
    private object? _currentView;

    public Navigator(IViewModelFactory viewModelFactory)
    {
        _viewModelFactory = viewModelFactory ?? throw new ArgumentNullException(nameof(viewModelFactory));
    }

    public object? CurrentView
    {
        get => _currentView;
        private set
        {
            if (ReferenceEquals(_currentView, value))
                return;

            _currentView = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentView)));
        }
    }

    public void Navigate<TViewModel>(params object[] args) where TViewModel : class
    {
        CurrentView = _viewModelFactory.Create<TViewModel>(args);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
