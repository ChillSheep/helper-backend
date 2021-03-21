using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringUtils
{
    class StringUtils
    {
        public static string RemoveEverythingAfterLast(string input, string remove)
        {
            try { input = input.Substring(0, input.LastIndexOf(remove) + 1); } catch { }
            return input;
        }
        public static string RemoveEverythingAfterFirst(string input, string remove)
        {
            try { input = input.Substring(0, input.IndexOf(remove) + 1); } catch { } //if there is none
            return input;
        }
        public static string RemoveEverythingBeforeFirst(string input, string remove)
        {
            try { input = input.Substring(input.IndexOf(remove)); } catch { } //if there is none
            return input;
        }
        public static string RemoveEverythingBeforeLast(string input, string remove)
        {
            try { input = input = input.Substring(input.LastIndexOf(remove) + 1); } catch { }
            return input;
        }
        public static string RemoveLastChar(string s)
        {
            if (s == null || s.Length == 0)
            {
                return s;
            }
            return s.Substring(0, s.Length - 1);
        }
    }
}
