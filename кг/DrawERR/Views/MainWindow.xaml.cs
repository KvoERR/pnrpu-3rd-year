using DrawERR.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DrawERR.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ItemsControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var itemsControl = sender as ItemsControl;
            var viewModel = itemsControl?.DataContext as MainViewModel;
            if (viewModel == null) return;

            // Определяем элемент под курсором
            var clickedElement = e.OriginalSource as DependencyObject;
            if (clickedElement == null) return;

            // Проверяем, попали ли мы в элемент фигуры (Canvas с фигурой)
            var shapeCanvas = GetParentByType(clickedElement, typeof(Canvas));
            if (shapeCanvas == null) return;

            // Ищем ShapeViewModel в визуальном дереве
            var shapeViewModel = FindParentWithDataContext<ShapeViewModel>(shapeCanvas);
            if (shapeViewModel != null)
            {
                e.Handled = true;
                viewModel.ToggleShapeSelection(shapeViewModel);
            }
        }

        private DependencyObject GetParentByType(DependencyObject child, Type targetType)
        {
            var current = child;
            while (current != null)
            {
                if (current.GetType() == targetType)
                    return current;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private T FindParentWithDataContext<T>(DependencyObject child) where T : class
        {
            var current = child;
            while (current != null)
            {
                if (current is FrameworkElement fe && fe.DataContext is T)
                    return fe.DataContext as T;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
