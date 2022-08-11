using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EleCho.JsonModelGenerator.Utils
{
    internal class StrUtils
    {
        public static List<string> SplitIdentifier(string str)
        {
            List<string> result = new List<string>();
            StringBuilder sb = new StringBuilder();

            if (char.IsNumber(str[0]))
                throw new ArgumentException("Invalid string");

            foreach (char c in str)
            {
                if (char.IsUpper(c))
                {
                    if (sb.Length > 0)
                        result.Add(sb.ToString());
                    sb.Clear();

                    sb.Append(c);
                }
                else if (c == '_')
                {
                    if (sb.Length > 0)
                        result.Add(sb.ToString());
                    sb.Clear();
                }
                else if (char.IsLower(c) || char.IsNumber(c))
                {
                    sb.Append(c);
                    continue;
                }
                else
                {

                    throw new ArgumentException("Invalid string");
                }
            }

            if (sb.Length > 0)
                result.Add(sb.ToString());

            return result;
        }

        public static string MakePascal(IEnumerable<string> words)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var word in words)
            {
                sb.Append(char.ToUpper(word[0]));
                sb.Append(word[1..].ToLower());
            }

            return sb.ToString();
        }

        public static string MakeCamel(IEnumerable<string> words)
        {
            if (!words.Any())
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            string fistWord = words.First();
            sb.Append(char.ToLower(fistWord[0]));
            sb.Append(fistWord[1..]);

            foreach (var word in words.Skip(1))
            {
                sb.Append(char.ToUpper(word[0]));
                sb.Append(word[1..].ToLower());
            }

            return sb.ToString();
        }

        public static string MakeSnake(IEnumerable<string> words)
        {
            return string.Join('_', words.Select(v => v.ToLower()));
        }

        /// <summary>
        /// 单词变成复数形式
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string MakePlural(string name)
        {
            Regex plural1 = new Regex("(?<keep>[^aeiou])y$");
            Regex plural2 = new Regex("(?<keep>[aeiou]y)$");
            Regex plural3 = new Regex("(?<keep>[sxzh])$");
            Regex plural4 = new Regex("(?<keep>[^sxzhy])$");

            if (plural1.IsMatch(name))
                return plural1.Replace(name, "${keep}ies");
            else if (plural2.IsMatch(name))
                return plural2.Replace(name, "${keep}s");
            else if (plural3.IsMatch(name))
                return plural3.Replace(name, "${keep}es");
            else if (plural4.IsMatch(name))
                return plural4.Replace(name, "${keep}s");

            return name;
        }
    }
}
