using AnalyZer.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AnalyZer.ViewModels
{
    public class MainViewModel
    {
        public Models.Main Model { get; set; }

        public ICommand DoAnalyzeGilb { get; set; }
        public ICommand DoAnalyzeChepin { get; set; }

        private delegate bool Criteria(int count, bool isControlling);

        public string[] operators = { "==", "!=", "<>", ">", "<", ">=", "<=", "&", @"\|", @"\^", "~", "<<", ">>",
            @"\bin\b", @"\bnot in\b", @"\bis\b", @"\bis not\b", @"\bor\b", @"\band\b", @"\?"};
        public string[] assignationOperators = { @"\+", "-", @"\*", "/", "%", @"\*\*", "//", "=", @"\+=", @"\-=", @"\*=", "/=", "%=", @"\*\*=", "//=" };

        public MainViewModel()
        {
            DoAnalyzeGilb = new DelegateCommand(AnalyzeGilb);
            DoAnalyzeChepin = new DelegateCommand(AnalyzeChepin);
            Model = new Models.Main();
            try
            {
                Model.Text = File.ReadAllText("progExample.txt");
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error reading file!\n{e.Message}", "Error", MessageBoxButton.OK);
            }
        }

        public void AnalyzeGilb()
        {
            var UsedBranchingOperators = Model.Text.Split(' ').Where(el => (new Regex("((^if)|(^else)|(^elif))(:*)(\r*)(\n*)")).IsMatch(el)).ToList();
            var Loops = Model.Text.Split(' ').Where(el => (new Regex("((^)|( ))(for|while)")).IsMatch(el)).ToList();
            var GilbCL = UsedBranchingOperators.Count + Loops.Count;
            var LinesWithOperators = Model.Text.Split('\n').Where(l => operators.Any(o => (new Regex($@"(^|[^\'|""])({o})($|[^\'|""])")).IsMatch(l)) 
            || assignationOperators.Any(o => (new Regex($@"(^|[^\'|""])({o})($|[^\'|""])")).IsMatch(l))).ToList();
            var Gilbcl = Math.Round((double)GilbCL / LinesWithOperators.Count, 3);

            var LinesList = Model.Text.Split('\n').ToList();
            int cliHere = 0, cli = 0, indHere, indExtIf = 0;
            bool underIf = false;
            for (var i = 0; i < LinesList.Count - 1; i++)
            {
                indHere = LinesList[i].GetIndentation();

                if (underIf && indExtIf >= indHere)
                {
                    underIf = false;
                    cliHere = 0;
                }
                if (underIf && (new Regex("( |^)(if |else|elif)")).IsMatch(LinesList[i]))
                {
                    cliHere++;
                    indExtIf = indHere;
                }
                if (!underIf && (new Regex("( |^)(if |else|elif)")).IsMatch(LinesList[i]))
                {
                    underIf = true;
                    indExtIf = indHere;
                    cliHere = 0;
                }

                if (cliHere > cli) cli = cliHere;
            }

            MessageBox.Show($"Code length: {Model.Text.Split('\n').Count()}\n" +
                $"Branches & loops (CL): {GilbCL}\n" +
                $"cl: {Gilbcl}\n" +
                $"cli: {cli}", "Results", MessageBoxButton.OK);
        }

        public void AnalyzeChepin()
        {
            var Lines = Model.Text.Split('\n').ToList();
            var idList = new List<string>();

            var result = "Classes and functions span:\n";
            FilterLines(Lines, @"(def|class) (\w+)\(?", ref idList, ref result);

            result += "\nVariables span:\n";
            FilterLines(Lines, @"( |^)+(\w+) ?= ?", ref idList, ref result);

            var enteredVariables = ClassifyVariables(Lines, @"(\w+) ?= ?input\(", (count, control) => count == 1 && !control);
            var modifiableVariables = ClassifyVariables(Lines, @"(\w+) ?= ?", (count, control) => count > 1 && !control);
            var controllingVariables = ClassifyVariables(Lines, @"(\w+) ?= ?", (count, control) => control);
            var parasiteVariables = ClassifyVariables(Lines, @"(\w+) ?= ?", (count, control) => true).Where(v => !(enteredVariables.Contains(v) 
            || modifiableVariables.Contains(v) || controllingVariables.Contains(v))).ToList();

            result += $"\nChepin metrics:\n" +
                $"P = {enteredVariables.Count}\n" +
                $"M = {modifiableVariables.Count}\n" +
                $"C = {controllingVariables.Count}\n" +
                $"T = {parasiteVariables.Count}\n" +
                $"\nQ = {enteredVariables.Count + 2*modifiableVariables.Count+ 3*controllingVariables.Count+ 0.5*parasiteVariables.Count}";

            var myWindow = new TextWindow(result);
            myWindow.Show();
        }

        private void FilterLines(List<string> Lines, string reg, ref List<string> idList, ref string result)
        {
            foreach (var line in Lines)
            {
                var matchId = Regex.Match(line, reg);
                if (matchId.Success)
                {
                    var id = matchId.Groups[2].Value.Trim();
                    if (!idList.Contains(id))
                    {
                        idList.Add(id);
                        var count = Lines.Where(l => (new Regex($@"(^|[^\'|""])\b({id})\b($|[^\'|""])")).IsMatch(l)).Count() - 1;
                        if (count != 0) result += $"{id}: {count}\n";
                    }
                }
            }
        }

        private List<string> ClassifyVariables(List<string> Lines, string reg, Criteria criteria)
        {
            var SuitableVariables = new List<string>();
            foreach (var line in Lines)
            {
                var matchId = Regex.Match(line, reg);
                if (matchId.Success)
                {
                    var id = matchId.Groups[1].Value.Trim();
                    if (!SuitableVariables.Contains(id))
                    {
                        var count = Lines.Where(l => assignationOperators.Any(o => (new Regex($@"(^|[^\'|""])\b({id})\b ?{o}($|[^\'|""])")).IsMatch(l))).Count();
                        var isControlling = Lines.Any(l => operators.Any(o => (new Regex($@"(^|[^\'|""])\b({id})\b($|[^\'|""])")).IsMatch(l)
                        && (new Regex($@"(^|[^\'|""])({o})($|[^\'|""])")).IsMatch(l)));
                        if (criteria(count, isControlling)) SuitableVariables.Add(id);
                    }
                }
            }
            return SuitableVariables;
        }

    }
}
