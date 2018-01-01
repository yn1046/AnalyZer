using System;
using System.Collections.Generic;
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

        public ICommand DoAnalyze { get; set; }

        public string[] ops = { "+", "-", "*", "/", "%", "**", "//" , "==", "!=",
        "<>", ">", "<", ">=", "<=", "=", "+=", "-=", "*=", "/=", "%=", "**=", "//=",
        "&", "|", "^", "~", "<<", ">>", "in", "not in", "is", "is not", "or", "and", "?"};

        public MainViewModel()
        {
            DoAnalyze = new DelegateCommand(Analyze);
            Model = new Models.Main();
        }

        public void Analyze()
        {
            var count = ops.Where(o => Model.Text.Contains(o)).Count();
            var UsedBranchingOperators = Model.Text.Split(' ').Where(el => (new Regex("((^if)|(^else)|(^elif))(:*)(\r*)(\n*)")).IsMatch(el)).ToList();
            var countBranches = UsedBranchingOperators.Count;
            MessageBox.Show($"Length: {Model.Text.Split('\n').Count()}\n" +
                $"Operators: {count}\n" +
                $"Branches: {countBranches}", "Results", MessageBoxButton.OK);
        }
    }
}
