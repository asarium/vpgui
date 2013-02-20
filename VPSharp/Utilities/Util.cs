using System;
using System.Collections.Generic;

namespace VPSharp.Utilities
{
    internal static class Util
    {
        public static int GetFittingIndex<T>(this IEnumerable<T> enumerable, T element) where T : IComparable<T>
        {
            var index = 0;

            foreach (var item in enumerable)
            {
                if (element.CompareTo(item) < 0)
                {
                    return index;
                }

                index++;
            }

            return index;
        }
    }
}
