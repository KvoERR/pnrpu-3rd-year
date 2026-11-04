using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DrawERR.ViewModels
{
    public static class FigureSelectionBehavior
    {
        public static readonly DependencyProperty IsSelectableProperty =
            DependencyProperty.RegisterAttached("IsSelectable", typeof(bool), typeof(FigureSelectionBehavior),
                new PropertyMetadata(false, OnIsSelectableChanged));

        public static bool GetIsSelectable(DependencyObject obj) => (bool)obj.GetValue(IsSelectableProperty);
        public static void SetIsSelectable(DependencyObject obj, bool value) => obj.SetValue(IsSelectableProperty, value);

        private static void OnIsSelectableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is Rectangle rectangle)
            {
                if ((bool)e.NewValue)
                {
                    rectangle.MouseLeftButtonDown += Rectangle_MouseLeftButtonDown;
                    rectangle.Cursor = Cursors.Hand;
                }
                else
                {
                    rectangle.MouseLeftButtonDown -= Rectangle_MouseLeftButtonDown;
                    rectangle.Cursor = null;
                }
            }
        }

        private static void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Rectangle rectangle) return;

            var mainVm = FindMainViewModel(rectangle);
            if (mainVm == null) return;

            // Сначала ищем GroupViewModel
            var groupVm = FindGroupViewModel(rectangle);
            if (groupVm != null)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    // Ctrl+Click — переключаем группу
                    if (mainVm.SelectedGroup == groupVm)
                    {
                        // Убираем все дочерние фигуры из выделения
                        foreach (var child in groupVm.ChildShapes)
                        {
                            child.IsSelected = false;
                            mainVm.SelectedShapes.Remove(child);
                        }
                        groupVm.IsSelected = false;
                        mainVm.SelectedGroup = null;
                        if (mainVm.SelectedShapes.Count > 0)
                            mainVm.SelectedShape = mainVm.SelectedShapes.Last();
                        else
                            mainVm.SelectedShape = null;
                    }
                    else
                    {
                        // Добавляем группу
                        groupVm.IsSelected = true;
                        mainVm.SelectedGroup = groupVm;
                        foreach (var child in groupVm.ChildShapes)
                        {
                            child.IsSelected = true;
                            mainVm.SelectedShapes.Add(child);
                        }
                        mainVm.SelectedShape = mainVm.SelectedShapes.Last();
                    }
                }
                else
                {
                    // Без Ctrl — одиночное выделение группы
                    foreach (var s in mainVm.Shapes) s.IsSelected = false;
                    mainVm.SelectedShapes.Clear();
                    foreach (var g in mainVm.Groups) g.IsSelected = false;
                    mainVm.SelectedGroup = null;

                    groupVm.IsSelected = true;
                    mainVm.SelectedGroup = groupVm;
                    foreach (var child in groupVm.ChildShapes)
                    {
                        child.IsSelected = true;
                        mainVm.SelectedShapes.Add(child);
                    }
                    mainVm.SelectedShape = mainVm.SelectedShapes.Count > 0 ? mainVm.SelectedShapes.Last() : null;
                }
                return;
            }

            // Ищем ShapeViewModel
            var shapeVm = FindShapeViewModel(rectangle);
            if (shapeVm == null) return;

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (mainVm.SelectedShapes.Contains(shapeVm))
                {
                    mainVm.SelectedShapes.Remove(shapeVm);
                    shapeVm.IsSelected = false;
                    if (mainVm.SelectedShapes.Count > 0)
                        mainVm.SelectedShape = mainVm.SelectedShapes.Last();
                    else
                        mainVm.SelectedShape = null;
                }
                else
                {
                    shapeVm.IsSelected = true;
                    mainVm.SelectedShapes.Add(shapeVm);
                    mainVm.SelectedShape = shapeVm;
                }
            }
            else
            {
                foreach (var s in mainVm.Shapes) s.IsSelected = false;
                mainVm.SelectedShapes.Clear();
                shapeVm.SelectedPointIndex = -1;
                shapeVm.IsSelected = true;
                mainVm.SelectedShapes.Add(shapeVm);
                mainVm.SelectedShape = shapeVm;
            }
        }

        private static ShapeViewModel FindShapeViewModel(DependencyObject obj)
        {
            var current = obj;
            while (current != null)
            {
                if (current is FrameworkElement fe)
                {
                    if (fe.DataContext is ShapeViewModel vm) return vm;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private static GroupViewModel FindGroupViewModel(DependencyObject obj)
        {
            var current = obj;
            while (current != null)
            {
                if (current is FrameworkElement fe)
                {
                    if (fe.DataContext is GroupViewModel vm) return vm;
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
