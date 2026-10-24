using System.Windows;
using KostyaBankApp.ViewModels;

namespace KostyaBankApp.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainMenuViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
