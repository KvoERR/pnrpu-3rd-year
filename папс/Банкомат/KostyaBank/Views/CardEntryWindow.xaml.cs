using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KostyaBankApp.Views;

/// <summary>
/// Interaction logic for CardEntryWindow.xaml
/// </summary>
public partial class CardEntryWindow : Window
{
    public CardEntryWindow()
    {
        InitializeComponent();
        CardNumberTextBox.Focus();
    }

    private void CardNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^\d$");
    }

    private void CardNumberTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            var text = (string)e.DataObject.GetData(typeof(string));
            if (!System.Text.RegularExpressions.Regex.IsMatch(text, @"^\d+$"))
                e.CancelCommand();
        }
        else
        {
            e.CancelCommand();
        }
    }
}
