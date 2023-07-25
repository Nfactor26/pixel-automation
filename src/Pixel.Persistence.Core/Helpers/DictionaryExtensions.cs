using System;
using System.Collections.Generic;
using System.Text;

namespace Pixel.Automation.Core
{
    public static class DictionaryExtensions
    {
        public static string ToCommaSeperateString(this Dictionary<string, string> dictionary)
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (var argument in dictionary)
            {
                sb.Append(argument.Key);
                sb.Append("=");
                sb.Append(argument.Value);
                i++;
                if (i < dictionary.Count)
                {
                    sb.Append(',');
                }
            }
            return sb.ToString();
        }

        public static Dictionary<string, string> ToDictionary(this string commanSeperatedKeyValuePair)
        {
            Dictionary<string, string> dictionary = new();
            if (!string.IsNullOrEmpty(commanSeperatedKeyValuePair))
            {
                foreach (var arg in commanSeperatedKeyValuePair.Split(','))
                {
                    var keyValuePair = arg.Split('=');
                    if (keyValuePair.Length != 2)
                    {
                        throw new ArgumentException($"Argument {arg} could not be parsed.");
                    }
                    dictionary.Add(keyValuePair[0], keyValuePair[1]);
                }
            }
            return dictionary;
        }
    }
}
