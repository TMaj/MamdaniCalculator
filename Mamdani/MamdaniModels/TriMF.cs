using System.Collections.Generic;

namespace Mamdani.MamdaniModels
{
    public class TriMF : IFunction
    {
        public double StartPoint { get => Points[0]; set => Points[0] = value; }
        public double MiddlePoint { get => Points[1]; set => Points[1] = value; }
        public double EndPoint { get => Points[2]; set => Points[2] = value; }
        public List<double> Points { get; set; }

        public double Value(double point)
        {
            if (point < StartPoint || point > EndPoint)
            {
                return 0;
            }

            if (point == MiddlePoint)
            {
                return 1;
            }

            if (point < MiddlePoint)
            {
                return ((point - StartPoint) / (MiddlePoint - StartPoint));

            }

            return ((EndPoint - point) / (EndPoint - MiddlePoint));
        }

        public double LeftMax(double value)
        {
            if (value < 0 || value > 1)
            {
                return 0;
            }

            return ((MiddlePoint - StartPoint) * value) + StartPoint;
        }

        public double RightMax(double value)
        {
            if (value < 0 || value > 1)
            {
                return 0;
            }

            return EndPoint - ((EndPoint - MiddlePoint) * value);

        }

        public double CenterMax(double value)
        {
           return LeftMax(value) + ((RightMax(value) - LeftMax(value)) / 2);
        }

        public TriMF(double start, double middle, double end)
        {
            Points = new List<double>();

            var pointList = new List<double> { start, middle, end };
            pointList.Sort();

            Points = pointList;
        }
    }
}
