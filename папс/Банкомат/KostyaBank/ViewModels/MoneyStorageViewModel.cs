using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using KostyaBankApp.Models;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// ViewModel для хранилища денег (загрузка/выгрузка купюр).
/// </summary>
public class MoneyStorageViewModel : BaseViewModel
{
    private readonly CashStorage _cashStorage;
    private readonly AppViewModel _appViewModel;
    private readonly ICommand _backCommand;
    private readonly ICommand _loadCommand;
    private readonly ICommand _unloadCommand;
    private readonly ICommand _clearCommand;
    private int _selectedAmount;
    private string _statusText = string.Empty;
    private Visibility _statusVisibility = Visibility.Collapsed;
    private string _totalText = "0";

    public MoneyStorageViewModel(AppViewModel appViewModel)
    {
        _appViewModel = appViewModel;
        _cashStorage = new CashStorage();
        _backCommand = new SimpleCommand(() => _appViewModel.ShowServiceView());
        _loadCommand = new SimpleCommand(() => LoadMoney(), () => _selectedAmount > 0);
        _unloadCommand = new SimpleCommand(() => UnloadMoney(), () => _selectedAmount > 0);
        _clearCommand = new SimpleCommand(() => ClearStorage(), () => _cashStorage.SumMoney() > 0);
    }

    public ObservableCollection<int> AvailableAmounts { get; } = new ObservableCollection<int>
    {
        50, 100, 200, 500, 1000, 5000
    };

    public int SelectedAmount
    {
        get => _selectedAmount;
        set
        {
            if (SetProperty(ref _selectedAmount, value))
            {
                (_loadCommand as SimpleCommand)?.RaiseCanExecuteChanged();
                (_unloadCommand as SimpleCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string TotalText
    {
        get => _totalText;
        private set => SetProperty(ref _totalText, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public Visibility StatusVisibility
    {
        get => _statusVisibility;
        private set => SetProperty(ref _statusVisibility, value);
    }

    public bool CanExecuteAction => _selectedAmount > 0;
    public bool CanExecuteClear => _cashStorage.SumMoney() > 0;

    public ICommand BackCommand => _backCommand;
    public ICommand LoadCommand => _loadCommand;
    public ICommand UnloadCommand => _unloadCommand;
    public ICommand ClearCommand => _clearCommand;

    private void LoadMoney()
    {
        for (int i = 0; i < 5; i++)
        {
            _cashStorage.Money.Add(new Money { Amount = SelectedAmount });
        }
        UpdateTotal();
        StatusText = $"Загружено 5 купюр по {SelectedAmount} руб.";
        StatusVisibility = Visibility.Visible;
    }

    private void UnloadMoney()
    {
        int count = 0;
        for (int i = _cashStorage.Money.Count - 1; i >= 0 && count < 5; i--)
        {
            if (_cashStorage.Money[i].Amount == SelectedAmount)
            {
                _cashStorage.Money.RemoveAt(i);
                count++;
            }
        }

        if (count == 0)
        {
            StatusText = $"В хранилище нет купюр номиналом {SelectedAmount} руб.";
            StatusVisibility = Visibility.Visible;
            return;
        }

        UpdateTotal();
        StatusText = $"Выгружено {count} купюр по {SelectedAmount} руб.";
        StatusVisibility = Visibility.Visible;
    }

    private void ClearStorage()
    {
        int total = _cashStorage.ClearStorage();
        UpdateTotal();
        StatusText = $"Хранилище очищено. Выгружено {total} руб.";
        StatusVisibility = Visibility.Visible;
    }

    private void UpdateTotal()
    {
        TotalText = _cashStorage.SumMoney().ToString();
    }
}
