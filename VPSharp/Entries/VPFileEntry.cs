using System;
using System.IO;

namespace VPSharp.Entries
{
    /// <summary>
    /// A file entry of a VP-file. Offers ways to open a 
    /// read-only stream and query informations about the file.
    /// </summary>
    public class VPFileEntry : VPEntry
    {
        /// <summary>
        /// Constructs the file entry with the given VPFile and parent.
        /// </summary>
        /// <param name="file">The containing file, used to open the read-streams</param>
        /// <param name="parent">The parent directory.</param>
        internal VPFileEntry(VPDirectoryEntry parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Opens a stream to read the contents of the file stream.
        /// </summary>
        /// <returns>A stream instance which will read the contents of the file.</returns>
        public virtual Stream OpenFileStream()
        {
            return ContainingFile.GetFileStream(DirEntry.offset, DirEntry.size);
        }

        /// <summary>
        /// Specifies when the file was last modified.
        /// </summary>
        public virtual DateTime LastModified
        {
            get
            {
                // Convert from Unix-Time
                return (new DateTime(1970, 1, 1, 0, 0, 0)).AddSeconds(DirEntry.timestamp);
            }
        }

        /// <summary>
        /// Gets the size of the file in bytes.
        /// </summary>
        public virtual int FileSize
        {
            get
            {
                return DirEntry.size;
            }
        }
    }

    /// <summary>
    /// Symbolizes a file entry which is an actual file in the filesystem to make adding files
    /// to a VP-file possible.
    /// </summary>
    public class VPFileSystemEntry : VPFileEntry
    {
        private string _filePath;

        /// <summary>
        /// The file-system path of the referenced file.
        /// </summary>
        public string FilePath
        {
            get
            {
                return _filePath;
            }

            internal set
            {
                if (!File.Exists(value))
                {
                    throw new ArgumentException("The file does not exist!");
                }

                FileAttributes attr = File.GetAttributes(value);

                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    throw new ArgumentException("The path points to a directory!");
                }

                _filePath = value;

                _filePathInfo = new FileInfo(_filePath);
            }
        }

        private FileInfo _filePathInfo;

        /// <summary>
        /// Creates a new file entry under the given parent which references the given file path.
        /// </summary>
        /// <param name="file">The containing VP-file</param>
        /// <param name="parent">The parent firectory</param>
        /// <param name="filePath">The filepath of the file</param>
        /// <exception cref="ArgumentException">Thrown when the filepath is not valid.</exception>
        public VPFileSystemEntry(VPDirectoryEntry parent, string filePath)
            : base(parent)
        {
            FilePath = filePath;
        }

        /// <summary>
        /// Opens a FileStream to read the contents of the file.
        /// </summary>
        /// <returns>A FileStream instance of the referenced filepath.</returns>
        public override Stream OpenFileStream()
        {
            return new FileStream(FilePath, FileMode.Open);
        }

        /// <summary>
        /// Gets the time the file was last modified.
        /// </summary>
        public override DateTime LastModified
        {
            get
            {
                return File.GetLastWriteTime(FilePath);
            }
        }

        /// <summary>
        /// Gets the file size in bytes.
        /// </summary>
        public override int FileSize
        {
            get
            {
                return (int) _filePathInfo.Length;
            }
        }

        public override bool Changed
        {
            get
            {
                return true;
            }

            internal set
            {
            }
        }

        public override string Name
        {
            get
            {
                return _filePathInfo.Name;
            }
        }
    }
}
