using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using KostyaBankApp.Models;
using KostyaBankApp.Services;

namespace KostyaBankApp.ViewModels;

/// <summary>
/// ViewModel для хранилища карт (выгрузка всех карт).
/// </summary>
public class CardStorageViewModel : BaseViewModel
{
    private readonly ICardStorageReader _cardStorageReader;
    private readonly ICardStorageWriter _cardStorageWriter;
    private readonly INavigator _navigation;
    private readonly ICommand _backCommand;
    private readonly ICommand _clearCommand;
    private string _statusText = string.Empty;
    private Visibility _statusVisibility = Visibility.Collapsed;
    private string _countText = "0";

    public CardStorageViewModel(INavigator navigation, ICardStorageReader cardStorageReader, ICardStorageWriter cardStorageWriter)
    {
        _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
        _cardStorageReader = cardStorageReader ?? throw new ArgumentNullException(nameof(cardStorageReader));
        _cardStorageWriter = cardStorageWriter ?? throw new ArgumentNullException(nameof(cardStorageWriter));
        _backCommand = new SimpleCommand(() => _navigation.Navigate<ServiceViewModel>());
        _clearCommand = new SimpleCommand(() => ClearStorage(), () => _cardStorageReader.Count > 0);

        foreach (var card in _cardStorageReader.Cards)
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

    public bool CanExecuteClear => _cardStorageReader.Count > 0;

    public ICommand BackCommand => _backCommand;
    public ICommand ClearCommand => _clearCommand;

    private void ClearStorage()
    {
        Cards.Clear();
        _cardStorageWriter.Clear();
        CountText = "0";
        StatusText = "Хранилище карт очищено.";
        StatusVisibility = Visibility.Visible;
    }
}
