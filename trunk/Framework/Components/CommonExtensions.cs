using System;
using System.Collections.Generic;
using System.Text;

namespace Components
{
    public static class CommonExtensions
    {
        public static string IsNull<T>(this string s)
        {
            string DatePattern = "0:MM/dd/yyyy";
            if (string.IsNullOrEmpty(s)) return "null";
            else if (typeof(T) == typeof(string)) return $"'{s}'";
            else if (typeof(T) == typeof(int) || typeof(T) == typeof(long)) return $"{s}";
            else if (typeof(T) == typeof(DateTime)) return $"'{Convert.ToDateTime(s).ToString(DatePattern) }'";
            else return s;
        }
    }
}
