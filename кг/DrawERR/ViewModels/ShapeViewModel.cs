using DrawERR.Models;
using System;
using System.Collections.ObjectModel;
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
        private int _selectedPointIndex = -1;

        public ShapeViewModel(ShapeItem model)
        {
            _model = model;
        }

        public ShapeItem Shape => _model;

        public string Id => _model.Id;
        public ShapeType Type => _model.Type;

        // Коллекция точек с координатами для привязки
        public ObservableCollection<PointViewModel> PointViewModels { get; } = new ObservableCollection<PointViewModel>();

        // Коллекция линий с координатами для рисования
        public ObservableCollection<LineInfo> Lines { get; } = new ObservableCollection<LineInfo>();

        // Инициализация ViewModel из модели
        public void Initialize()
        {
            PointViewModels.Clear();
            Lines.Clear();

            foreach (var pt in _model.Points)
            {
                var vm = new PointViewModel(pt);
                vm.PropertyChanged += (s, e) => NotifyChanged();
                PointViewModels.Add(vm);
            }

            NotifyChanged();
        }

        // Выбранная точка (индекс)
        public int SelectedPointIndex
        {
            get => _selectedPointIndex;
            set
            {
                _selectedPointIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedPoint));
            }
        }

        public PointViewModel SelectedPoint
        {
            get => _selectedPointIndex >= 0 && _selectedPointIndex < PointViewModels.Count
                ? PointViewModels[_selectedPointIndex] : null;
            set
            {
                if (value == null)
                    _selectedPointIndex = -1;
                else
                {
                    var idx = PointViewModels.IndexOf(value);
                    _selectedPointIndex = idx >= 0 ? idx : -1;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedPointIndex));
            }
        }

        // Переключение выделения точки
        public void TogglePointSelection(int index)
        {
            if (_selectedPointIndex == index)
                SelectedPointIndex = -1; // отмена выделения
            else
                SelectedPointIndex = index;
        }

        private void NotifyChanged()
        {
            Lines.Clear();
            for (int i = 0; i < _model.Lines.Count; i++)
            {
                var lineDef = _model.Lines[i];
                if (lineDef.FromIndex < PointViewModels.Count && lineDef.ToIndex < PointViewModels.Count)
                {
                    var p1 = PointViewModels[lineDef.FromIndex];
                    var p2 = PointViewModels[lineDef.ToIndex];
                    Lines.Add(new LineInfo
                    {
                        Points = new PointCollection(new[] { new Point(p1.X, p1.Y), new Point(p2.X, p2.Y) }),
                        Stroke = lineDef.Stroke,
                        StrokeThickness = lineDef.StrokeThickness
                    });
                }
            }
            OnPropertyChanged(nameof(Lines));
            OnPropertyChanged(nameof(Bounds));
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
                    ShapeType.Point => $"Точка",
                    ShapeType.Line => $"Прямая ({_model.Points.Count} точек)",
                    ShapeType.Triangle => $"Треугольник ({_model.Points.Count} точек)",
                    _ => $"{Type}"
                };
            }
        }

        private int PointsCount => _model.Points.Count;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // ViewModel точки для привязки свойств
    public class PointViewModel : INotifyPropertyChanged
    {
        private ShapePoint _model;

        public PointViewModel(ShapePoint model)
        {
            _model = model;
        }

        public double X
        {
            get => _model.Position.X;
            set
            {
                _model.Position = new Point(value, _model.Position.Y);
                OnPropertyChanged();
            }
        }

        public double Y
        {
            get => _model.Position.Y;
            set
            {
                _model.Position = new Point(_model.Position.X, value);
                OnPropertyChanged();
            }
        }

        public Brush Fill
        {
            get => _model.Fill;
            set
            {
                _model.Fill = value;
                OnPropertyChanged();
            }
        }

        public double Radius
        {
            get => _model.Radius;
            set
            {
                _model.Radius = value;
                OnPropertyChanged();
            }
        }

        public double Z
        {
            get => _model.Z;
            set
            {
                _model.Z = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Информация о линии для рисования
    public class LineInfo
    {
        public PointCollection Points { get; set; }
        public Brush Stroke { get; set; }
        public double StrokeThickness { get; set; }
    }

    // ViewModel группы фигур
    public class GroupViewModel : INotifyPropertyChanged
    {
        private GroupModel _model;
        private bool _isSelected;

        public GroupViewModel(GroupModel model)
        {
            _model = model;
        }

        // Конструктор с готовыми ShapeViewModel
        public GroupViewModel(GroupModel model, IEnumerable<ShapeViewModel> childViewModels)
        {
            _model = model;
            foreach (var childVm in childViewModels)
            {
                ChildShapes.Add(childVm);
                foreach (var pt in childVm.PointViewModels)
                    PointViewModels.Add(pt);
            }
        }

        public GroupModel Group => _model;
        public string Id => _model.Id;

        // Коллекция точек всех фигур внутри группы
        public ObservableCollection<PointViewModel> PointViewModels { get; } = new ObservableCollection<PointViewModel>();

        // Коллекция ShapeViewModel дочерних фигур
        public ObservableCollection<ShapeViewModel> ChildShapes { get; } = new ObservableCollection<ShapeViewModel>();

        // Инициализация ViewModel из модели (для загрузки из файла)
        public void Initialize()
        {
            PointViewModels.Clear();
            ChildShapes.Clear();

            foreach (var child in _model.Children)
            {
                // Если это ShapeItem, создаём ShapeViewModel
                if (child is not GroupModel)
                {
                    var shapeVm = new ShapeViewModel(child);
                    shapeVm.Initialize();
                    ChildShapes.Add(shapeVm);

                    foreach (var pt in shapeVm.PointViewModels)
                        PointViewModels.Add(pt);
                }
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

        public string DisplayName => $"Группа ({ChildShapes.Count} фигур)";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
