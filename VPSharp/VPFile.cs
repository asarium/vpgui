using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using VPSharp.Entries;
using VPSharp.Utilities;

namespace VPSharp
{
    /// <summary>
    /// Provides utility functions for VP-file handling
    /// </summary>
    public static class VPUtilities
    {
        /// <summary>
        /// Checks if the given path points to a valid VP-File
        /// </summary>
        /// <param name="path">The filepath</param>
        /// <returns>true when the filepath points to a valid VP-file, false otherwise</returns>
        public static bool CheckVPFile(string path)
        {
            try
            {
                using (MemoryMappedFile stream = MemoryMappedFile.CreateFromFile(path))
                {
                    try
                    {
                        // Throws exception when the stream is not valid.
                        CheckVPStream(stream).Wait();

                        return true;
                    }
                    catch (ArgumentException)
                    {
                        return false;
                    }
                }
            }
            catch (IOException)
            {
                return false;
            }
        }

        internal static async Task<Header> CheckVPStream(MemoryMappedFile file)
        {
            using (MemoryMappedViewStream accessor = file.CreateViewStream(0, Marshal.SizeOf(typeof(Header)), MemoryMappedFileAccess.Read))
            {
                Header header = await accessor.ReadStructAsync<Header>();

                CheckHeader(header);

                return header;
            }
        }

        /// <summary>
        /// Validates the given header.
        /// </summary>
        /// <param name="header">The header which should be validated.</param>
        /// <exception cref="ArgumentException">Thrown when the given header is not valid.</exception>
        internal static void CheckHeader(Header header)
        {
            // Check the header
            char[] h = header.header;
            if (!(h[0] == 'V' &&
                h[1] == 'P' &&
                h[2] == 'V' &&
                h[3] == 'P'))
            {
                throw new ArgumentException("No VP file, first four bytes did not match.");
            }

            if (header.version > VPFile.MAX_VERSION)
            {
                throw new ArgumentException(String.Format("Version {0} is not supported", header.version));
            }
        }
    }

    /// <summary>
    /// Main class for handling VP-files. Provides ways to read and write VP-files.
    /// </summary>
    public sealed class VPFile : IDisposable
    {
        internal static readonly int MAX_VERSION = 2;

        private MemoryMappedFile MemoryFile;

        private bool Disposed = false;

        private Header FileHeader;

        private List<Direntry> DirEntries = null;

        /// <summary>
        /// The root node of the vp-file which contains the tree of all entries in this file.
        /// </summary>
        public VPDirectoryEntry RootNode
        {
            get;

            internal set;
        }

        public bool SortEntries
        {
            get;

            set;
        }

        public FileInfo VPFileInfo
        {
            get;
            internal set;
        }

        public VPFileMessages FileMessages
        {
            get;

            internal set;
        }

        /// <summary>
        /// Constructs a new empty VP-file.
        /// </summary>
        public VPFile()
        {
            RootNode = new VPDirectoryEntry(this);
        }

        /// <summary>
        /// Constructs a VP-file which uses the given file
        /// </summary>
        /// <param name="path">The path of the VP-file</param>
        public VPFile(string path)
            : this(new FileStream(path, FileMode.Open))
        {
        }

        /// <summary>
        /// Constructs a VP-file from the given FileStream.
        /// </summary>
        /// <param name="stream">A stream of the VP-File</param>
        public VPFile(FileStream stream)
        {
            VPFileInfo = new FileInfo(stream.Name);

            MemoryFile = MemoryMappedFile.CreateFromFile(stream, VPFileInfo.Name, 0,
                MemoryMappedFileAccess.Read, new MemoryMappedFileSecurity(),
                HandleInheritability.None, false);

            SortEntries = true;
        }

        /// <summary>
        /// Disposes any resources which are used by the file.
        /// </summary>
        ~VPFile()
        {
            if (!Disposed)
            {
                this.Dispose();
            }
        }

        /// <summary>
        /// Disposes any resources which are used by the file.
        /// </summary>
        public void Dispose()
        {
            if (Disposed)
            {
                return;
            }
            else
            {
                Close();

                Disposed = true;
            }
        }

        /// <summary>
        /// Closes all connections to underlying resources.
        /// </summary>
        public void Close()
        {
            if (MemoryFile != null)
            {
                MemoryFile.Dispose();
                MemoryFile = null;
            }
        }

        /// <summary>
        /// Asynchonously reads index of the VP file.
        /// </summary>
        /// <returns></returns>
        public async Task ReadIndexAsync()
        {
            FileMessages = new VPFileMessages();

            if (MemoryFile == null)
            {
                throw new InvalidOperationException("File not in reading state!");
            }
            else
            {
                await ReadHeader();

                await ReadDirEntries();

                InitializeRootNode();
            }
        }

        private async Task ReadHeader()
        {
            FileHeader = await VPUtilities.CheckVPStream(MemoryFile);
        }

        private async Task ReadDirEntries()
        {
            int entrySize = Marshal.SizeOf(typeof(Direntry));

            int offset = FileHeader.diroffset;
            int size = FileHeader.direntries * entrySize;

            int end = offset + size;

            int diff = (int) VPFileInfo.Length - offset;

            // There are files that have a corrupted number of dir entries, this should take care of that.
            using (MemoryMappedViewStream accessor = MemoryFile.CreateViewStream
                (offset, Math.Min(size, diff), MemoryMappedFileAccess.Read))
            {
                DirEntries = new List<Direntry>(diff / entrySize);

                try
                {
                    while ((accessor.Length - accessor.Position) >= entrySize)
                    {
                        DirEntries.Add(await accessor.ReadStructAsync<Direntry>());
                    }

                    if (DirEntries.Count != FileHeader.direntries)
                    {
                        FileMessages.AddMessage(new VPFileMessage(MessageType.WARNING,
                            String.Format("Illegal count of entries specified in file header! Header says {0} but got {1}.", FileHeader.direntries, DirEntries.Count)));
                    }
                }
                catch (IOException e)
                {
                    throw new ArgumentException("Error while reading direntries. The file is " +
                        "probably corrupt.", e);
                }
            }
        }

