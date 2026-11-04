using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DrawERR.ViewModels
{
    public static class VertexSelectionBehavior
    {
        public static readonly DependencyProperty IsSelectableProperty =
            DependencyProperty.RegisterAttached("IsSelectable", typeof(bool), typeof(VertexSelectionBehavior),
                new PropertyMetadata(false, OnIsSelectableChanged));

        public static bool GetIsSelectable(DependencyObject obj) => (bool)obj.GetValue(IsSelectableProperty);
        public static void SetIsSelectable(DependencyObject obj, bool value) => obj.SetValue(IsSelectableProperty, value);

        private static void OnIsSelectableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is Ellipse ellipse)
            {
                if ((bool)e.NewValue)
                {
                    ellipse.MouseLeftButtonDown += Ellipse_MouseLeftButtonDown;
                    ellipse.Cursor = Cursors.SizeAll;
                }
                else
                {
                    ellipse.MouseLeftButtonDown -= Ellipse_MouseLeftButtonDown;
                    ellipse.Cursor = null;
                }
            }
        }

        private static void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Ellipse ellipse) return;

            var pointVm = ellipse.DataContext as PointViewModel;
            if (pointVm == null) return;

            // Ищем ShapeViewModel через DataContext
            var shapeVm = FindShapeViewModel(ellipse);
            if (shapeVm == null) return;

            var mainVm = FindMainViewModel(ellipse);
            if (mainVm == null) return;

            int index = shapeVm.PointViewModels.IndexOf(pointVm);
            if (index < 0) return;

            // Отменяем выделение фигуры
            foreach (var s in mainVm.SelectedShapes)
                s.IsSelected = false;
            mainVm.SelectedShapes.Clear();

            // Выделяем точку
            shapeVm.TogglePointSelection(index);
            shapeVm.IsSelected = true;
            mainVm.SelectedShapes.Add(shapeVm);
            mainVm.SelectedShape = shapeVm;
        }

        private static ShapeViewModel FindShapeViewModel(DependencyObject obj)
        {
            var current = obj;
            while (current != null)
            {
                if (current is FrameworkElement fe)
                {
                    if (fe.DataContext is ShapeViewModel vm) return vm;
                    if (fe.Parent is ItemsControl ic && ic.DataContext is ShapeViewModel vm2) return vm2;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private static MainViewModel FindMainViewModel(DependencyObject obj)
        {
            var current = obj;
            while (current != null)
            {
                if (current is FrameworkElement fe)
                {
                    if (fe.DataContext is MainViewModel vm) return vm;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
