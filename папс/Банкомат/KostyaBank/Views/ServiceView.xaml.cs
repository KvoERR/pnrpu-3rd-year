using System.Windows;
using KostyaBankApp.ViewModels;

namespace KostyaBankApp.Views;

/// <summary>
/// Interaction logic for ServiceView.
/// </summary>
public partial class ServiceView : Window
{
    public ServiceView(ServiceViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
