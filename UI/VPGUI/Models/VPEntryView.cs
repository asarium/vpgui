using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Etier.IconHelper;
using VPGUI.IconHelper;
using VPGUI.Utilities;
using VPSharp.Entries;

namespace VPGUI.Models
{
    public interface IEntryView<out TEntryType> : INotifyPropertyChanged, IEditableObject where TEntryType : VPEntry
    {
        TEntryType Entry { get; }

        bool IsSelected
        {
            get; set;
        }

        bool IsEditing
        {
            get;
        }

        string Name
        {
            get; set;
        }

        ImageSource EntryIcon { get; }
    }

    public class VpEntryView<TEntryType> : IEntryView<TEntryType> where TEntryType : VPEntry
    {
        private bool _isSelected;

        protected VpEntryView(TEntryType entry)
        {
            this.Entry = entry;

            Entry.PropertyChanged += EntryChanged;
        }

        protected virtual void EntryChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "Name" && !IsEditing)
            {
                OnPropertyChanged("Name");
            }
        }

        public TEntryType Entry
        {
            get;
            private set;
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(obj, this))
            {
                return 0;
            }

            if (obj == null)
            {
                return 1;
            }

            var entryView = obj as IEntryView<TEntryType>;

            if (entryView == null)
            {
                return 1;
            }

            if (entryView.Entry == null)
            {
                return 1;
            }

            if (this.Entry == null)
            {
                return -1;
            }

            return this.Entry.CompareTo(entryView.Entry);
        }

        /// <summary>
        ///     Gets/sets whether the Item
        ///     associated with this object is selected.
        /// </summary>
        public virtual bool IsSelected
        {
            get { return this._isSelected; }
            set
            {
                if (value != this._isSelected)
                {
                    this._isSelected = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private bool _isEditing = false;
        public bool IsEditing
        {
            get
            {
                return this._isEditing;
            }

            private set
            {
                if (this._isEditing != value)
                {
                    this._isEditing = value;

                    OnPropertyChanged();
                }
            }
        }

        private string _editName =null;

        public string Name
        {
            get
            {
                if (IsEditing)
                {
                    return _editName;
                }
                else
                {
                    return this.Entry.Name;
                }
            }

            set
            {
                if (IsEditing)
                {
                    _editName = value;
                }
                else
                {
                    this.Entry.Name = value;
                }
            }
        }

        public virtual ImageSource EntryIcon
        {
            get
            {
                if (this.Entry is VPDirectoryEntry)
                {
                    return IconReader.GetFolderIcon(IconReader.IconSize.Large,
                                                    IconReader.FolderType.Closed, "Something").ToImageSource();
                }
                else
                {
                    return
                        IconReader.GetFileIcon(Util.GetExtensionString(this.Name), IconReader.IconSize.Large, false)
                                  .ToImageSource();
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public virtual void BeginEdit()
        {
            _editName = Name;
            IsEditing = true;
        }

        public virtual void EndEdit()
        {
            if (!IsEditing)
            {
                return;
            }

            IsEditing = false;

            Name = _editName;
        }

        public virtual void CancelEdit()
        {
            if (!IsEditing)
            {
                return;
            }

            _editName = null;
            IsEditing = false;
        }
    }
}
