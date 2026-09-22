using DrawERR.Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace DrawERR.ViewModels
{
    public class ShapeViewModel : INotifyPropertyChanged
    {
        private ShapeItem _model;
        private bool _isSelected;
        private MainViewModel _mainViewModel;

        public ShapeViewModel(ShapeItem model)
        {
            _model = model;
        }

        public ShapeItem Shape => _model;

        public MainViewModel MainViewModel
        {
            get => _mainViewModel;
            set
            {
                _mainViewModel = value;
                OnPropertyChanged();
            }
        }

        public string Id => _model.Id;
        public ShapeType Type => _model.Type;

        // Трансформированные точки (для ItemsControl — кружки)
        public PointCollection TransformedPoints
        {
            get
            {
                var pts = _model.GetTransformedPoints();
                var collection = new PointCollection(pts.Length);
                foreach (var p in pts)
                    collection.Add(p);
                return collection;
            }
        }

        // Для замкнутых фигур (3+ точек) — добавляем первую точку в конец
        public PointCollection ClosedPoints
        {
            get
            {
                var pts = _model.GetTransformedPoints();
                int count = pts.Length >= 3 ? pts.Length + 1 : pts.Length;
                var collection = new PointCollection(count);
                foreach (var p in pts)
                    collection.Add(p);
                if (pts.Length >= 3 && pts.Length > 0)
                    collection.Add(pts[0]);
                return collection;
            }
        }

        // Координаты (для удобства)
        public double X
        {
            get => _model.TransformMatrix.OffsetX;
            set
            {
                var matrix = _model.TransformMatrix;
                matrix.OffsetX = value;
                _model.TransformMatrix = matrix;
                NotifyTransformChanged();
                OnPropertyChanged();
            }
        }

        public double Y
        {
            get => _model.TransformMatrix.OffsetY;
            set
            {
                var matrix = _model.TransformMatrix;
                matrix.OffsetY = value;
                _model.TransformMatrix = matrix;
                NotifyTransformChanged();
                OnPropertyChanged();
            }
        }

        // Угол поворота (в градусах)
        public double RotationAngle
        {
            get
            {
                var matrix = _model.TransformMatrix;
                return Math.Atan2(matrix.M12, matrix.M11) * 180.0 / Math.PI;
            }
            set
            {
                var currentAngle = RotationAngle;
                double delta = value - currentAngle;
                if (Math.Abs(delta) > 0.001)
                {
                    _model.Rotate(delta);
                    NotifyTransformChanged();
                }
                OnPropertyChanged();
            }
        }

        // Масштаб
        public double ScaleFactor
        {
            get
            {
                var matrix = _model.TransformMatrix;
                return Math.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12);
            }
            set
            {
                if (value <= 0.01) return;
                var matrix = _model.TransformMatrix;
                var center = _model.GetCenter();
                matrix.Translate(-center.X, -center.Y);
                matrix.Scale(value, value);
                matrix.Translate(center.X, center.Y);
                _model.TransformMatrix = matrix;
                NotifyTransformChanged();
                OnPropertyChanged();
            }
        }

        // Свойства для отображения
        public Brush Fill
        {
            get => _model.Fill;
            set
            {
                _model.Fill = value;
                OnPropertyChanged();
            }
        }

        public Brush Stroke
        {
            get => _model.Stroke;
            set
            {
                _model.Stroke = value;
                OnPropertyChanged();
            }
        }

        public double StrokeThickness
        {
            get => _model.StrokeThickness;
            set
            {
                _model.StrokeThickness = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                _model.IsSelected = value;
                OnPropertyChanged();
            }
        }

        public Rect Bounds => _model.GetBounds();

        public string DisplayName
        {
            get
            {
                return Type switch
                {
                    ShapeType.Point => $"Точка ({X:F0}, {Y:F0})",
                    ShapeType.Line => $"Прямая ({X:F0}, {Y:F0})",
                    ShapeType.Triangle => $"Треугольник ({X:F0}, {Y:F0})",
                    _ => $"{Type} ({X:F0}, {Y:F0})"
                };
            }
        }

        /// <summary>
        /// Вызывается при любых трансформациях фигуры (Move, Rotate, Scale).
        /// Уведомляет UI об изменении всех зависимых свойств.
        /// </summary>
        public void NotifyTransformChanged()
        {
            OnPropertyChanged(nameof(TransformedPoints));
            OnPropertyChanged(nameof(ClosedPoints));
            OnPropertyChanged(nameof(Bounds));
            OnPropertyChanged(nameof(DisplayName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
