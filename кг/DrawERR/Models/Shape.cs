using System;
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

    // Базовый класс для всех фигур
    public abstract class ShapeItem
    {
        public string Id { get; set; }
        public ShapeType Type { get; set; }
        public Brush Fill { get; set; }
        public Brush Stroke { get; set; }
        public double StrokeThickness { get; set; }
        public bool IsSelected { get; set; }

        // МАТРИЦА ТОЧЕК (локальные координаты)
        public Point[] Points { get; set; }

        // Трансформационная матрица
        public Matrix TransformMatrix { get; set; }

        protected ShapeItem()
        {
            Id = Guid.NewGuid().ToString();
            Fill = new SolidColorBrush(Colors.Transparent);
            Stroke = new SolidColorBrush(Colors.Black);
            StrokeThickness = 2;
            TransformMatrix = Matrix.Identity;
        }

        // Получение трансформированных точек (глобальные координаты)
        public virtual Point[] GetTransformedPoints()
        {
            if (Points == null) return new Point[0];

            var transformed = new Point[Points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                transformed[i] = Points[i] * TransformMatrix;
            }
            return transformed;
        }

        // Вычисление bounding box
        public virtual Rect GetBounds()
        {
            var points = GetTransformedPoints();
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

            // Добавляем отступ для отображения
            double padding = 5;
            return new Rect(
                minX - padding,
                minY - padding,
                maxX - minX + padding * 2,
                maxY - minY + padding * 2
            );
        }

        // Перемещение фигуры
        public void Move(double dx, double dy)
        {
            var matrix = TransformMatrix;
            matrix.OffsetX += dx;
            matrix.OffsetY += dy;
            TransformMatrix = matrix;
        }

        // Поворот вокруг центра
        public void Rotate(double angleDegrees)
        {
            var angle = angleDegrees * Math.PI / 180;
            var localCenter = GetLocalCenter();

            var matrix = TransformMatrix;
            double currentAngle = Math.Atan2(matrix.M12, matrix.M11);
            double currentScale = Math.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12);
            double newAngle = currentAngle + angle;

            var result = new Matrix();
            result.OffsetX = matrix.OffsetX;
            result.OffsetY = matrix.OffsetY;
            result.M11 = Math.Cos(newAngle) * currentScale;
            result.M12 = Math.Sin(newAngle) * currentScale;
            result.M21 = -Math.Sin(newAngle) * currentScale;
            result.M22 = Math.Cos(newAngle) * currentScale;

            var rotScale = new Matrix(result.M11, result.M12, result.M21, result.M22, 0, 0);
            rotScale.Translate(-localCenter.X, -localCenter.Y);
            rotScale.Translate(localCenter.X, localCenter.Y);
            result = Matrix.Multiply(result, rotScale);

            TransformMatrix = result;
        }

        // Масштабирование от центра
        public void Scale(double scale)
        {
            var localCenter = GetLocalCenter();

            var matrix = TransformMatrix;
            double currentAngle = Math.Atan2(matrix.M12, matrix.M11);
            double currentScale = Math.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12);
            double newScale = currentScale * scale;

            var result = new Matrix();
            result.OffsetX = matrix.OffsetX;
            result.OffsetY = matrix.OffsetY;
            result.M11 = Math.Cos(currentAngle) * newScale;
            result.M12 = Math.Sin(currentAngle) * newScale;
            result.M21 = -Math.Sin(currentAngle) * newScale;
            result.M22 = Math.Cos(currentAngle) * newScale;

            var rotScale = new Matrix(result.M11, result.M12, result.M21, result.M22, 0, 0);
            rotScale.Translate(-localCenter.X, -localCenter.Y);
            rotScale.Translate(localCenter.X, localCenter.Y);
            result = Matrix.Multiply(result, rotScale);

            TransformMatrix = result;
        }

        // Получение центра в локальных координатах
        private Point GetLocalCenter()
        {
            if (Points == null || Points.Length == 0) return new Point(0, 0);
            double cx = 0, cy = 0;
            foreach (var p in Points)
            {
                cx += p.X;
                cy += p.Y;
            }
            return new Point(cx / Points.Length, cy / Points.Length);
        }

        // Получение центра фигуры
        public Point GetCenter()
        {
            var points = GetTransformedPoints();
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
            Points = new Point[] { new Point(0, 0) };
            Fill = new SolidColorBrush(Colors.Red);
            StrokeThickness = 0;
        }

        public PointShape(double x, double y) : this()
        {
            var matrix = TransformMatrix;
            matrix.OffsetX = x;
            matrix.OffsetY = y;
            TransformMatrix = matrix;
        }

        public override Rect GetBounds()
        {
            var point = GetTransformedPoints()[0];
            return new Rect(point.X - 4, point.Y - 4, 8, 8);
        }
    }

    // ========== ПРЯМАЯ ==========
    public class LineShape : ShapeItem
    {
        public LineShape()
        {
            Type = ShapeType.Line;
            Points = new Point[]
            {
                new Point(0, 0),
                new Point(50, 0)
            };
            Fill = new SolidColorBrush(Colors.Transparent);
            Stroke = new SolidColorBrush(Colors.Blue);
            StrokeThickness = 3;
        }

        public LineShape(Point start, Point end) : this()
        {
            Points = new Point[] { start, end };
        }

        public override Rect GetBounds()
        {
            var points = GetTransformedPoints();
            if (points.Length < 2) return Rect.Empty;

            double minX = Math.Min(points[0].X, points[1].X);
            double maxX = Math.Max(points[0].X, points[1].X);
            double minY = Math.Min(points[0].Y, points[1].Y);
            double maxY = Math.Max(points[0].Y, points[1].Y);

            double padding = 5;
            return new Rect(
                minX - padding,
                minY - padding,
                maxX - minX + padding * 2,
                maxY - minY + padding * 2
            );
        }
    }

    // ========== ТРЕУГОЛЬНИК ==========
    public class TriangleShape : ShapeItem
    {
        public TriangleShape()
        {
            Type = ShapeType.Triangle;
            Points = new Point[]
            {
                new Point(0, -25),
                new Point(-25, 25),
                new Point(25, 25)
            };
            Fill = new SolidColorBrush(Colors.LightCoral);
            Stroke = new SolidColorBrush(Colors.Black);
            StrokeThickness = 2;
        }

        public TriangleShape(Point p1, Point p2, Point p3) : this()
        {
            Points = new Point[] { p1, p2, p3 };
        }

        public override Rect GetBounds()
        {
            var points = GetTransformedPoints();
            if (points.Length < 3) return Rect.Empty;

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
    }
}