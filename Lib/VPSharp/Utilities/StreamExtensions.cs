﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace VPSharp.Utilities
{
    // Copied from http://stackoverflow.com/questions/4159184/c-read-structures-from-binary-file
    internal static class StreamExtensions
    {
        public static T ReadStruct<T>(this Stream stream) where T : struct
        {
            int sz = Marshal.SizeOf(typeof (T));
            var buffer = new byte[sz];
            stream.Read(buffer, 0, sz);

            GCHandle pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            T structure = (T) Marshal.PtrToStructure(
                pinnedBuffer.AddrOfPinnedObject(), typeof (T));

            pinnedBuffer.Free();
            return structure;
        }

        public static async Task<T> ReadStructAsync<T>(this Stream stream) where T : struct
        {
            int sz = Marshal.SizeOf(typeof (T));
            var buffer = new byte[sz];
            await stream.ReadAsync(buffer, 0, sz);

            GCHandle pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            T structure = (T) Marshal.PtrToStructure(
                pinnedBuffer.AddrOfPinnedObject(), typeof (T));

            pinnedBuffer.Free();
            return structure;
        }

        public static void WriteStruct<T>(this Stream stream, T val) where T : struct
        {
            int sz = Marshal.SizeOf(typeof (T));
            var buffer = new byte[sz];

            GCHandle pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                Marshal.StructureToPtr(val, pinnedBuffer.AddrOfPinnedObject(), false);

                stream.Write(buffer, 0, buffer.Length);
            }
            finally
            {
                if (pinnedBuffer != null)
                {
                    pinnedBuffer.Free();
                }
            }
        }

        public static async Task WriteStructAsync<T>(this Stream stream, T val) where T : struct
        {
            int sz = Marshal.SizeOf(typeof (T));
            var buffer = new byte[sz];

            GCHandle pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                Marshal.StructureToPtr(val, pinnedBuffer.AddrOfPinnedObject(), false);

                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
            finally
            {
                if (pinnedBuffer != null)
                {
                    pinnedBuffer.Free();
                }
            }
        }
    }
}
