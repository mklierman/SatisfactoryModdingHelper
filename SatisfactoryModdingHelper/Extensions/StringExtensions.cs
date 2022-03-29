using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatisfactoryModdingHelper.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Replaces all instances of the backtick character, `, with quotation marks
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SetQuotes(this string str)
        {
            return str.Replace("`", "\"");
        }
    }
}
