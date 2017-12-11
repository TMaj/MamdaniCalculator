using Mamdani.MamdaniModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mamdani
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MamdaniProcessor processor;
        List<VariableLine> inputVariables;
        VariableLine outputVariable;
        VariableLine selectedVariable;
        RuleCreateLine ruleCreateLine;
        List<RuleLine> ruleLines;
        List<TextBox> inputTextBoxes;

        public MainWindow()
        {
            InitializeComponent();

            processor = new MamdaniProcessor();
            cbxInputs.ItemsSource = new List<int> { 1, 2, 3 };
            inputVariables = new List<VariableLine>();
            outputVariable = new VariableLine(1);
            outputVariable.CheckBoxChecked += CheckboxCheckedEvent;
            outputStackPanel.Children.Add(outputVariable);
            ruleLines = new List<RuleLine>();
            inputTextBoxes = new List<TextBox>();
            listBoxRules.ItemsSource = ruleLines;

            cbxDefuzzifiction.Items.Add("Left Max");
            cbxDefuzzifiction.Items.Add("Center Max");
            cbxDefuzzifiction.Items.Add("Right Max");
            cbxDefuzzifiction.SelectedIndex = 0;
            cbxInputs.SelectedIndex = 1;

        }

        private void cbxInputs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            inputVariables.Clear();
            inputStackPanel.Children.Clear();
            inputsStackPanel.Children.Clear();

            for (int i = 1; i <= cbxInputs.SelectedIndex + 1; i++)
            {
                var item = new VariableLine(i);
                item.CheckBoxChecked += CheckboxCheckedEvent;
                inputVariables.Add(item);
                inputStackPanel.Children.Add(item);

                var textBox = new TextBox();
                textBox.Margin = new Thickness(5, 10, 15, 0);
                inputsStackPanel.Children.Add(textBox);
                inputTextBoxes.Add(textBox);
            }
            ruleLines.Clear();
            listBoxRules.Items.Refresh();
            btnRefreshRules_Click(this, null);
        }

        private void CheckboxCheckedEvent(VariableLine variableLine)
        {
            if (outputVariable != variableLine) { outputVariable.Checked = false; }
            inputVariables.ForEach(x => { if (x != variableLine) { x.Checked = false; } });
            selectedVariable = variableLine;
            RefreshStackPanel(varStackPanel, variableLine.MemberFunctions);
        }

        private void btnAddFunction_Click(object sender, RoutedEventArgs e)
        {
            if (selectedVariable != null)
            {
                selectedVariable.MemberFunctions.Add(new MemberFunction(selectedVariable.MemberFunctions.Count + 1));
                RefreshStackPanel(varStackPanel, selectedVariable.MemberFunctions);
            }
        }

        private void btnRemoveFunction_Click(object sender, RoutedEventArgs e)
        {
            if (selectedVariable != null && selectedVariable.MemberFunctions.Any())
            {
                selectedVariable.MemberFunctions.RemoveAt(selectedVariable.MemberFunctions.Count - 1);
                RefreshStackPanel(varStackPanel, selectedVariable.MemberFunctions);
            }
        }

        private void RefreshStackPanel(StackPanel panel, IEnumerable<StackPanel> elements)
        {
            panel.Children.Clear();
            foreach (var element in elements)
            {
                panel.Children.Add(element);
            }
        }

        private void btnRefreshRules_Click(object sender, RoutedEventArgs e)
        {
            var ruleCreatorDictionary = new Dictionary<VariableLine, List<MemberFunction>>();
            foreach (var variable in inputVariables)
            {
                ruleCreatorDictionary.Add(variable, variable.MemberFunctions);
            }
            ruleCreateLine = new RuleCreateLine(ruleCreatorDictionary, outputVariable, outputVariable.MemberFunctions);
            rulesCreatorStackPanel.Children.Clear();
            rulesCreatorStackPanel.Children.Add(ruleCreateLine);
        }

        private void btnAddRule_Click(object sender, RoutedEventArgs e)
        {
            var rule = ruleCreateLine.CreateRuleLine();
            if (!ruleLines.Contains(rule))
            {
                ruleLines.Add(ruleCreateLine.CreateRuleLine());
                listBoxRules.Items.Refresh();
            }
        }

        private void btnRemoveRule_Click(object sender, RoutedEventArgs e)
        {
            ruleLines.Remove((RuleLine)listBoxRules.SelectedItem);
            listBoxRules.Items.Refresh();
        }

        private void calcButton_Click(object sender, RoutedEventArgs e)
        {
            if (inputVariables.Any(v => v.Range.Length != 2)
                || inputVariables.Any(v => v.MemberFunctions.Any(mf => mf.FunctionPoints.Length != 3))
                || !ruleLines.Any())
            {
                MessageBox.Show("Invalid input");
                return;
            }
            CalculateOutput();
        }

        private void CalculateOutput()
        {
            var dictionary = new Dictionary<VariableLine, List<MemberFunction>>();
            foreach (var variable in inputVariables)
            {
                dictionary.Add(variable, variable.MemberFunctions);
            }
            
            processor.CreateInputVariables(dictionary);
            processor.CreateOutputVariables(outputVariable, outputVariable.MemberFunctions);
            processor.CreateRules(ruleLines, (minRadioButton.IsChecked == true)? NormOperator.MIN : NormOperator.PROD);
            tbxResult.Text= processor.CalculateOutput(inputTextBoxes.Select(tb => Convert.ToDouble(tb.Text)).ToList(), (DeffuzyficationType)cbxDefuzzifiction.SelectedIndex).ToString();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            SplashScreen screen = new SplashScreen("MamdaniSplashScreen.png");
            screen.Show(false);
            Thread.Sleep(2000);
            screen.Close(TimeSpan.Zero);
        }
    }
    public class VariableLine : StackPanel
    {
        public delegate void CheckBoxCheckedEventHandler(VariableLine variableLine);
        public event CheckBoxCheckedEventHandler CheckBoxChecked;

        public bool Checked { get => (bool)checkBox.IsChecked; set => checkBox.IsChecked = value; }
        public string VariableName { get => txtVariableName.Text; }
        public string[] Range { get => txtRange.Text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries); }
        public List<MemberFunction> MemberFunctions;

        CheckBox checkBox;
        TextBox txtVariableName;
        TextBox txtRange;
        Label label;

        public VariableLine(int number)
        {
            MemberFunctions = new List<MemberFunction>();

            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.Orientation = Orientation.Horizontal;

            checkBox = new CheckBox();
            checkBox.IsChecked = false;
            checkBox.Checked += checkbox_OnChecked;

            this.txtRange = new TextBox();
            this.txtRange.Width = 80;
            this.txtRange.Height = 20;

            this.txtVariableName = new TextBox();
            this.txtVariableName.Width = 150;
            this.txtVariableName.Height = 20;

            this.label = new Label();
            this.label.Content = number.ToString();

            this.Children.Add(checkBox);
            this.Children.Add(label);
            this.Children.Add(txtVariableName);
            this.Children.Add(txtRange);
        }

        private void checkbox_OnChecked(object sender, EventArgs args)
        {
            CheckBoxChecked(this);
        }

        public override string ToString()
        {
            return VariableName;
        }
    }

    public class MemberFunction : StackPanel
    {
        public FunctionType FunctionType { get => (FunctionType)cbxFunctionType.SelectedIndex; }
        public string SetName { get => txtSetName.Text; }
        public string[] FunctionPoints { get => txtFunctionPoints.Text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries); }

        Label label;
        ComboBox cbxFunctionType;
        TextBox txtSetName;
        TextBox txtFunctionPoints;

        public MemberFunction(int number)
        {
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.Orientation = Orientation.Horizontal;

            this.cbxFunctionType = new ComboBox();
            this.cbxFunctionType.Width = 80;
            this.cbxFunctionType.Height = 20;
            this.cbxFunctionType.ItemsSource = new List<string> { "Triangle" };
            this.cbxFunctionType.SelectedIndex = 0;

            this.txtFunctionPoints = new TextBox();
            this.txtFunctionPoints.Width = 80;
            this.txtFunctionPoints.Height = 20;

            this.txtSetName = new TextBox();
            this.txtSetName.Width = 200;
            this.txtSetName.Height = 20;

            this.label = new Label();
            this.label.Content = number.ToString();

            this.Children.Add(label);
            this.Children.Add(cbxFunctionType);
            this.Children.Add(txtSetName);
            this.Children.Add(txtFunctionPoints);
        }

        public override string ToString()
        {
            return SetName;
        }
    }

    public class RuleCreateLine : StackPanel
    {
        ListBox listBoxOperator;
        Dictionary<VariableLine,ListBox> variablesListBoxes;
        Dictionary<VariableLine, ListBox> outputVariableListBox;

        public RuleCreateLine(Dictionary<VariableLine, List<MemberFunction>> variablesFunctions,VariableLine outputVariable, List<MemberFunction> outputFunctions)
        {
            this.Orientation = Orientation.Horizontal;

            variablesListBoxes =new Dictionary<VariableLine, ListBox>();
            outputVariableListBox = new Dictionary<VariableLine, ListBox>();

            listBoxOperator = new ListBox();
            listBoxOperator.ItemsSource = new List<string> { "AND", "OR" };
            this.Children.Add(listBoxOperator);

            foreach( var variableFunction in variablesFunctions)
            {
                var label1 = new Label();
                label1.Content = variableFunction.Key.VariableName;
                var listBoxInput = new ListBox();
                listBoxInput.Width = 120;
                listBoxInput.Height = 80;
                listBoxInput.ItemsSource = variableFunction.Value;
                listBoxInput.DisplayMemberPath = "SetName";
                
                this.Children.Add(label1);
                this.Children.Add(listBoxInput);
                variablesListBoxes.Add(variableFunction.Key, listBoxInput);
            }

            var label2 = new Label();
            label2.Content = outputVariable.VariableName;
            var listBoxOutput = new ListBox();
            listBoxOutput.Width = 120;
            listBoxOutput.Height = 80;
            listBoxOutput.ItemsSource = outputFunctions;
            listBoxOutput.DisplayMemberPath = "SetName";

            this.Children.Add(label2);
            this.Children.Add(listBoxOutput);
            outputVariableListBox.Add(outputVariable, listBoxOutput);
        }

        public RuleLine CreateRuleLine()
        {
            var dictionary = new Dictionary<VariableLine, MemberFunction>();
            foreach (var pair in variablesListBoxes)
            {
                dictionary.Add(pair.Key, (MemberFunction) pair.Value.SelectedItem);
            }
            var tuple = new Tuple<VariableLine, MemberFunction>(outputVariableListBox.FirstOrDefault().Key, (MemberFunction)outputVariableListBox.FirstOrDefault().Value.SelectedItem);
            return new RuleLine((RuleOperator)listBoxOperator.SelectedIndex,dictionary, tuple);
        }
    }

    public class RuleLine
    {
        public RuleLine(RuleOperator ruleOperator, Dictionary<VariableLine, MemberFunction> variableFunction, Tuple<VariableLine, MemberFunction> ouputVariableFunction)
        {
            RuleOperator = ruleOperator;
            VariableFunction = variableFunction;
            OuputVariableFunction = ouputVariableFunction;
        }

        public RuleOperator RuleOperator { get; set; }
        public Dictionary<VariableLine, MemberFunction> VariableFunction {get; set; }
        public Tuple<VariableLine, MemberFunction> OuputVariableFunction { get; set; }

        public override string ToString()
        {
            string returnVal = "IF ";
            var statements = VariableFunction.Select(x => x.Key.VariableName + " = " + x.Value.SetName).ToList();

            statements.ForEach(x => returnVal +=" " + x + " " + RuleOperator.GetName(typeof(RuleOperator), RuleOperator));
            returnVal = returnVal.Substring(0, returnVal.Length - 3);
            returnVal += (" THEN " + OuputVariableFunction.Item1.VariableName + " = " + OuputVariableFunction.Item2.SetName);
            return returnVal;
        }
        public override bool Equals(object obj)
        {
            var ruleLine = obj as RuleLine;
            if (ruleLine == null)
            {
                return false;
            }
            if (ruleLine.ToString().Equals(this.ToString()))
            {
                return true;
            }
            return base.Equals(obj);            
        }
    }



    public enum FunctionType
    {
        TriangleMF
    }

    public enum DeffuzyficationType
    {
        LeftMax,
        CenterMax,
        RightMax
    }

}