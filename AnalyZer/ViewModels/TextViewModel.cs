using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyZer.ViewModels
{
    public class TextViewModel
    {
        public Models.Text Model { get; set; }

        public TextViewModel(string str)
        {
            Model = new Models.Text(str);
        }
    }
}
