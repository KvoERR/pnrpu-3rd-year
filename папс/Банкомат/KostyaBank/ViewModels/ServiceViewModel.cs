using System.Windows.Input;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// ViewModel для экрана сервисных операций.
/// </summary>
public class ServiceViewModel : BaseViewModel
{
    private readonly AppViewModel _appViewModel;
    private string _statusText = "Сервисные операции";
    private readonly ICommand _backCommand;

    public ServiceViewModel(AppViewModel appViewModel)
    {
        _appViewModel = appViewModel;
        _backCommand = new SimpleCommand(() => _appViewModel.ShowCardEntry());
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public ICommand BackCommand => _backCommand;
}
