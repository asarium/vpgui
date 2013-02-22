using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using VPSharp.Utilities;

namespace VPSharp.Entries
{
    /// <summary>
    ///     A directory in a VPFile which keeps track of its children.
    /// </summary>
    public class VPDirectoryEntry : VPEntry
    {
        private int _aggregatedFileSize;
        private DateTime _lastModified;

        /// <summary>
        ///     Constructs the directory without a prarent, used for the root node.
        /// </summary>
        /// <param name="file">The containing file</param>
        internal VPDirectoryEntry(VPFile file)
            : base(file)
        {
            Children = new ObservableCollection<VPEntry>();
            SubDirectories = new ObservableCollection<VPDirectoryEntry>();

            AggregatedFileSize = 0;
            this.LastModified = DateTime.MinValue;
        }

        /// <summary>
        ///     Constructs a new directory which has a parent.
        /// </summary>
        /// <param name="file">The containing file</param>
        /// <param name="parent">The parent directory of this directory</param>
        public VPDirectoryEntry(VPDirectoryEntry parent)
            : base(parent)
        {
            Children = new ObservableCollection<VPEntry>();
            SubDirectories = new ObservableCollection<VPDirectoryEntry>();

            AggregatedFileSize = 0;
            this.LastModified = DateTime.MinValue;
        }

        /// <summary>
        ///     The Children of this directory.
        /// </summary>
        public ObservableCollection<VPEntry> Children { get; private set; }

        /// <summary>
        ///     The sub-directories of this directory.
        /// </summary>
        public ObservableCollection<VPDirectoryEntry> SubDirectories { get; private set; }

        public int AggregatedFileSize
        {
            get { return _aggregatedFileSize; }

            private set
            {
                if (_aggregatedFileSize != value)
                {
                    _aggregatedFileSize = value;

                    OnPropertyChanged();
                }
            }
        }

        public DateTime LastModified
        {
            get { return this._lastModified; }

            private set
            {
                if (this._lastModified != value)
                {
                    this._lastModified = value;

                    OnPropertyChanged();
                }
            }
        }

        private int _totalDirectoryCount = 0;
        public int TotalDirectoryCount
        {
            get { return _totalDirectoryCount; }
            
            set
            {
                if (_totalDirectoryCount != value)
                {
                    _totalDirectoryCount = value;

                    OnPropertyChanged();
                }
            }
        }

