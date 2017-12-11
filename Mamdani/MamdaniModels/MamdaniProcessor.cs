using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mamdani;

namespace Mamdani.MamdaniModels
{
    public class MamdaniProcessor
    {
        public List<Variable> Inputs { get; set; }
        public Variable Output { get; set; }
        public List<Rule> Rules { get; set; }

        public MamdaniProcessor()
        {
            Inputs = new List<Variable>();
            Rules = new List<Rule>();
        }

        public void CreateInputVariables(Dictionary<VariableLine,List<MemberFunction>> variableFunctions) 
        {      
            Inputs.Clear();
            foreach(var inputVariableLine in variableFunctions)
            {
                double rangeStart = Convert.ToDouble(inputVariableLine.Key.Range[0]);
                double rangeEnd = Convert.ToDouble(inputVariableLine.Key.Range[1]);

                var variable = new Variable(inputVariableLine.Key.VariableName, rangeStart, rangeEnd);

                foreach (var memberFunction in inputVariableLine.Value)
                {
                    var fuzzySet = new FuzzySet(memberFunction.SetName);
                    IFunction function;
                    switch (memberFunction.FunctionType)
                    {
                        case FunctionType.TriangleMF :
                            {
                                function = new TriMF(Convert.ToDouble(memberFunction.FunctionPoints[0]),
                                                     Convert.ToDouble(memberFunction.FunctionPoints[1]),
                                                     Convert.ToDouble(memberFunction.FunctionPoints[2]));
                                break;
                            }
                        default:
                            {
                                function = new TriMF(Convert.ToDouble(memberFunction.FunctionPoints[0]),
                                                     Convert.ToDouble(memberFunction.FunctionPoints[1]),
                                                     Convert.ToDouble(memberFunction.FunctionPoints[2]));
                                break;
                            }
                    }

                    variable.AddFuzzySet(fuzzySet, function);                     
                }

                Inputs.Add(variable);
            }
        }

        public void CreateOutputVariables(VariableLine outputVariable, List<MemberFunction> memberFunctions)
        {
            double rangeStart = Convert.ToDouble(outputVariable.Range[0]);
            double rangeEnd = Convert.ToDouble(outputVariable.Range[1]);

            var variable = new Variable(outputVariable.VariableName, rangeStart, rangeEnd);

            foreach (var memberFunction in memberFunctions)
            {
                var fuzzySet = new FuzzySet(memberFunction.SetName);
                IFunction function;
                switch (memberFunction.FunctionType)
                {
                    case FunctionType.TriangleMF:
                        {
                            function = new TriMF(Convert.ToDouble(memberFunction.FunctionPoints[0]),
                                                 Convert.ToDouble(memberFunction.FunctionPoints[1]),
                                                 Convert.ToDouble(memberFunction.FunctionPoints[2]));
                            break;
                        }
                    default:
                        {
                            function = new TriMF(Convert.ToDouble(memberFunction.FunctionPoints[0]),
                                                 Convert.ToDouble(memberFunction.FunctionPoints[1]),
                                                 Convert.ToDouble(memberFunction.FunctionPoints[2]));
                            break;
                        }
                }

                variable.AddFuzzySet(fuzzySet, function);
            }

            Output = variable;
        }

        public void CreateRules(List<RuleLine> ruleLines, NormOperator normOperator)
        {
            Rules.Clear();

            foreach (var ruleLine in ruleLines)
            {
                var inputSets = new Dictionary<Variable, FuzzySet>();
                
                    foreach (var variableFunction in ruleLine.VariableFunction)
                    {
                        var inputVariable = Inputs.FirstOrDefault(iv => iv.Name.Equals(variableFunction.Key.VariableName));
                        var inputFuzzySet = inputVariable.FuzzySets.FirstOrDefault(fs => fs.Key.Name.Equals(variableFunction.Value.SetName));
                        inputSets.Add(inputVariable, inputFuzzySet.Key);
                    }

                var outputVariable = Output;
                var outputFuzzySet = Output.FuzzySets.Keys.FirstOrDefault(fs => fs.Name.Equals(ruleLine.OuputVariableFunction.Item2.SetName));
                var outputSet= new Tuple<Variable, FuzzySet>(outputVariable, outputFuzzySet);

                var rule = new Rule(ruleLine.RuleOperator, normOperator, outputSet, inputSets);
                Rules.Add(rule);
            }
          
        }

        public double CalculateOutput(List<double> values, DeffuzyficationType deffuzyficationType )
        {
            for(int i =0; i< Inputs.Count; i++)
            {
                Inputs[i].CountValues(values[i]);
            }

            var listOfRulesOutputs = new List<Tuple<string, double>>();
            var resultList = new Dictionary<string, double>();
            foreach (var rule in Rules)
            {
                listOfRulesOutputs.Add(new Tuple<string, double>(rule.OutputSet.Item2.Name, rule.CalculateOutputValue()));
            }

            foreach (var fuzzySet in Output.FuzzySets.Keys)
            {
                var tempList = listOfRulesOutputs.Where(x => x.Item1.Equals(fuzzySet.Name));
                var maxrecord = tempList.OrderByDescending(x => x.Item2).FirstOrDefault();
                resultList.Add(maxrecord.Item1,maxrecord.Item2);
            }

            var maxFuzzySet = resultList.OrderByDescending(x => x.Value).FirstOrDefault();

            double outputValue;
            switch (deffuzyficationType)
            {
                case DeffuzyficationType.LeftMax:
                    {
                         outputValue = Output.FuzzySets.FirstOrDefault(x => x.Key.Name.Equals(maxFuzzySet.Key)).Value.LeftMax(maxFuzzySet.Value);
                        break;
                    }
                case DeffuzyficationType.CenterMax:
                    {
                         outputValue = Output.FuzzySets.FirstOrDefault(x => x.Key.Name.Equals(maxFuzzySet.Key)).Value.CenterMax(maxFuzzySet.Value);
                        break;
                    }
                case DeffuzyficationType.RightMax:
                    {
                         outputValue = Output.FuzzySets.FirstOrDefault(x => x.Key.Name.Equals(maxFuzzySet.Key)).Value.RightMax(maxFuzzySet.Value);
                        break;
                    }
                default:
                    {
                        outputValue = Output.FuzzySets.FirstOrDefault(x => x.Key.Name.Equals(maxFuzzySet.Key)).Value.LeftMax(maxFuzzySet.Value);
                        break;
                    }


            }

            return outputValue;
        }

    }
}
