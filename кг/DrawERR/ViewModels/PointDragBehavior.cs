using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DrawERR.ViewModels
{
    public static class PointDragBehavior
    {
        public static readonly DependencyProperty IsDraggableProperty =
            DependencyProperty.RegisterAttached("IsDraggable", typeof(bool), typeof(PointDragBehavior),
                new PropertyMetadata(false, OnIsDraggableChanged));

        public static bool GetIsDraggable(DependencyObject obj) => (bool)obj.GetValue(IsDraggableProperty);
        public static void SetIsDraggable(DependencyObject obj, bool value) => obj.SetValue(IsDraggableProperty, value);

        private static readonly DependencyProperty StartMousePositionProperty =
            DependencyProperty.RegisterAttached("StartMousePosition", typeof(Point), typeof(PointDragBehavior), new PropertyMetadata(default(Point)));

        private static readonly DependencyProperty StartPointPositionProperty =
            DependencyProperty.RegisterAttached("StartPointPosition", typeof(Point), typeof(PointDragBehavior), new PropertyMetadata(default(Point)));

        private static readonly DependencyProperty IsDraggingProperty =
            DependencyProperty.RegisterAttached("IsDragging", typeof(bool), typeof(PointDragBehavior), new PropertyMetadata(false));

        private const double DragThreshold = 5.0;

        private static void OnIsDraggableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is Ellipse ellipse)
            {
                if ((bool)e.NewValue)
                {
                    ellipse.MouseLeftButtonDown += Ellipse_MouseLeftButtonDown;
                    ellipse.MouseMove += Ellipse_MouseMove;
                    ellipse.MouseLeftButtonUp += Ellipse_MouseLeftButtonUp;
                    ellipse.Cursor = Cursors.SizeAll;
                }
                else
                {
                    ellipse.MouseLeftButtonDown -= Ellipse_MouseLeftButtonDown;
                    ellipse.MouseMove -= Ellipse_MouseMove;
                    ellipse.MouseLeftButtonUp -= Ellipse_MouseLeftButtonUp;
                    ellipse.Cursor = null;
                }
            }
        }

        private static void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Ellipse ellipse) return;

            var pointVm = ellipse.DataContext as PointViewModel;
            if (pointVm == null) return;

            var canvas = FindParentCanvas(ellipse);
            if (canvas == null) return;

            // Запоминаем начальную позицию мыши
            var startPos = e.GetPosition(canvas);
            ellipse.SetValue(StartMousePositionProperty, startPos);

            // Запоминаем начальную позицию точки
            ellipse.SetValue(StartPointPositionProperty, new Point(pointVm.X, pointVm.Y));

            // Захватываем мышь
            ellipse.CaptureMouse();
        }

        private static void Ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is not Ellipse ellipse) return;
            if (!ellipse.IsMouseCaptured) return;

            var canvas = FindParentCanvas(ellipse);
            if (canvas == null) return;

            var isDragging = (bool)ellipse.GetValue(IsDraggingProperty);

            if (!isDragging)
            {
                var checkStart = (Point)ellipse.GetValue(StartMousePositionProperty);
                var checkCurrent = e.GetPosition(canvas);
                var dx = Math.Abs(checkCurrent.X - checkStart.X);
                var dy = Math.Abs(checkCurrent.Y - checkStart.Y);

                if (dx <= DragThreshold && dy <= DragThreshold)
                    return; // Слишком маленькое движение — это клик

                // Начинаем drag
                ellipse.SetValue(IsDraggingProperty, true);
            }

            // Выполняем drag
            var ptVm = ellipse.DataContext as PointViewModel;
            if (ptVm == null) return;

            var dragStart = (Point)ellipse.GetValue(StartMousePositionProperty);
            var dragPoint = (Point)ellipse.GetValue(StartPointPositionProperty);
            var dragCurrent = e.GetPosition(canvas);

            var deltaX = dragCurrent.X - dragStart.X;
            var deltaY = dragCurrent.Y - dragStart.Y;

            ptVm.X = dragPoint.X + deltaX;
            ptVm.Y = dragPoint.Y + deltaY;
        }

        private static void Ellipse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Ellipse ellipse) return;

            var pointVm = ellipse.DataContext as PointViewModel;
            if (pointVm == null) return;

            var isDragging = (bool)ellipse.GetValue(IsDraggingProperty);

            // Если drag не начинался — это был клик, выделяем точку
            if (!isDragging)
            {
                var shapeVm = FindShapeViewModel(ellipse);
                if (shapeVm == null) return;

                var mainVm = FindMainViewModel(ellipse);
                if (mainVm == null) return;

                int index = shapeVm.PointViewModels.IndexOf(pointVm);
                if (index < 0) return;

                // Сбрасываем выделение всех фигур
                foreach (var s in mainVm.SelectedShapes)
                    s.IsSelected = false;
                mainVm.SelectedShapes.Clear();

                // Выделяем точку
                shapeVm.TogglePointSelection(index);
                shapeVm.IsSelected = true;
                mainVm.SelectedShapes.Add(shapeVm);
                mainVm.SelectedShape = shapeVm;
            }
            else
            {
                // Завершаем drag
                ellipse.SetValue(IsDraggingProperty, false);
            }

            ellipse.ReleaseMouseCapture();
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

        private static Canvas FindParentCanvas(DependencyObject obj)
        {
            var current = obj;
            while (current != null)
            {
                if (current is Canvas canvas) return canvas;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
