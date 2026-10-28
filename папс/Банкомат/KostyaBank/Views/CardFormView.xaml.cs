using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KostyaBankApp.ViewModels;

namespace KostyaBankApp.Views;

/// <summary>
/// Base user control for card operation forms.
/// Provides card info, action button, print checkbox, and message area.
/// Derived views set ContentTemplate and HeaderTitle.
/// </summary>
public partial class CardFormView : UserControl
{
    public static readonly DependencyProperty HeaderTitleProperty =
        DependencyProperty.Register(
            nameof(HeaderTitle),
            typeof(string),
            typeof(CardFormView),
            new PropertyMetadata(string.Empty));

    public string HeaderTitle
    {
        get => (string)GetValue(HeaderTitleProperty);
        set => SetValue(HeaderTitleProperty, value);
    }

    public static readonly DependencyProperty ContentTemplateProperty =
        DependencyProperty.Register(
            nameof(ContentTemplate),
            typeof(DataTemplate),
            typeof(CardFormView),
            new PropertyMetadata(null));

    public DataTemplate? ContentTemplate
    {
        get => (DataTemplate?)GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    public CardFormView()
    {
        InitializeComponent();
    }
}
