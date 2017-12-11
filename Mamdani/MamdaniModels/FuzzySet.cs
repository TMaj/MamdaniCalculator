using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mamdani.MamdaniModels
{
    public class FuzzySet
    {
        public FuzzySet(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public double Value { get; set; }
    }
}