        private int _totalChildrenCount = 0;
        public int TotalChildrenCount
        {
            get
            {
                return _totalChildrenCount;
            }

            set
            {
                if (_totalChildrenCount != value)
                {
                    _totalChildrenCount = value;

                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Adds a child to this directory. Also sets a new parent for the specified entry.
        /// </summary>
        /// <param name="entry">The entry which will be added.</param>
        public bool AddChild(VPEntry entry, bool remove = true)
        {
            if (Children.Any(item => item == entry || item.Name == entry.Name))
            {
                throw new ArgumentException("Item already exists.");
            }

            if (remove && entry.Parent != null)
            {
                entry.Parent.RemoveChild(entry);
            }

            entry.Parent = this;

            if (ContainingFile.SortEntries)
            {
                Children.Insert(Children.GetFittingIndex(entry), entry);

                var item = entry as VPDirectoryEntry;
                if (item != null)
                {
                    SubDirectories.Insert(SubDirectories.GetFittingIndex(item), item);

                    TotalDirectoryCount++;
                }
            }
            else
            {
                Children.Add(entry);

                var item = entry as VPDirectoryEntry;
                if (item != null)
                {
                    SubDirectories.Add(item);

                    TotalDirectoryCount++;
                }
            }
            TotalChildrenCount++;

            this.UpdateProperties(entry);

            entry.PropertyChanged += EntryOnPropertyChanged;

            return true;
        }

        private void EntryOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            string str = propertyChangedEventArgs.PropertyName;
            switch (str)
            {
                case "AggregatedFileSize":
                case "FileSize":
                case "LastModified":
                    this.RecalculateProperties();
                    break;
                case "TotalDirectoryCount":
                    this.TotalDirectoryCount = this.SubDirectories.Count + this.SubDirectories.Sum(dir => dir.TotalDirectoryCount);
                    break;
                case "TotalChildrenCount":
                    this.TotalChildrenCount = this.Children.Count + this.SubDirectories.Sum(dir => dir.TotalChildrenCount);
                    break;
            }
        }

        private void RecalculateProperties()
        {
            int newSize = 0;
            var newTime = DateTime.MinValue;

            foreach (var item in Children)
            {
                SelectNewValues(item, ref newTime, ref newSize);
            }

            AggregatedFileSize = newSize;
            LastModified = newTime;
        }

        private static void SelectNewValues(VPEntry entry, ref DateTime newLastModified, ref int newFileSize)
        {
            var file = entry as VPFileEntry;
            if (file != null)
            {
                newFileSize = newFileSize + file.FileSize;

                if (newLastModified < file.LastModified)
                {
                    newLastModified = file.LastModified;
                }
            }
            else
            {
                var dir = entry as VPDirectoryEntry;

                if (dir != null)
                {
                    newFileSize = newFileSize + dir.AggregatedFileSize;

                    if (newLastModified < dir.LastModified)
                    {
                        newLastModified = dir.LastModified;
                    }
                }
            }
        }


        private void UpdateProperties(VPEntry changedEntry, bool removed = false)
        {
            if (!removed)
            {
                int newSize = AggregatedFileSize;
                var newDate = LastModified;

                SelectNewValues(changedEntry, ref newDate, ref newSize);

                AggregatedFileSize = newSize;
                LastModified = newDate;
            }
            else
            {
                int size;

                var file = changedEntry as VPFileEntry;
                if (file != null)
                {
                    size = file.FileSize;
                }
                else
                {
                    var dir = changedEntry as VPDirectoryEntry;

                    size = dir.AggregatedFileSize;
                }

                AggregatedFileSize = AggregatedFileSize - size;

                LastModified = Children.Max(item =>
                    {
                        var fileEntry = item as VPFileEntry;

                        return fileEntry != null ? fileEntry.LastModified : (item as VPDirectoryEntry).LastModified;
                    });
            }
        }

        /// <summary>
        ///     Removes the given entry from this directory
        /// </summary>
        /// <param name="entry">The entry which will be removed</param>
        /// <returns>true when the entry got removed, false otherwise</returns>
        public bool RemoveChild(VPEntry entry)
        {
            bool removed = Children.Remove(entry);

            var vpDirectoryEntry = entry as VPDirectoryEntry;
            if (vpDirectoryEntry != null)
            {
                SubDirectories.Remove(vpDirectoryEntry);

                TotalDirectoryCount--;
            }
            TotalChildrenCount--;

            if (removed)
            {
                entry.Parent = null;
                entry.PropertyChanged -= EntryOnPropertyChanged;

                this.UpdateProperties(entry, true);
            }

            return removed;
        }

        /// <summary>
        ///     Enumerates the children of this directory recursively.
        /// </summary>
        /// <returns>
        ///     An IEnumerable which specifies the entries
        ///     in the order in which they appear in the directory structure.
        /// </returns>
        public IEnumerable<VPEntry> ChildrenRecursive()
        {
            foreach (VPEntry entry in Children)
            {
                yield return entry;

                if (entry is VPDirectoryEntry)
                {
                    foreach (VPEntry child in ((VPDirectoryEntry) entry).ChildrenRecursive())
                    {
                        yield return child;
                    }
                }
            }
        }

        /// <summary>
        ///     Searches for the given path under this directory
        /// </summary>
        /// <param name="entryPath">The path to search for</param>
        /// <returns>The correspondig VPEntry, or null if it wasn't found.</returns>
        public VPEntry GetByPath(string entryPath)
        {
            if (!entryPath.StartsWith("/"))
            {
                entryPath = "/" + entryPath;
            }

            foreach (VPEntry entry in this.ChildrenRecursive())
            {
                if (entry.Path == entryPath)
                {
                    return entry;
                }
            }

            return null;
        }

        /// <summary>
        ///     Gets the first item with the specified name.
        /// </summary>
        /// <param name="name">The name of the entry</param>
        /// <returns>The entry of null if it could not be found.</returns>
        public VPEntry GetByName(string name)
        {
            return this.ChildrenRecursive().FirstOrDefault(entry => entry.Name == name);
        }

        internal void RecalculateChangedStatus()
        {
            this.Changed = Children.Any(entry => entry.Changed);
        }

        public bool HasSubdirectory(VPDirectoryEntry vpEntry)
        {
            foreach (var dir in SubDirectories)
            {
                if (dir == vpEntry)
                {
                    return true;
                }
                else
                {
                    dir.HasSubdirectory(vpEntry);
                }
            }

            return false;
        }
    }
}
