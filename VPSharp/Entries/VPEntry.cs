using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace VPSharp.Entries
{
    /// <summary>
    ///     An abstract entry in a VP-file. Can either be a VPFileEntry or a VPDirectoryEntry
    /// </summary>
    public abstract class VPEntry : INotifyPropertyChanged, IComparable<VPEntry>
    {
        private bool _changed;
        internal Direntry? _dirEntry = null;
        private string _name;
        private VPDirectoryEntry _parent;

        internal VPEntry(VPFile containing)
        {
            this.ContainingFile = containing;
        }

        /// <summary>
        ///     Initializes the entry in the given VPFile and with the given parent
        /// </summary>
        /// <param name="file">The VPFile object which contains this entry</param>
        /// <param name="parent">The parent of this entry, can be null for the root entry</param>
        internal VPEntry(VPDirectoryEntry parent)
        {
            if (parent != null)
            {
                ContainingFile = parent.ContainingFile;
            }
        }

        /// <summary>
        ///     The VPFile which contains this entry.
        /// </summary>
        internal VPFile ContainingFile { get; set; }

        /// <summary>
        ///     Specifies if this entry has changed.
        /// </summary>
        public virtual bool Changed
        {
            get { return _changed; }

            internal set
            {
                if (_changed != value)
                {
                    _changed = value;

                    if (Parent != null)
                    {
                        if (_changed)
                        {
                            Parent.Changed = value;
                        }
                        else
                        {
                            Parent.RecalculateChangedStatus();
                        }
                    }

                    OnPropertyChanged();
                }
            }
        }

        internal Direntry DirEntry
        {
            get
            {
                if (_dirEntry.HasValue)
                {
                    return _dirEntry.Value;
                }
                else
                {
                    throw new InvalidOperationException("DirEntry not set!");
                }
            }

            set
            {
                _dirEntry = value;

                if (_dirEntry.HasValue)
                {
                    Name = _dirEntry.Value.name;
                }
            }
        }

        /// <summary>
        ///     The parent of this entry
        /// </summary>
        public virtual VPDirectoryEntry Parent
        {
            get { return _parent; }

            internal set
            {
                _parent = value;

                OnPropertyChanged();
            }
        }


        /// <summary>
        ///     The name of this entry
        /// </summary>
        public virtual string Name
        {
            get { return _name; }

            set
            {
                if (_name != value)
                {
                    _name = value;

                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Constructs the path of the file within this file. The directory separator is '/'.
        /// </summary>
        public string Path
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                builder.Insert(0, Name == null ? "" : Name);
                builder.Insert(0, "/");

                VPDirectoryEntry parent = Parent;
                while (parent != null)
                {
                    if (parent.Path != "/")
                    {
                        builder.Insert(0, parent.Name);
                        builder.Insert(0, "/");
                    }
                    parent = parent.Parent;
                }

                return builder.ToString();
            }
        }

        #region IComparable<VPEntry> Members

        public int CompareTo(VPEntry other)
        {
            if (other == this)
            {
                return 0;
            }

            if (other == null)
            {
                return 1;
            }

            if (this is VPFileEntry)
            {
                // This is a file
                if (other is VPDirectoryEntry)
                {
                    // The other is a directory
                    return 1;
                }
                else
                {
                    if (Name == null)
                    {
                        if (other.Name == null)
                        {
                            return 0;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    else if (other.Name == null)
                    {
                        return 1;
                    }
                    else
                    {
                        // Both are file, compare the name
                        return this.Name.CompareTo(other.Name);
                    }
                }
            }
            else
            {
                if (other is VPFileEntry)
                {
                    return -1;
                }
                else
                {
                    if (Name == null && other.Name == null)
                    {
                        return 0;
                    }
                    else if (Name == null)
                    {
                        return -1;
                    }
                    else if (other.Name == null)
                    {
                        return 1;
                    }
                    else
                    {
                        // Both are file, compare the name
                        return this.Name.CompareTo(other.Name);
                    }
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        ///     Fired when a property of this entry has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
