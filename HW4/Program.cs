public class Program
{
    const double defaultE = 0.0000001d;
    static void Main(string[] args)
    {
        var result = Solve(1, 2, 3);
    }

    public static double[] Solve(double a, double b, double c,double eps = defaultE)
    {
        if (Math.Abs(a) < eps)
            throw new AZeroException(a);
        
        if(double.IsInfinity(a) || double.IsInfinity(b) || double.IsInfinity(c))
            throw new InfinityException();

        double d = b * b - 4 * a * c;

        if (d <0)
            return new double[0];

        else if (Math.Abs(d) < eps)
            return new double[2]
                {-b/(2*a) ,-b/(2*a) };

        else return new double[2]
                { (-b + Math.Sqrt(d))/2*a ,
                  (-b - Math.Sqrt(d))/2*a };
    }
    
    public class AZeroException : ArgumentException
    {
        public AZeroException(double a) : base($"параметр a = {a} слишком близок к 0") { }
    }

    public class InfinityException : ArgumentException
    {
        public InfinityException() : base($"один из параметров не может быть равен бесконечности") { }
    }
}