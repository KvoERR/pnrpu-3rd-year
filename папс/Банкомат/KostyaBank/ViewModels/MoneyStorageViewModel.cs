using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using KostyaBankApp.Models;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// ViewModel для хранилища денег (загрузка/выгрузка купюр по номиналам).
/// </summary>
public class MoneyStorageViewModel : BaseViewModel
{
    private readonly MoneyStorage _moneyStorage;
    private readonly AppViewModel _appViewModel;
    private readonly ICommand _backCommand;
    private readonly ICommand _loadAllCommand;
    private readonly ICommand _unloadAllCommand;
    private readonly ICommand _clearCommand;
    private string _statusText = string.Empty;
    private Visibility _statusVisibility = Visibility.Collapsed;
    private string _totalText = "0";

    public MoneyStorageViewModel(AppViewModel appViewModel)
    {
        _appViewModel = appViewModel;
        _moneyStorage = appViewModel.MoneyStorage;
        _backCommand = new SimpleCommand(() => _appViewModel.ShowServiceView());
        _loadAllCommand = new SimpleCommand(() => LoadAllMoney(), () => HasAnyEntries());
        _unloadAllCommand = new SimpleCommand(() => UnloadAllMoney(), () => HasAnyEntries());
        _clearCommand = new SimpleCommand(() => ClearStorage(), () => _moneyStorage.SumMoney() > 0);

        // Инициализация коллекции по всем номиналам
        MoneyEntries = new ObservableCollection<Money>
        {
            new Money { Amount = 50 },
            new Money { Amount = 100 },
            new Money { Amount = 200 },
            new Money { Amount = 500 },
            new Money { Amount = 1000 },
            new Money { Amount = 5000 }
        };

        // Подписка на изменения Count
        foreach (var money in MoneyEntries)
            money.PropertyChanged += OnMoneyEntryChanged;

        ReloadFromExcel();
        UpdateTotal();
    }

    public ObservableCollection<Money> MoneyEntries { get; }

    public string TotalText { get => _totalText; private set => SetProperty(ref _totalText, value); }
    public string StatusText { get => _statusText; private set => SetProperty(ref _statusText, value); }
    public Visibility StatusVisibility { get => _statusVisibility; private set => SetProperty(ref _statusVisibility, value); }

    public ICommand BackCommand => _backCommand;
    public ICommand LoadAllCommand => _loadAllCommand;
    public ICommand UnloadAllCommand => _unloadAllCommand;
    public ICommand ClearCommand => _clearCommand;

    private void OnMoneyEntryChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Money.Count))
        {
            (_loadAllCommand as SimpleCommand)?.RaiseCanExecuteChanged();
            (_unloadAllCommand as SimpleCommand)?.RaiseCanExecuteChanged();
        }
    }

    private bool HasAnyEntries() => MoneyEntries.Any(m => m.Count > 0);

    private void LoadAllMoney()
    {
        var details = new List<string>();
        foreach (var money in MoneyEntries)
        {
            if (money.Count > 0)
            {
                _moneyStorage.AddMoney(money.Amount, money.Count);
                details.Add($"{money.Count} × {money.Amount} руб.");
                money.Count = 0; // Сброс после загрузки
            }
        }
        ReloadFromExcel();
        StatusText = $"Загружено: {string.Join(", ", details)}";
        StatusVisibility = Visibility.Visible;
    }

    private void UnloadAllMoney()
    {
        var details = new List<string>();
        foreach (var money in MoneyEntries)
        {
            if (money.Count > 0)
            {
                int available = _moneyStorage.GetCountByNominal(money.Amount);
                int toUnload = Math.Min(money.Count, available);
                if (toUnload == 0) continue;
                _moneyStorage.RemoveMoney(money.Amount, toUnload);
                details.Add($"{toUnload} × {money.Amount} руб.");
                money.Count -= toUnload; // Уменьшаем на невыгруженное
            }
        }
        if (details.Count > 0)
        {
            ReloadFromExcel();
            StatusText = $"Выгружено: {string.Join(", ", details)}";
            StatusVisibility = Visibility.Visible;
        }
    }

    private void ClearStorage()
    {
        int total = _moneyStorage.ClearStorage();
        ReloadFromExcel();
        StatusText = $"Хранилище очищено. Выгружено {total} руб.";
        StatusVisibility = Visibility.Visible;
    }

    private void UpdateTotal() => TotalText = _moneyStorage.SumMoney().ToString();

    /// <summary>
    /// Перезагружает Count из MoneyStorage (Excel) в MoneyEntries.
    /// </summary>
    private void ReloadFromExcel()
    {
        _moneyStorage.ReloadFromExcel();
        foreach (var money in MoneyEntries)
        {
            money.Count = _moneyStorage.GetCountByNominal(money.Amount);
        }
        UpdateTotal();
    }
}
