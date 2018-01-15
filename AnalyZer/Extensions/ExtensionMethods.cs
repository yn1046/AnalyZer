using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyZer.Extensions
{
    public static class ExtensionMethods
    {
        public static int GetIndentation(this string str)
        {
            var count = 0;
            for (var i = 0; str[i] == ' '; i++) count++;
            return count;
        }
    }
}
