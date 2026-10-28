using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace KostyaBankApp.Behaviors;

/// <summary>
/// Attached behavior for restricting TextBox input to digits only.
/// </summary>
public static class TextBoxValidation
{
    public static readonly DependencyProperty AllowOnlyDigitsProperty =
        DependencyProperty.RegisterAttached(
            "AllowOnlyDigits",
            typeof(bool),
            typeof(TextBoxValidation),
            new PropertyMetadata(false, OnAllowOnlyDigitsChanged));

    public static bool GetAllowOnlyDigits(DependencyObject obj) => (bool)obj.GetValue(AllowOnlyDigitsProperty);
    public static void SetAllowOnlyDigits(DependencyObject obj, bool value) => obj.SetValue(AllowOnlyDigitsProperty, value);

    private static void OnAllowOnlyDigitsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox)
        {
            bool allow = (bool)e.NewValue;
            if (allow)
            {
                textBox.PreviewTextInput += TextBox_PreviewTextInput;
                DataObject.AddPastingHandler(textBox, TextBox_Pasting);
            }
            else
            {
                textBox.PreviewTextInput -= TextBox_PreviewTextInput;
                DataObject.RemovePastingHandler(textBox, TextBox_Pasting);
            }
        }
    }

    private static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^\d$");
    }

    private static void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
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
