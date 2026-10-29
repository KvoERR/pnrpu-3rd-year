using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using KostyaBankApp.Models;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// ViewModel для хранилища карт (выгрузка всех карт).
/// </summary>
public class CardStorageViewModel : BaseViewModel
{
    private readonly CardStorage _cardStorage;
    private readonly AppViewModel _appViewModel;
    private readonly ICommand _backCommand;
    private readonly ICommand _exportCommand;
    private readonly ICommand _clearCommand;
    private string _statusText = string.Empty;
    private Visibility _statusVisibility = Visibility.Collapsed;
    private string _countText = "0";

    public CardStorageViewModel(AppViewModel appViewModel)
    {
        _appViewModel = appViewModel;
        _cardStorage = new CardStorage();
        _backCommand = new SimpleCommand(() => _appViewModel.ShowServiceView());
        _clearCommand = new SimpleCommand(() => ClearStorage(), () => _cardStorage.Count > 0);

        // Данные загружаются из Excel автоматически
        foreach (var card in _cardStorage.Cards)
            Cards.Add(card);
        CountText = Cards.Count.ToString();
    }

    public ObservableCollection<Card> Cards { get; } = new ObservableCollection<Card>();

    public string CountText
    {
        get => _countText;
        private set => SetProperty(ref _countText, value);
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

    public bool CanExecuteExport => _cardStorage.Count > 0;
    public bool CanExecuteClear => _cardStorage.Count > 0;

    public ICommand BackCommand => _backCommand;
    public ICommand ClearCommand => _clearCommand;

    private void ClearStorage()
    {
        Cards.Clear();
        _cardStorage.Clear();
        CountText = "0";
        StatusText = "Хранилище карт очищено.";
        StatusVisibility = Visibility.Visible;
    }
}
