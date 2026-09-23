using DrawERR.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;

namespace DrawERR.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ObservableCollection<ShapeViewModel> _shapes;
        private readonly ObservableCollection<ShapeViewModel> _selectedShapes;
        private ShapeViewModel _selectedShape;
        private ShapeType _selectedShapeType;
        private static readonly Random _random = new Random();

        public MainViewModel()
        {
            _shapes = new ObservableCollection<ShapeViewModel>();
            _selectedShapes = new ObservableCollection<ShapeViewModel>();
            _selectedShapeType = ShapeType.Point;

            // Команды
            AddShapeCommand = new RelayCommand(AddShape);
            DeleteShapeCommand = new RelayCommand(DeleteShape, param => _selectedShapes.Count > 0);
            SaveCommand = new RelayCommand(Save);
            LoadCommand = new RelayCommand(Load);
            ClearCommand = new RelayCommand(Clear);
            ToggleSelectionCommand = new RelayCommand<ShapeViewModel>(ToggleShapeSelection);

            // Трансформации
            MoveLeftCommand = new RelayCommand(MoveLeft, param => _selectedShapes.Count > 0);
            MoveRightCommand = new RelayCommand(MoveRight, param => _selectedShapes.Count > 0);
            MoveUpCommand = new RelayCommand(MoveUp, param => _selectedShapes.Count > 0);
            MoveDownCommand = new RelayCommand(MoveDown, param => _selectedShapes.Count > 0);
            RotateLeftCommand = new RelayCommand(RotateLeft, param => _selectedShapes.Count > 0);
            RotateRightCommand = new RelayCommand(RotateRight, param => _selectedShapes.Count > 0);
            ScaleUpCommand = new RelayCommand(ScaleUp, param => _selectedShapes.Count > 0);
            ScaleDownCommand = new RelayCommand(ScaleDown, param => _selectedShapes.Count > 0);
            DeselectAllCommand = new RelayCommand(DeselectAll);
        }

        public ObservableCollection<ShapeViewModel> Shapes => _shapes;
        public IEnumerable<ShapeType> ShapeTypes => Enum.GetValues(typeof(ShapeType)).Cast<ShapeType>();

        public ShapeType SelectedShapeType
        {
            get => _selectedShapeType;
            set
            {
                _selectedShapeType = value;
                OnPropertyChanged();
            }
        }

        public ShapeViewModel SelectedShape
        {
            get => _selectedShape;
            set
            {
                if (_selectedShape != value)
                {
                    _selectedShape = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<ShapeViewModel> SelectedShapes => _selectedShapes;

        public ICommand AddShapeCommand { get; }
        public ICommand DeleteShapeCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand ToggleSelectionCommand { get; }

        public ICommand MoveLeftCommand { get; }
        public ICommand MoveRightCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand RotateLeftCommand { get; }
        public ICommand RotateRightCommand { get; }
        public ICommand ScaleUpCommand { get; }
        public ICommand ScaleDownCommand { get; }
        public ICommand DeselectAllCommand { get; }

        // ========== ДОБАВЛЕНИЕ ФИГУР ==========
        private void AddShape(object parameter)
        {
            ShapeItem shape = SelectedShapeType switch
            {
                ShapeType.Point => new PointShape(),
                ShapeType.Line => new LineShape(),
                ShapeType.Triangle => new TriangleShape(),
                _ => new PointShape()
            };

            // Случайная позиция
            var matrix = shape.TransformMatrix;
            matrix.OffsetX = _random.Next(50, 750);
            matrix.OffsetY = _random.Next(50, 550);
            shape.TransformMatrix = matrix;

            // Случайный цвет для треугольника
            if (shape is TriangleShape)
            {
                shape.Fill = GetRandomColor();
            }

            var viewModel = new ShapeViewModel(shape);
            viewModel.MainViewModel = this;
            _shapes.Add(viewModel);
            _selectedShapes.Add(viewModel);
            viewModel.IsSelected = true;
            SelectedShape = viewModel;
        }

        // ========== ТРАНСФОРМАЦИИ ==========
        private void MoveLeft(object parameter)
        {
            foreach (var shape in _selectedShapes)
                shape.X -= 10;
        }

        private void MoveRight(object parameter)
        {
            foreach (var shape in _selectedShapes)
                shape.X += 10;
        }

        private void MoveUp(object parameter)
        {
            foreach (var shape in _selectedShapes)
                shape.Y -= 10;
        }

        private void MoveDown(object parameter)
        {
            foreach (var shape in _selectedShapes)
                shape.Y += 10;
        }

        private void RotateLeft(object parameter)
        {
            foreach (var shape in _selectedShapes)
            {
                shape.Shape.Rotate(-15);
                shape.NotifyTransformChanged();
            }
        }

        private void RotateRight(object parameter)
        {
            foreach (var shape in _selectedShapes)
            {
                shape.Shape.Rotate(15);
                shape.NotifyTransformChanged();
            }
        }

        private void ScaleUp(object parameter)
        {
            foreach (var shape in _selectedShapes)
            {
                shape.Shape.Scale(1.2);
                shape.NotifyTransformChanged();
            }
        }

        private void ScaleDown(object parameter)
        {
            foreach (var shape in _selectedShapes)
            {
                shape.Shape.Scale(0.8);
                shape.NotifyTransformChanged();
            }
        }

        private void DeselectAll(object parameter)
        {
            foreach (var shape in _selectedShapes)
                shape.IsSelected = false;
            _selectedShapes.Clear();
            SelectedShape = null;
        }

        public void ToggleShapeSelection(ShapeViewModel shape)
        {
            if (shape == null) return;

            if (_selectedShapes.Contains(shape))
                _selectedShapes.Remove(shape);
            else
                _selectedShapes.Add(shape);

            shape.IsSelected = _selectedShapes.Contains(shape);
            SelectedShape = _selectedShapes.Count > 0 ? _selectedShapes.Last() : null;
        }

        // ========== ОСТАЛЬНЫЕ МЕТОДЫ ==========
        private void DeleteShape(object parameter)
        {
            var toRemove = _selectedShapes.ToList();
            foreach (var shape in toRemove)
                _shapes.Remove(shape);
            _selectedShapes.Clear();
            SelectedShape = null;
        }

        private void Save(object parameter)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "XML-файл фигур|*.drawerr",
                DefaultExt = ".drawerr",
                FileName = "figures"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var dtos = _shapes.Select(sv => ShapeDto.FromViewModel(sv)).ToList();
                    var serializer = new XmlSerializer(typeof(List<ShapeDto>));
                    using var writer = new StreamWriter(dlg.FileName);
                    serializer.Serialize(writer, dtos);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Load(object parameter)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "XML-файл фигур|*.drawerr",
                DefaultExt = ".drawerr"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(List<ShapeDto>));
                    using var reader = new StreamReader(dlg.FileName);
                    var dtos = (List<ShapeDto>)serializer.Deserialize(reader);

                    _shapes.Clear();
                    foreach (var dto in dtos)
                    {
                        var shape = dto.ToShapeItem();
                        var vm = new ShapeViewModel(shape);
                        vm.MainViewModel = this;
                        _shapes.Add(vm);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Clear(object parameter)
        {
            _shapes.Clear();
            foreach (var shape in _selectedShapes.ToList())
                shape.IsSelected = false;
            _selectedShapes.Clear();
            SelectedShape = null;
        }

        private Brush GetRandomColor()
        {
            var colors = new[]
            {
                Colors.LightBlue, Colors.LightGreen, Colors.LightPink,
                Colors.LightYellow, Colors.LightCoral, Colors.LightSalmon,
                Colors.LightSkyBlue, Colors.LightGoldenrodYellow
            };
            return new SolidColorBrush(colors[_random.Next(colors.Length)]);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // ========== DTO для сериализации ==========
    [XmlRoot("Shape")]
    public class ShapeDto
    {
        public string Id { get; set; }
        public ShapeType Type { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public double RotationAngle { get; set; }
        public double ScaleFactor { get; set; }
        public string FillColor { get; set; }
        public string StrokeColor { get; set; }
        public double StrokeThickness { get; set; }
        public PointDto[] Points { get; set; }

        public static ShapeDto FromViewModel(ShapeViewModel vm)
        {
            var shape = vm.Shape;
            var matrix = shape.TransformMatrix;

            // Вычисляем итоговый угол поворота и масштаб из матрицы
            double angle = Math.Atan2(matrix.M12, matrix.M11) * 180.0 / Math.PI;
            double scale = Math.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12);

            return new ShapeDto
            {
                Id = shape.Id,
                Type = shape.Type,
                OffsetX = matrix.OffsetX,
                OffsetY = matrix.OffsetY,
                RotationAngle = angle,
                ScaleFactor = scale,
                FillColor = ColorToHex(((SolidColorBrush)shape.Fill)?.Color ?? Colors.Transparent),
                StrokeColor = ColorToHex(((SolidColorBrush)shape.Stroke)?.Color ?? Colors.Black),
                StrokeThickness = shape.StrokeThickness,
                Points = shape.Points.Select(p => new PointDto { X = p.X, Y = p.Y }).ToArray()
            };
        }

        public ShapeItem ToShapeItem()
        {
            ShapeItem shape = Type switch
            {
                ShapeType.Point => new PointShape(),
                ShapeType.Line => new LineShape(),
                ShapeType.Triangle => new TriangleShape(),
                _ => new PointShape()
            };

            // Восстанавливаем точки
            if (Points != null && Points.Length > 0)
            {
                shape.Points = Points.Select(p => new Point(p.X, p.Y)).ToArray();
            }

            // Восстанавливаем цвета
            shape.Fill = new SolidColorBrush(HexToColor(FillColor));
            shape.Stroke = new SolidColorBrush(HexToColor(StrokeColor));
            shape.StrokeThickness = StrokeThickness;

            // Восстанавливаем трансформацию
            // Сначала устанавливаем локальные точки
            if (Points != null && Points.Length > 0)
            {
                shape.Points = Points.Select(p => new Point(p.X, p.Y)).ToArray();
            }

            // Вычисляем центр фигуры в локальных координатах
            var localCenter = shape.GetCenter();

            // Строим матрицу: сдвиг к позиции на холсте, поворот, масштаб
            var matrix = new Matrix();
            matrix.OffsetX = OffsetX;
            matrix.OffsetY = OffsetY;

            // Поворот вокруг центра фигуры
            if (RotationAngle != 0)
            {
                var rotMatrix = new Matrix();
                rotMatrix.Translate(-localCenter.X, -localCenter.Y);
                rotMatrix.Rotate(RotationAngle);
                rotMatrix.Translate(localCenter.X, localCenter.Y);
                matrix = Matrix.Multiply(matrix, rotMatrix);
            }

            // Масштабирование от центра
            if (ScaleFactor != 1.0)
            {
                var scaleMatrix = new Matrix();
                scaleMatrix.Translate(-localCenter.X, -localCenter.Y);
                scaleMatrix.Scale(ScaleFactor, ScaleFactor);
                scaleMatrix.Translate(localCenter.X, localCenter.Y);
                matrix = Matrix.Multiply(matrix, scaleMatrix);
            }

            shape.TransformMatrix = matrix;

            return shape;
        }

        private static string ColorToHex(Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private static Color HexToColor(string hex)
        {
            if (string.IsNullOrEmpty(hex) || hex.Length < 7)
                return Colors.Black;

            hex = hex.TrimStart('#');
            if (hex.Length == 6) hex = "FF" + hex;

            try
            {
                return Color.FromArgb(
                    byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                    byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                    byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber),
                    byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber)
                );
            }
            catch
            {
                return Colors.Black;
            }
        }
    }

    [XmlRoot("Point")]
    public class PointDto
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
