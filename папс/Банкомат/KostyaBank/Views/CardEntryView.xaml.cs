using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KostyaBankApp.ViewModels;

namespace KostyaBankApp.Views;

/// <summary>
/// Interaction logic for CardEntryView.xaml
/// </summary>
public partial class CardEntryView : UserControl
{
    public CardEntryView()
    {
        InitializeComponent();
        DataObject.AddPastingHandler(CardNumberTextBox, CardNumberTextBox_Pasting);
        DataObject.AddPastingHandler(PinCodeTextBox, PinCodeTextBox_Pasting);
    }

    private void CardNumberTextBox_Loaded(object sender, RoutedEventArgs e)
    {
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

    private void PinCodeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^\d$");
    }

    private void PinCodeTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
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
