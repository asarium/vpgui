using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Etier.IconHelper;
using VPGUI.IconHelper;
using VPGUI.Utilities;
using VPSharp.Entries;

namespace VPGUI.Models
{
    public interface IEntryView<out TEntryType> :INotifyPropertyChanged where TEntryType : VPEntry
    {
        TEntryType Entry { get; }

        bool IsSelected
        {
            get; set;
        }

        bool IsEditing
        {
            get; set;
        }
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
            if (propertyChangedEventArgs.PropertyName == "Name")
            {
                OnPropertyChanged("Name");
            }
        }

        public TEntryType Entry { get; private set; }

        /// <summary>
        ///     Gets/sets whether the TreeViewItem
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

            set
            {
                if (this._isEditing != value)
                {
                    this._isEditing = value;

                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get { return this.Entry.Name; }

            set { this.Entry.Name = value; }
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
    }
}
