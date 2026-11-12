using System.ComponentModel;
using KostyaBankApp.Models;
using Microsoft.Extensions.DependencyInjection;

namespace KostyaBankApp.Services;

public class Navigator : INavigator, INotifyPropertyChanged
{
    private readonly IServiceProvider _serviceProvider;
    private object? _currentView;

    public Navigator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
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
        CurrentView = ActivatorUtilities.CreateInstance<TViewModel>(_serviceProvider, args);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}