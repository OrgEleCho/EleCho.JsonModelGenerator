using EleCho.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EleCho.JsonModelGenerator.Utils
{
    internal class NumUtils
    {
        public static string GetNumType(JsonNumber num)
        {
            bool issigned = num.Value.Contains('-');
            bool isfloat = num.Value.Contains('.');

            if (isfloat)
            {
                return "double";
            }
            else
            {
                if (issigned)
                {
                    long max = num.GetLongValue();

                    if (max <= int.MaxValue)
                        return "int";

                    return "long";
                }
                else
                {
                    ulong max = num.GetULongValue();

                    if (max <= int.MaxValue)
                        return "int";
                    if (max <= uint.MaxValue)
                        return "uint";
                    if (max <= long.MaxValue)
                        return "long";

                    return "ulong";
                }
            }
        }
        public static string GetNumType(IEnumerable<JsonNumber> nums)
        {
            bool issigned = false;
            bool isfloat = false;

            foreach (JsonNumber num in nums)
            {
                issigned = num.Value.Contains('-');
                if (issigned)
                    break;
            }

            foreach (JsonNumber num in nums)
            {
                isfloat = num.Value.Contains('-');
                if (isfloat)
                    break;
            }

            if (isfloat)
            {
                return "double";
            }
            else
            {
                if (issigned)
                {
                    long max = 0;
                    foreach (JsonNumber num in nums)
                    {
                        var numV = num.GetLongValue();
                        if (numV > max)
                            max = numV;
                    }

                    if (max <= int.MaxValue)
                        return "int";

                    return "long";
                }
                else
                {
                    ulong max = 0;
                    foreach (JsonNumber num in nums)
                    {
                        var numV = num.GetULongValue();
                        if (numV > max)
                            max = numV;
                    }

                    if (max <= int.MaxValue)
                        return "int";
                    if (max <= uint.MaxValue)
                        return "uint";
                    if (max <= long.MaxValue)
                        return "long";

                    return "ulong";
                }
            }
        }
    }
}
