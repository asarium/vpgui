using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        private readonly Dictionary<string, object> _originalValues = new Dictionary<string, object>();
        private readonly Dictionary<string, Predicate<object>> _changedCheckers = new Dictionary<string, Predicate<object>>();
        private readonly Dictionary<string, bool> _changedStatus = new Dictionary<string, bool>();


        internal VPEntry(VPFile containing)
        {
            this.ContainingFile = containing;

            ChangedOverride = true;

            this.InitChangedCheckers();
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

            ChangedOverride = true;

            this.InitChangedCheckers();
        }

        /// <summary>
        ///     The VPFile which contains this entry.
        /// </summary>
        internal VPFile ContainingFile { get; set; }


        internal bool ChangedOverride
        {
            get;
            set;
        }

        /// <summary>
        ///     Specifies if this entry has changed.
        /// </summary>
        public virtual bool Changed
        {
            get 
            { 
                if (_dirEntry == null && ChangedOverride)
                {
                    return true;
                }
                else
                {
                    return _changed;
                }
            }

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
                if (_parent != value)
                {
                    _parent = value;

                    OnPropertyChanged();
                }
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
                var builder = new StringBuilder();
                builder.Insert(0, this.Name ?? "");
                builder.Insert(0, "/");

                var parent = Parent;
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

        protected void AddChangedChecker(string property, Predicate<object> checker)
        {
            _changedCheckers[property] = checker;
        }

        private void InitChangedCheckers()
        {
            PropertyChanged += (sender, args) =>
                {
                    if (!ChangedOverride && !ContainingFile.BuildingIndex && args.PropertyName != "Changed")
                    {
                        var origVal = !_originalValues.ContainsKey(args.PropertyName) ? null : _originalValues[args.PropertyName];
                        var nowVal = this.GetType().GetProperty(args.PropertyName).GetValue(this);

                        if (_changedCheckers.ContainsKey(args.PropertyName))
                        {
                            _changedStatus[args.PropertyName] = _changedCheckers[args.PropertyName](origVal);
                        }
                        else
                        {
                            _changedStatus[args.PropertyName] = !Equals(nowVal, origVal);
                        }

                        Changed = _changedStatus.Any(item => item.Value);
                    }
                };
            PropertyChanged += (sender, args) =>
                {
                    if (ChangedOverride && (ContainingFile.BuildingIndex || !_originalValues.ContainsKey(args.PropertyName)) && args.PropertyName != "Changed")
                    {
                        _originalValues[args.PropertyName] = this.GetType().GetProperty(args.PropertyName).GetValue(this);
                    }
                };
        }
    }
}
