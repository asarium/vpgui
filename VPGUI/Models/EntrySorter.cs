using System;
using System.Collections;
using VPSharp.Entries;

namespace VPGUI.Models
{
    internal class EntrySorter : IComparer
    {
        public int Compare(object x, object y)
        {
            var entryX = x as IEntryView<VPEntry>;
            var entryY = y as IEntryView<VPEntry>;

            if (entryX == entryY)
            {
                return 0;
            }

            if (entryX == null)
            {
                return -1;
            }

            if (entryY == null)
            {
                return 1;
            }

            if (entryX.Entry == entryY.Entry)
            {
                return 0;
            }

            if (entryX.Entry == null)
            {
                return -1;
            }

            if (entryX.Entry == null)
            {
                return 1;
            }

            return entryX.Entry.CompareTo(entryY.Entry);
        }
    }
}