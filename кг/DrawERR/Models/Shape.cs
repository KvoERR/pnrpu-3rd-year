using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace DrawERR.Models
{
    public enum ShapeType
    {
        Point,
        Line,
        Triangle
    }

    // Точка фигуры
    public class ShapePoint
    {
        public Point Position { get; set; }
        public Brush Fill { get; set; }
        public double Radius { get; set; }
        public double Z { get; set; }

        public ShapePoint()
        {
            Position = new Point(0, 0);
            Fill = new SolidColorBrush(Colors.Red);
            Radius = 8;
            Z = 0;
        }

        public ShapePoint(double x, double y)
        {
            Position = new Point(x, y);
            Fill = new SolidColorBrush(Colors.Red);
            Radius = 8;
            Z = 0;
        }
    }

    // Линия между двумя точками (по индексам)
    public class ShapeLine
    {
        public int FromIndex { get; set; }
        public int ToIndex { get; set; }
        public Brush Stroke { get; set; }
        public double StrokeThickness { get; set; }

        public ShapeLine()
        {
            Stroke = new SolidColorBrush(Colors.Black);
            StrokeThickness = 2;
        }

        public ShapeLine(int from, int to)
        {
            FromIndex = from;
            ToIndex = to;
            Stroke = new SolidColorBrush(Colors.Black);
            StrokeThickness = 2;
        }
    }

    // Фигура = группа точек + линии между ними
    public abstract class ShapeItem
    {
        public string Id { get; set; }
        public ShapeType Type { get; set; }
        public bool IsSelected { get; set; }

        // Точки фигуры
        public ObservableCollection<ShapePoint> Points { get; set; }

        // Линии между точками
        public ObservableCollection<ShapeLine> Lines { get; set; }

        protected ShapeItem()
        {
            Id = Guid.NewGuid().ToString();
            Points = new ObservableCollection<ShapePoint>();
            Lines = new ObservableCollection<ShapeLine>();
        }

        // Получение всех позиций точек
        public Point[] GetAllPoints()
        {
            var pts = new Point[Points.Count];
            for (int i = 0; i < Points.Count; i++)
                pts[i] = Points[i].Position;
            return pts;
        }

        // Вычисление bounding box
        public virtual Rect GetBounds()
        {
            var points = GetAllPoints();
            if (points.Length == 0) return Rect.Empty;

            double minX = points[0].X, maxX = points[0].X;
            double minY = points[0].Y, maxY = points[0].Y;

            foreach (var p in points)
            {
                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
            }

            double padding = 5;
            return new Rect(
                minX - padding,
                minY - padding,
                maxX - minX + padding * 2,
                maxY - minY + padding * 2
            );
        }

        // Получение центра фигуры
        public virtual Point GetCenter()
        {
            var points = GetAllPoints();
            if (points.Length == 0) return new Point(0, 0);

            double cx = 0, cy = 0;
            foreach (var p in points)
            {
                cx += p.X;
                cy += p.Y;
            }
            return new Point(cx / points.Length, cy / points.Length);
        }
    }

    // ========== ТОЧКА ==========
    public class PointShape : ShapeItem
    {
        public PointShape()
        {
            Type = ShapeType.Point;
            Points.Add(new ShapePoint { Fill = new SolidColorBrush(Colors.Red) });
        }

        public PointShape(double x, double y) : this()
        {
            Points[0].Position = new Point(x, y);
        }
    }

    // ========== ПРЯМАЯ (2 точки + линия между ними) ==========
    public class LineShape : ShapeItem
    {
        public LineShape()
        {
            Type = ShapeType.Line;

            // 2 точки
            Points.Add(new ShapePoint(0, 0) { Fill = new SolidColorBrush(Colors.Blue) });
            Points.Add(new ShapePoint(50, 0) { Fill = new SolidColorBrush(Colors.Blue) });

            // 1 линия: от точки 0 к точке 1
            Lines.Add(new ShapeLine(0, 1)
            {
                Stroke = new SolidColorBrush(Colors.Blue),
                StrokeThickness = 3
            });
        }

        public LineShape(Point p1, Point p2) : this()
        {
            Points[0].Position = p1;
            Points[1].Position = p2;
        }
    }

    // ========== ГРУППА ==========
    public class GroupModel : ShapeItem
    {
        public ObservableCollection<ShapeItem> Children { get; set; }

        public GroupModel()
        {
            Type = ShapeType.Point; // Placeholder
            Children = new ObservableCollection<ShapeItem>();
        }

        public GroupModel(IEnumerable<ShapeItem> shapes) : this()
        {
            foreach (var s in shapes)
                Children.Add(s);
        }

        // Переопределяем GetBounds для группы
        public override Rect GetBounds()
        {
            if (Children.Count == 0) return Rect.Empty;

            double minX = double.MaxValue, maxX = double.MinValue;
            double minY = double.MaxValue, maxY = double.MinValue;

            foreach (var child in Children)
            {
                var b = child.GetBounds();
                if (b.X < minX) minX = b.X;
                if (b.Right > maxX) maxX = b.Right;
                if (b.Y < minY) minY = b.Y;
                if (b.Bottom > maxY) maxY = b.Bottom;
            }

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        // Переопределяем GetCenter для группы
        public override Point GetCenter()
        {
            var bounds = GetBounds();
            if (bounds.IsEmpty) return new Point(0, 0);
            return new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
        }
    }

    // ========== ТРЕУГОЛЬНИК (3 точки + 3 линии между ними) ==========
    public class TriangleShape : ShapeItem
    {
        public TriangleShape()
        {
            Type = ShapeType.Triangle;

            // 3 точки
            Points.Add(new ShapePoint(0, -25) { Fill = new SolidColorBrush(Colors.Orange) });
            Points.Add(new ShapePoint(-25, 25) { Fill = new SolidColorBrush(Colors.Orange) });
            Points.Add(new ShapePoint(25, 25) { Fill = new SolidColorBrush(Colors.Orange) });

            // 3 линии: 0→1, 1→2, 2→0
            Lines.Add(new ShapeLine(0, 1) { Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = 2 });
            Lines.Add(new ShapeLine(1, 2) { Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = 2 });
            Lines.Add(new ShapeLine(2, 0) { Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = 2 });
        }

        public TriangleShape(Point p1, Point p2, Point p3) : this()
        {
            Points[0].Position = p1;
            Points[1].Position = p2;
            Points[2].Position = p3;
        }
    }
}
