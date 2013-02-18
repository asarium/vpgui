using System;
using System.Runtime.InteropServices;

namespace VPSharp
{
    [StructLayout(LayoutKind.Sequential, Size = 16, CharSet = CharSet.Ansi)]
    [Serializable]
    internal struct Header
    {
        /// <summary>
        ///     Always "VPVP"
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public char[] header;

        /// <summary>
        ///     As of this version, still 2.
        /// </summary>
        public int version;

        /// <summary>
        ///     Offset to the file index
        /// </summary>
        public int diroffset;

        /// <summary>
        ///     Number of entries
        /// </summary>
        public int direntries;
    }

    [StructLayout(LayoutKind.Sequential, Size = 44)]
    [Serializable]
    internal struct Direntry
    {
        /// <summary>
        ///     Offset of the file data for this entry.
        /// </summary>
        public int offset;

        /// <summary>
        ///     Size of the file data for this entry
        /// </summary>
        public int size;

        /// <summary>
        ///     Null-terminated filename, directory name, or ".." for backdir
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string name;

        /// <summary>
        ///     Time the file was last modified, in unix time.
        /// </summary>
        public int timestamp;
    }
}
