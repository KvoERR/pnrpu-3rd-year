using System;
using System.Windows;
using System.Windows.Data;

namespace DrawERR.ViewModels
{
    /// <summary>
    /// Перспективная проекция 3D→2D.
    /// scale = focalLength / (focalLength + Z)
    /// projectedX = centerX + (X - centerX) * scale
    /// projectedY = centerY + (Y - centerY) * scale
    /// </summary>
    public class ProjectionConverter : IMultiValueConverter
    {
        private const double FocalLength = 500;

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length < 3) return new Point(0, 0);

            if (!double.TryParse(values[0]?.ToString(), out double x)) x = 0;
            if (!double.TryParse(values[1]?.ToString(), out double y)) y = 0;
            if (!double.TryParse(values[2]?.ToString(), out double z)) z = 0;

            double scale = FocalLength / (FocalLength + z);
            return new Point(x * scale, y * scale);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Проекция X: Canvas.Left = centerX + (X - centerX) * focalLength / (focalLength + Z)
    /// </summary>
    public class ProjectXConverter : IMultiValueConverter
    {
        private const double FocalLength = 500;

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length < 3) return 0.0;

            if (!double.TryParse(values[0]?.ToString(), out double x)) x = 0;
            if (!double.TryParse(values[1]?.ToString(), out double centerX)) centerX = 0;
            if (!double.TryParse(values[2]?.ToString(), out double z)) z = 0;

            double scale = FocalLength / (FocalLength + z);
            return centerX + (x - centerX) * scale;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Проекция Y: Canvas.Top = centerY + (Y - centerY) * focalLength / (focalLength + Z)
    /// </summary>
    public class ProjectYConverter : IMultiValueConverter
    {
        private const double FocalLength = 500;

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length < 3) return 0.0;

            if (!double.TryParse(values[0]?.ToString(), out double y)) y = 0;
            if (!double.TryParse(values[1]?.ToString(), out double centerY)) centerY = 0;
            if (!double.TryParse(values[2]?.ToString(), out double z)) z = 0;

            double scale = FocalLength / (FocalLength + z);
            return centerY + (y - centerY) * scale;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
