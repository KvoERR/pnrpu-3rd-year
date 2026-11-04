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
        private readonly ObservableCollection<GroupViewModel> _groups;
        private readonly ObservableCollection<ShapeViewModel> _selectedShapes;
        private ShapeViewModel _selectedShape;
        private GroupViewModel _selectedGroup;
        private ShapeType _selectedShapeType;
        private static readonly Random _random = new Random();

        private double _moveX;
        private double _moveY;
        private double _moveZ;
        private double _rotateAngle;
        private double _rotateY;
        private double _scaleX;
        private double _scaleY;
        private double _scaleZ;

        public MainViewModel()
        {
            _shapes = new ObservableCollection<ShapeViewModel>();
            _groups = new ObservableCollection<GroupViewModel>();
            _selectedShapes = new ObservableCollection<ShapeViewModel>();
            _selectedShapeType = ShapeType.Point;

            AddShapeCommand = new RelayCommand(AddShape);
            DeleteShapeCommand = new RelayCommand(DeleteShape, param => _selectedShapes.Count > 0);
            SaveCommand = new RelayCommand(Save);
            LoadCommand = new RelayCommand(Load);
            ClearCommand = new RelayCommand(Clear);
            ToggleSelectionCommand = new RelayCommand<ShapeViewModel>(ToggleShapeSelection);

            MoveLeftCommand = new RelayCommand(MoveLeft, param => _selectedShapes.Count > 0);
            MoveRightCommand = new RelayCommand(MoveRight, param => _selectedShapes.Count > 0);
            MoveUpCommand = new RelayCommand(MoveUp, param => _selectedShapes.Count > 0);
            MoveDownCommand = new RelayCommand(MoveDown, param => _selectedShapes.Count > 0);
            RotateLeftCommand = new RelayCommand(RotateLeft, param => _selectedShapes.Count > 0);
            RotateRightCommand = new RelayCommand(RotateRight, param => _selectedShapes.Count > 0);
            ScaleUpCommand = new RelayCommand(ScaleUp, param => _selectedShapes.Count > 0);
            ScaleDownCommand = new RelayCommand(ScaleDown, param => _selectedShapes.Count > 0);
            MirrorXCommand = new RelayCommand(MirrorX, param => _selectedShapes.Count > 0);
            MirrorYCommand = new RelayCommand(MirrorY, param => _selectedShapes.Count > 0);
            ShearXCommand = new RelayCommand(ShearX, param => _selectedShapes.Count > 0);
            ShearYCommand = new RelayCommand(ShearY, param => _selectedShapes.Count > 0);
            GroupCommand = new RelayCommand(Group, param => _selectedShapes.Count >= 2);
            UngroupCommand = new RelayCommand(Ungroup, param => _selectedGroup != null);
            DeselectAllCommand = new RelayCommand(DeselectAll);

            MoveXCommand = new RelayCommand(MoveXByAmount, param => _selectedShapes.Count > 0);
            MoveYCommand = new RelayCommand(MoveYByAmount, param => _selectedShapes.Count > 0);
            MoveZCommand = new RelayCommand(MoveZByAmount, param => _selectedShapes.Count > 0);
            RotateAngleCommand = new RelayCommand(RotateByAngle, param => _selectedShapes.Count > 0);
            RotateYCommand = new RelayCommand(RotateYByAngle, param => _selectedShapes.Count > 0);
            ScaleXCommand = new RelayCommand(ScaleXByAmount, param => _selectedShapes.Count > 0);
            ScaleYCommand = new RelayCommand(ScaleYByAmount, param => _selectedShapes.Count > 0);
            ScaleZCommand = new RelayCommand(ScaleZByAmount, param => _selectedShapes.Count > 0);
        }

        public double MoveX
        {
            get => _moveX;
            set { _moveX = value; OnPropertyChanged(); }
        }

        public double MoveY
        {
            get => _moveY;
            set { _moveY = value; OnPropertyChanged(); }
        }

        public double MoveZ
        {
            get => _moveZ;
            set { _moveZ = value; OnPropertyChanged(); }
        }

        public double RotateAngle
        {
            get => _rotateAngle;
            set { _rotateAngle = value; OnPropertyChanged(); }
        }

        public double RotateY
        {
            get => _rotateY;
            set { _rotateY = value; OnPropertyChanged(); }
        }

        public double ScaleX
        {
            get => _scaleX;
            set { _scaleX = value; OnPropertyChanged(); }
        }

        public double ScaleY
        {
            get => _scaleY;
            set { _scaleY = value; OnPropertyChanged(); }
        }

        public double ScaleZ
        {
            get => _scaleZ;
            set { _scaleZ = value; OnPropertyChanged(); }
        }

        public ICommand MoveXCommand { get; }
        public ICommand MoveYCommand { get; }
        public ICommand MoveZCommand { get; }
        public ICommand RotateAngleCommand { get; }
        public ICommand RotateYCommand { get; }
        public ICommand ScaleXCommand { get; }
        public ICommand ScaleYCommand { get; }
        public ICommand ScaleZCommand { get; }

        public ObservableCollection<ShapeViewModel> Shapes => _shapes;
        public ObservableCollection<GroupViewModel> Groups => _groups;
        public IEnumerable<ShapeType> ShapeTypes => Enum.GetValues(typeof(ShapeType)).Cast<ShapeType>();

        public ShapeType SelectedShapeType
        {
            get => _selectedShapeType;
            set { _selectedShapeType = value; OnPropertyChanged(); }
        }

        public ShapeViewModel SelectedShape
        {
            get => _selectedShape;
            set { _selectedShape = value; OnPropertyChanged(); }
        }

        public GroupViewModel SelectedGroup
        {
            get => _selectedGroup;
            set { _selectedGroup = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedItemDisplayName)); }
        }

        public string SelectedItemDisplayName
        {
            get
            {
                if (_selectedGroup != null) return _selectedGroup.DisplayName;
                if (_selectedShape != null) return _selectedShape.DisplayName;
                return "Не выбрано";
            }
        }

        public ObservableCollection<ShapeViewModel> SelectedShapes => _selectedShapes;

        // Bounding box всех выделенных фигур
        public Rect SelectedBounds
        {
            get
            {
                if (_selectedShapes.Count == 0) return Rect.Empty;
                double minX = double.MaxValue, maxX = double.MinValue;
                double minY = double.MaxValue, maxY = double.MinValue;
                foreach (var s in _selectedShapes)
                {
                    var b = s.Shape.GetBounds();
                    if (b.X < minX) minX = b.X;
                    if (b.Right > maxX) maxX = b.Right;
                    if (b.Y < minY) minY = b.Y;
                    if (b.Bottom > maxY) maxY = b.Bottom;
                }
                return new Rect(minX, minY, maxX - minX, maxY - minY);
            }
        }

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
        public ICommand MirrorXCommand { get; }
        public ICommand MirrorYCommand { get; }
        public ICommand ShearXCommand { get; }
        public ICommand ShearYCommand { get; }
        public ICommand GroupCommand { get; }
        public ICommand UngroupCommand { get; }
        public ICommand DeselectAllCommand { get; }

        private void AddShape(object parameter)
        {
            ShapeItem shape = SelectedShapeType switch
            {
                ShapeType.Point => new PointShape(),
                ShapeType.Line => new LineShape(),
                ShapeType.Triangle => new TriangleShape(),
                _ => new PointShape()
            };

            var offsetX = _random.Next(50, 750);
            var offsetY = _random.Next(50, 550);
            foreach (var p in shape.Points)
                p.Position = new Point(p.Position.X + offsetX, p.Position.Y + offsetY);

            if (shape is TriangleShape)
            {
                var colors = new[] { Colors.Orange, Colors.Gold, Colors.Goldenrod, Colors.DarkOrange, Colors.SandyBrown };
                foreach (var p in shape.Points)
                    p.Fill = new SolidColorBrush(colors[_random.Next(colors.Length)]);
            }

            var vm = new ShapeViewModel(shape);
            vm.Initialize();
            _shapes.Add(vm);
            _selectedShapes.Add(vm);
            vm.IsSelected = true;
            SelectedShape = vm;
        }

        private void MoveLeft(object parameter)
        {
            foreach (var shape in _selectedShapes) MoveShape(shape, -10, 0);
        }

        private void MoveRight(object parameter)
        {
            foreach (var shape in _selectedShapes) MoveShape(shape, 10, 0);
        }

        private void MoveUp(object parameter)
        {
            foreach (var shape in _selectedShapes) MoveShape(shape, 0, -10);
        }

        private void MoveDown(object parameter)
        {
            foreach (var shape in _selectedShapes) MoveShape(shape, 0, 10);
        }

        private void MoveXByAmount(object parameter)
        {
            foreach (var shape in _selectedShapes) MoveShape(shape, _moveX, 0);
        }

        private void MoveYByAmount(object parameter)
        {
            foreach (var shape in _selectedShapes) MoveShape(shape, 0, _moveY);
        }

        private void MoveZByAmount(object parameter)
        {
            const double FocalLength = 500;
            foreach (var shape in _selectedShapes)
            {
                foreach (var pt in shape.PointViewModels)
                {
                    double zOld = pt.Z;
                    pt.Z += _moveZ;
                    if (pt.Z < -490) pt.Z = -490;
                    double zNew = pt.Z;
                    double scaleOld = FocalLength / (FocalLength + zOld);
                    double scaleNew = FocalLength / (FocalLength + zNew);
                    double ratio = scaleOld / scaleNew;
                    pt.X = pt.X * ratio;
                    pt.Y = pt.Y * ratio;
                }
            }
        }

        private void RotateByAngle(object parameter)
        {
            var center = GetTransformCenter();
            foreach (var shape in _selectedShapes) RotateAroundCenter(shape, _rotateAngle, center);
        }

        private void RotateYByAngle(object parameter)
        {
            var center = GetTransformCenter();
            foreach (var shape in _selectedShapes) RotateYAroundCenter(shape, _rotateY, center);
        }

        private void ScaleXByAmount(object parameter)
        {
            var center = GetTransformCenter();
            foreach (var shape in _selectedShapes) ScaleAroundCenter(shape, _scaleX, 1.0, center);
        }

        private void ScaleYByAmount(object parameter)
        {
            var center = GetTransformCenter();
            foreach (var shape in _selectedShapes) ScaleAroundCenter(shape, 1.0, _scaleY, center);
        }

        private void ScaleZByAmount(object parameter)
        {
            var center = GetTransformCenter();
            foreach (var shape in _selectedShapes) ScaleAroundCenter(shape, _scaleZ, _scaleZ, center);
        }

        private void MoveShape(ShapeViewModel shape, double dx, double dy)
        {
            if (shape.SelectedPoint != null)
            {
                shape.SelectedPoint.X += dx;
                shape.SelectedPoint.Y += dy;
            }
            else
            {
                foreach (var p in shape.PointViewModels)
                {
                    p.X += dx;
                    p.Y += dy;
                }
            }
        }

        private void RotateLeft(object parameter)
        {
            var center = GetTransformCenter();
            foreach (var shape in _selectedShapes) RotateAroundCenter(shape, -15, center);
        }

        private void RotateRight(object parameter)
        {
            var center = GetTransformCenter();
            foreach (var shape in _selectedShapes) RotateAroundCenter(shape, 15, center);
        }

        private void RotateAroundCenter(ShapeViewModel shape, double angleDegrees, Point center)
        {
            var rad = angleDegrees * Math.PI / 180.0;
            var cos = Math.Cos(rad);
            var sin = Math.Sin(rad);

            foreach (var p in shape.PointViewModels)
            {
                var dx = p.X - center.X;
                var dy = p.Y - center.Y;
                p.X = center.X + dx * cos - dy * sin;
                p.Y = center.Y + dx * sin + dy * cos;
            }
        }

        private void RotateYAroundCenter(ShapeViewModel shape, double angleDegrees, Point center)
        {
            var rad = angleDegrees * Math.PI / 180.0;
            var cos = Math.Cos(rad);
            var sin = Math.Sin(rad);

            foreach (var p in shape.PointViewModels)
            {
                var dx = p.X - center.X;
                var dz = p.Z;
                p.X = center.X + dx * cos + dz * sin;
                p.Z = -dx * sin + dz * cos;
            }
        }

        private void ScaleUp(object parameter)
        {
            var center = GetTransformCenter();
            foreach (var shape in _selectedShapes) ScaleAroundCenter(shape, 1.2, center);
        }

        private void ScaleDown(object parameter)
        {
            var center = GetTransformCenter();
            foreach (var shape in _selectedShapes) ScaleAroundCenter(shape, 0.8, center);
        }

        private void ScaleAroundCenter(ShapeViewModel shape, double scale, Point center)
        {
            foreach (var p in shape.PointViewModels)
            {
                var dx = p.X - center.X;
                var dy = p.Y - center.Y;
                p.X = center.X + dx * scale;
                p.Y = center.Y + dy * scale;
            }
        }

        private void ScaleAroundCenter(ShapeViewModel shape, double scaleX, double scaleY, Point center)
        {
            foreach (var p in shape.PointViewModels)
            {
                var dx = p.X - center.X;
                var dy = p.Y - center.Y;
                p.X = center.X + dx * scaleX;
                p.Y = center.Y + dy * scaleY;
            }
        }

        private void MirrorX(object parameter)
        {
            var center = GetTransformCenter();
            foreach (var shape in _selectedShapes) MirrorShape(shape, true, false, center);
        }

        private void MirrorY(object parameter)
        {
            var center = GetTransformCenter();
            foreach (var shape in _selectedShapes) MirrorShape(shape, false, true, center);
        }

        private void MirrorShape(ShapeViewModel shape, bool mirrorX, bool mirrorY, Point center)
        {
            foreach (var p in shape.PointViewModels)
            {
                if (mirrorX) p.X = center.X - (p.X - center.X);
                if (mirrorY) p.Y = center.Y - (p.Y - center.Y);
            }
        }

        private void ShearX(object parameter)
        {
            var center = GetTransformCenter();
            foreach (var shape in _selectedShapes) ShearShape(shape, 0.3, 0, center);
        }

        private void ShearY(object parameter)
        {
            var center = GetTransformCenter();
            foreach (var shape in _selectedShapes) ShearShape(shape, 0, 0.3, center);
        }

        private void ShearShape(ShapeViewModel shape, double shearX, double shearY, Point center)
        {
            foreach (var p in shape.PointViewModels)
            {
                var dy = p.Y - center.Y;
                var dx = p.X - center.X;
                p.X = p.X + dy * shearX;
                p.Y = p.Y + dx * shearY;
            }
        }

        private Point GetTransformCenter()
        {
            if (_selectedShapes.Count == 0) return new Point(0, 0);
            var bounds = SelectedBounds;
            return new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
        }

        private void DeselectAll(object parameter)
        {
            foreach (var shape in _selectedShapes) shape.IsSelected = false;
            _selectedShapes.Clear();
            SelectedShape = null;
            OnPropertyChanged(nameof(SelectedBounds));
        }

        public void ToggleShapeSelection(ShapeViewModel shape)
        {
            if (shape == null) return;
            if (_selectedShapes.Contains(shape)) _selectedShapes.Remove(shape);
            else _selectedShapes.Add(shape);
            shape.IsSelected = _selectedShapes.Contains(shape);
            SelectedShape = _selectedShapes.Count > 0 ? _selectedShapes.Last() : null;
            OnPropertyChanged(nameof(SelectedBounds));
        }

        private void DeleteShape(object parameter)
        {
            foreach (var shape in _selectedShapes.ToList()) _shapes.Remove(shape);
            _selectedShapes.Clear();
            SelectedShape = null;
            OnPropertyChanged(nameof(SelectedBounds));
        }

        private void Group(object parameter)
        {
            if (_selectedShapes.Count < 2) return;

            var shapesToGroup = _selectedShapes.ToList();
            
            // Создаём GroupModel из моделей фигур
            var groupModel = new GroupModel(shapesToGroup.Select(s => s.Shape));
            
            // Создаём GroupViewModel с существующими ShapeViewModel
            var groupVm = new GroupViewModel(groupModel, shapesToGroup);

            // Удаляем фигуры из списка
            foreach (var shape in shapesToGroup)
            {
                _shapes.Remove(shape);
            }

            _selectedShapes.Clear();

            // Добавляем группу
            _groups.Add(groupVm);
            groupVm.IsSelected = true;
            SelectedGroup = groupVm;
            OnPropertyChanged(nameof(SelectedBounds));
        }

        private void Ungroup(object parameter)
        {
            if (_selectedGroup == null) return;

            var group = _selectedGroup;
            var childShapes = group.ChildShapes.ToList();

            // Перемещаем дочерние фигуры обратно в _shapes
            foreach (var child in childShapes)
            {
                _shapes.Add(child);
                child.IsSelected = true;
                _selectedShapes.Add(child);
            }

            // Удаляем группу
            _groups.Remove(group);
            _selectedGroup = null;
            _selectedShape = _selectedShapes.Count > 0 ? _selectedShapes.Last() : null;
            OnPropertyChanged(nameof(SelectedBounds));
        }

        private void Save(object parameter)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "XML-файл фигур|*.drawerr", DefaultExt = ".drawerr", FileName = "figures"
            };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var dtos = new List<ShapeDto>();
                    
                    // Сохраняем обычные фигуры
                    dtos.AddRange(_shapes.Select(ShapeDto.FromViewModel));
                    
                    // Сохраняем группы
                    foreach (var group in _groups)
                    {
                        var groupDto = ShapeDto.FromGroupViewModel(group);
                        dtos.Add(groupDto);
                    }
                    
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
                Filter = "XML-файл фигур|*.drawerr", DefaultExt = ".drawerr"
            };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(List<ShapeDto>));
                    using var reader = new StreamReader(dlg.FileName);
                    var dtos = (List<ShapeDto>)serializer.Deserialize(reader);
                    _shapes.Clear();
                    _groups.Clear();
                    
                    foreach (var dto in dtos)
                    {
                        if (dto.IsGroup)
                        {
                            var group = dto.ToGroup();
                            var groupVm = new GroupViewModel(group);
                            groupVm.Initialize(); // Загружаем дочерние фигуры из DTO
                            _groups.Add(groupVm);
                        }
                        else
                        {
                            var shape = dto.ToShapeItem();
                            var vm = new ShapeViewModel(shape);
                            vm.Initialize();
                            _shapes.Add(vm);
                        }
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
            foreach (var shape in _selectedShapes.ToList()) shape.IsSelected = false;
            _selectedShapes.Clear();
            SelectedShape = null;
            OnPropertyChanged(nameof(SelectedBounds));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // ========== DTO ==========
    [XmlRoot("Shape")]
    public class ShapeDto
    {
        public string Id { get; set; }
        public ShapeType Type { get; set; }
        public PointDto[] Points { get; set; }
        public LineDto[] Lines { get; set; }
        public ShapeDto[] Children { get; set; }
        public bool IsGroup { get; set; }

        public static ShapeDto FromViewModel(ShapeViewModel vm)
        {
            var shape = vm.Shape;
            var pointDtos = shape.Points.Select(p => new PointDto
            {
                X = p.Position.X, Y = p.Position.Y,
                FillColor = ColorToHex(((SolidColorBrush)p.Fill)?.Color ?? Colors.Red)
            }).ToArray();

            var lineDtos = shape.Lines.Select(l => new LineDto
            {
                FromIndex = l.FromIndex, ToIndex = l.ToIndex,
                StrokeColor = ColorToHex(((SolidColorBrush)l.Stroke)?.Color ?? Colors.Black),
                StrokeThickness = l.StrokeThickness
            }).ToArray();

            return new ShapeDto
            {
                Id = shape.Id, Type = shape.Type, IsGroup = false,
                Points = pointDtos, Lines = lineDtos
            };
        }

        public static ShapeDto FromGroupViewModel(GroupViewModel groupVm)
        {
            var childDtos = groupVm.ChildShapes.Select(s => FromViewModel(s)).ToArray();
            
            return new ShapeDto
            {
                Id = groupVm.Group.Id,
                Type = ShapeType.Point, // Placeholder
                IsGroup = true,
                Children = childDtos
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

            if (Points != null)
            {
                for (int i = 0; i < Points.Length && i < shape.Points.Count; i++)
                {
                    shape.Points[i].Position = new Point(Points[i].X, Points[i].Y);
                    shape.Points[i].Fill = new SolidColorBrush(HexToColor(Points[i].FillColor));
                }
            }

            if (Lines != null)
            {
                for (int i = 0; i < Lines.Length && i < shape.Lines.Count; i++)
                {
                    shape.Lines[i].FromIndex = Lines[i].FromIndex;
                    shape.Lines[i].ToIndex = Lines[i].ToIndex;
                    shape.Lines[i].Stroke = new SolidColorBrush(HexToColor(Lines[i].StrokeColor));
                    shape.Lines[i].StrokeThickness = Lines[i].StrokeThickness;
                }
            }

            return shape;
        }

        public GroupModel ToGroup()
        {
            var group = new GroupModel();
            if (Children != null)
            {
                foreach (var childDto in Children)
                {
                    var childShape = childDto.ToShapeItem();
                    group.Children.Add(childShape);
                }
            }
            return group;
        }

        private static string ColorToHex(Color c) => $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";
        private static Color HexToColor(string hex)
        {
            if (string.IsNullOrEmpty(hex) || hex.Length < 7) return Colors.Black;
            hex = hex.TrimStart('#');
            if (hex.Length == 6) hex = "FF" + hex;
            try
            {
                return Color.FromArgb(
                    byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                    byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                    byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber),
                    byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber));
            }
            catch { return Colors.Black; }
        }
    }

    public class PointDto { public double X { get; set; } public double Y { get; set; } public string FillColor { get; set; } }
    public class LineDto { public int FromIndex { get; set; } public int ToIndex { get; set; } public string StrokeColor { get; set; } public double StrokeThickness { get; set; } }
}
