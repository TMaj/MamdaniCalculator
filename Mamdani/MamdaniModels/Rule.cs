using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mamdani.MamdaniModels
{
    public class Rule
    {
        public Rule(RuleOperator ruleOperator, NormOperator normOperator, Tuple<Variable,FuzzySet> outputSet, Dictionary<Variable,FuzzySet> inputSet)
        {
            RuleOperator = ruleOperator;
            NormOperator = normOperator;
            OutputSet = outputSet;
            InputSet = inputSet;
            Valid = true;
        }

        public bool Valid { get; set; }

        public RuleOperator RuleOperator { get; set; }
        public NormOperator NormOperator { get; set; }

        public Tuple<Variable,FuzzySet> OutputSet { get; set; }
        public Dictionary<Variable,FuzzySet> InputSet { get; set; }


        public double CalculateOutputValue()
        {
            var inputSetsValues = InputSet.Select(x => x.Value.Value).ToList();

            if (inputSetsValues.All(x => x == 0))
            {
                Valid = false;
                return 0;
            }

            switch (RuleOperator)
            {
                case RuleOperator.OR:
                    {
                        return CalculateOutput(inputSetsValues, NormOperator);
                        break;
                    }
                case RuleOperator.AND:
                    {
                        if (inputSetsValues.Any(x => x == 0))
                        {
                            Valid = false;
                            return 0;
                        }
                        return CalculateOutput(inputSetsValues, NormOperator);
                        break;
                    }
                default:
                    {
                        return 0;
                        break;
                    }
            }
                
            

        }

        private double CalculateOutput(List<double> values, NormOperator norm)
        {
            switch (norm)
            {
                case NormOperator.PROD:
                    {
                        double returnVal = 1;
                        values.Where(x => x != 0).Select(x => returnVal *= x);
                        return returnVal;
                        break;
                    }
                case NormOperator.MIN:
                    {
                        return values.Min(); ;
                        break;
                    }
                default:
                    {
                        return 0;
                    }
            }                        
        }

        public override string ToString()
        {
            string returnVal = "IF ";
            var statements = InputSet.Select(x => x.Key.Name + " = " + x.Value.Name).ToList();

            statements.ForEach(x => x+= RuleOperator.GetName(typeof(RuleOperator),RuleOperator));
            returnVal += (" THEN " + OutputSet.Item1.Name + " = " + OutputSet.Item2.Name);
            return returnVal;
        }
    }
    
    public enum RuleOperator
    {
        AND,
        OR
    }

    public enum NormOperator
    {
        MIN,
        PROD
    }
}
