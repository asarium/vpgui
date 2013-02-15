﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPSharp.Utilities
{
    internal static class Util
    {
        public static int GetFittingIndex<T>(this ICollection<T> enumerable, T element) where T : IComparable<T>
        {
            int index = 0;

            foreach (T item in enumerable)
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