        private void InitializeRootNode()
        {
            RootNode = new VPDirectoryEntry(this);
            RootNode.ChangedOverride = true;

            VPDirectoryEntry currentParent = RootNode;

            foreach (Direntry entry in DirEntries)
            {
                // Accodring to the documentation offset should be zero, but it isn't...
                if (entry.size == 0 && entry.timestamp == 0)
                {
                    // This is a directory

                    if (entry.name == "..")
                    {
                        // backdir

                        currentParent = currentParent.Parent;

                        if (currentParent == null)
                        {
                            // End of files...
                            break;
                        }
                    }
                    else
                    {
                        VPDirectoryEntry directory = new VPDirectoryEntry(currentParent);
                        directory.DirEntry = entry;

                        currentParent.AddChild(directory);

                        currentParent = directory;
                    }
                }
                else
                {
                    // This is a file
                    VPFileEntry fileEntry = new VPFileEntry(currentParent);
                    fileEntry.DirEntry = entry;

                    currentParent.AddChild(fileEntry);
                }
            }
        }

        internal Stream GetFileStream(long offset, long size)
        {
            return MemoryFile.CreateViewStream(offset, size, MemoryMappedFileAccess.Read);
        }

        public delegate bool OverwriteCallback(FileInfo toBeOverwritten);

        /// <summary>
        /// Asynchronoulsy writes the current contents of the file to the specifed path
        /// </summary>
        /// <param name="outPath">The path where the file is written to. null if the current file should
        /// be overwritte.</param>
        /// <returns></returns>
        public async Task<string> WriteVPAsync(string outPath = null, OverwriteCallback callback = null)
        {
            if (outPath == null)
            {
                outPath = VPFileInfo.FullName;
            }

            string originalPath = outPath;
            if (File.Exists(outPath))
            {
                outPath = outPath + ".out";
            }

            bool success = false;

            using (FileStream stream = new FileStream(outPath, FileMode.Create))
            {
                await stream.WriteStructAsync<Header>(ComputeHeaderValues());

                Dictionary<VPFileEntry, int> offsetDictionary = new Dictionary<VPFileEntry, int>();

                foreach (VPEntry entry in RootNode.ChildrenRecursive())
                {
                    if (entry is VPFileEntry)
                    {
                        VPFileEntry file = entry as VPFileEntry;

                        offsetDictionary[file] = (int) stream.Position;

                        using (Stream st = file.OpenFileStream())
                        {
                            await file.OpenFileStream().CopyToAsync(stream);
                        }
                    }
                }

                await WriteDirEntriesRecursive(stream, RootNode, offsetDictionary);

                success = true;
            }

            if (success && originalPath != outPath)
            {
                if (callback == null || callback(new FileInfo(originalPath)))
                {
                    File.Delete(originalPath);

                    File.Move(outPath, originalPath);

                    File.Delete(outPath);
                }
                else
                {
                    throw new IOException("Callback did not allow the file to be overwritten!");
                }
            }

            return originalPath;
        }

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private async Task WriteDirEntriesRecursive(Stream stream, VPDirectoryEntry parentEntry,
            Dictionary<VPFileEntry, int> offsetDictionary)
        {
            Direntry direntry;

            foreach (VPEntry entry in parentEntry.Children)
            {
                if (entry is VPFileEntry)
                {
                    VPFileEntry file = entry as VPFileEntry;

                    direntry.offset = offsetDictionary[file];
                    direntry.size = file.FileSize;
                    direntry.name = file.Name;

                    direntry.timestamp = (int) (file.LastModified - UnixEpoch).TotalSeconds;

                    await stream.WriteStructAsync<Direntry>(direntry);
                }
                else
                {
                    // Write directory entry
                    direntry.offset = 0;
                    direntry.size = 0;
                    direntry.timestamp = 0;
                    direntry.name = entry.Name;

                    await stream.WriteStructAsync<Direntry>(direntry);

                    await WriteDirEntriesRecursive(stream, entry as VPDirectoryEntry, offsetDictionary);

                    // Write backdir
                    direntry.offset = 0;
                    direntry.size = 0;
                    direntry.timestamp = 0;
                    direntry.name = "..";

                    await stream.WriteStructAsync<Direntry>(direntry);
                }
            }
        }

        private Header ComputeHeaderValues()
        {
            Header header;

            header.header = new char[] { 'V', 'P', 'V', 'P' };
            header.version = MAX_VERSION;

            ComputeDirHeaderValues(out header.direntries, out header.diroffset);

            return header;
        }

        private void ComputeDirHeaderValues(out int direntries, out int diroffset)
        {
            diroffset = Marshal.SizeOf(typeof(Header)); // add size of header first
            direntries = 0;

            TraverseEntriesRecursive(RootNode, ref direntries, ref diroffset);
        }

        private void TraverseEntriesRecursive(VPDirectoryEntry parentDir, ref int direntries, ref int diroffset)
        {
            foreach (VPEntry entry in parentDir.Children)
            {
                if (entry is VPFileEntry)
                {
                    VPFileEntry file = (VPFileEntry) entry;

                    diroffset += file.FileSize;
                    direntries++;
                }
                else
                {
                    VPDirectoryEntry dirEntry = (VPDirectoryEntry) entry;

                    TraverseEntriesRecursive(dirEntry, ref direntries, ref diroffset);

                    direntries++; // Backdir at end of directory
                }
            }
        }
    }
}
