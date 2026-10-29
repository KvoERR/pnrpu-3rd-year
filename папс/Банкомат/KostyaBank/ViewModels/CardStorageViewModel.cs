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
        _exportCommand = new SimpleCommand(() => ExportCards(), () => _cardStorage.Count > 0);
        _clearCommand = new SimpleCommand(() => ClearStorage(), () => _cardStorage.Count > 0);

        LoadSampleCards();
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
    public ICommand ExportCommand => _exportCommand;
    public ICommand ClearCommand => _clearCommand;

    private void LoadSampleCards()
    {
        var sampleCards = new[]
        {
            ("4276380000001001", "Иванов Иван Иванович", 50000m),
            ("4276380000001002", "Петрова Мария Сергеевна", 12500m),
            ("4276380000001003", "Сидоров Алексей Петрович", 78000m),
        };

        foreach (var (number, name, balance) in sampleCards)
        {
            var card = new Card { CardNumber = number, HolderName = name, Balance = balance };
            if (card.IsValid)
            {
                Cards.Add(card);
                _cardStorage.AddCard(card);
            }
        }

        CountText = Cards.Count.ToString();
    }

    private void ExportCards()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Номер карты    | Владелец                  | Баланс");
        sb.AppendLine(new string('-', 70));

        foreach (var card in Cards)
        {
            sb.AppendLine($"{card.CardNumber} | {card.HolderName,-25} | {card.Balance,10:C2}");
        }

        sb.AppendLine(new string('-', 70));
        sb.AppendLine($"Итого карт: {Cards.Count}");

        System.Windows.MessageBox.Show(
            sb.ToString(),
            "Выгрузка карт",
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);

        StatusText = $"Выгружено {Cards.Count} карт.";
        StatusVisibility = Visibility.Visible;
    }

    private void ClearStorage()
    {
        Cards.Clear();
        _cardStorage.Clear();
        CountText = "0";
        StatusText = "Хранилище карт очищено.";
        StatusVisibility = Visibility.Visible;
    }
}
