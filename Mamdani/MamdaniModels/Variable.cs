using System.Collections.Generic;
using System.Linq;

namespace Mamdani.MamdaniModels
{
    public class Variable
    {
        public string Name { get; set; }
        public double RangeStart { get; set; }
        public double RangeEnd { get; set; }
        
        public Dictionary<FuzzySet, IFunction> FuzzySets { get; set; }

        public Variable(string name ,double rangeStart, double rangeEnd)
        {
            FuzzySets = new Dictionary<FuzzySet, IFunction>();
            Name = name;

            if (rangeEnd < rangeStart)
            {
                RangeStart = rangeEnd;
                RangeEnd = rangeStart;
            }
            else
            {
                RangeStart = rangeStart;
                RangeEnd = rangeEnd;
            }        
        }

        public void AddFuzzySet(FuzzySet fuzzySet,IFunction mfFunction)
        {
            if (mfFunction.StartPoint < RangeStart || mfFunction.EndPoint < RangeStart
                || mfFunction.StartPoint > RangeEnd || mfFunction.EndPoint > RangeEnd)
            {
                return;
            }

            FuzzySets.Add(fuzzySet,mfFunction);
        }

        public IList<double> CountValues(double point)
        {
            foreach (var item in FuzzySets)
            {
                item.Key.Value = item.Value.Value(point);
            }

            return FuzzySets.Select(fs => fs.Key.Value).ToList();
        }
    }
}
