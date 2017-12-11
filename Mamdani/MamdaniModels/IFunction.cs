using System.Collections.Generic;

namespace Mamdani.MamdaniModels
{   /// <summary>
    /// Membership function interface
    /// </summary>
    public interface IFunction
    {
        double StartPoint { get; set; }
        double EndPoint { get; set; }
        List<double> Points { get; set; }

        double Value(double point);
        double LeftMax(double value);
        double RightMax(double value);
        double CenterMax(double value);
    }
}
