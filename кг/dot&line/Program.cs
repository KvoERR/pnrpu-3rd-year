class Program
{
    public static void Main()
    {
        Console.WriteLine(52);
    }
    
    public static (double A2, double B2, double C2, double x2, double y2) Height(
        double A1, double B1, double C1,
        double x1, double y1)
    {
        double A2 = (-B1) / (x1 * B1 - A1 * y1);
        double B2 = A1 / (x1 * B1 - A1 * y1);
        double C2 = 1;
        
        double x2 = ((-x1 * B1 + A1 * y1) * B1 + A1) / (-B1 * B1 - A1 * A1);
        double y2 = (B1 + A1 * (-x1 * B1 + A1 * y1)) / (-B1 * B1 - A1 * A1);
        
        return (A2, B2, C2, x2, y2);
    }

    public static (double A, double B, double C, double xm, double ym) Median(
    double x1, double y1,
    double x2, double y2,
    double x3, double y3)
    {
        double xm = (x1 + x2) / 2;
        double ym = (y1 + y2) / 2;
        
        double A = y3 - ym;
        double B = xm - x3;
        double C = x3 * ym - xm * y3;
        
        return (A, B, C, xm, ym);
    }

    public static ((double A, double B, double C), (double A, double B, double C)) Bisectors(
    double A1, double B1, double C1,
    double A2, double B2, double C2)
    {
        double n1 = Math.Sqrt(A1 * A1 + B1 * B1);
        double n2 = Math.Sqrt(A2 * A2 + B2 * B2);
        
        return (
            (A1 / n1 - A2 / n2, B1 / n1 - B2 / n2, C1 / n1 - C2 / n2),
            (A1 / n1 + A2 / n2, B1 / n1 + B2 / n2, C1 / n1 + C2 / n2)
        );
    }

}
