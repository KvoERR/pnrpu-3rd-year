using System.Windows.Input;
using KostyaBankApp.Models;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// ViewModel для экрана сервисных операций.
/// </summary>
public class ServiceViewModel : BaseViewModel
{
    private readonly AppViewModel _appViewModel;
    private string _statusText = "Сервисные операции";
    private readonly ICommand _backCommand;
    private readonly ICommand _moneyStorageCommand;
    private readonly ICommand _cardStorageCommand;

    public ServiceViewModel(AppViewModel appViewModel)
    {
        _appViewModel = appViewModel ?? throw new ArgumentNullException(nameof(appViewModel));
        _backCommand = new SimpleCommand(() => _appViewModel.Show(CardEntryViewModelFactory.Create(_appViewModel)));
        _moneyStorageCommand = new SimpleCommand(() => _appViewModel.Show(MoneyStorageViewModelFactory.Create(_appViewModel)));
        _cardStorageCommand = new SimpleCommand(() => _appViewModel.Show(CardStorageViewModelFactory.Create(_appViewModel)));
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public ICommand BackCommand => _backCommand;
    public ICommand MoneyStorageCommand => _moneyStorageCommand;
    public ICommand CardStorageCommand => _cardStorageCommand;
}
