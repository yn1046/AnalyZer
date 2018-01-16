using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyZer.Models
{
    public class Text
    {
        public string Content { get; set; }

        public Text(string str = "") => Content = str;
    }
}
