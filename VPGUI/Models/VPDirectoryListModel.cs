﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using VPGUI.Utilities;
using VPSharp.Entries;

namespace VPGUI.Models
{
    public class VpDirectoryListModel : INotifyPropertyChanged
    {
        private readonly Lazy<ObservableCollection<VpEntryView<VPEntry>>> _entryViews;

        private readonly Lazy<ObservableCollection<VpListEntryViewModel>> _selectedEntries;
        private VPTreeEntryViewModel dirEntry;

        public VpDirectoryListModel(VPTreeEntryViewModel entry)
        {
            this.dirEntry = entry;

            Func<VPEntry, VpEntryView<VPEntry>> viewModelCreator = this.GetEntryItem;
            this._entryViews =
                new Lazy<ObservableCollection<VpEntryView<VPEntry>>>(() => new ObservableViewModelCollection
                                                                               <VpEntryView<VPEntry>, VPEntry>(
                                                                               this.dirEntry.Entry.Children,
                                                                               viewModelCreator));

            this._selectedEntries = new Lazy<ObservableCollection<VpListEntryViewModel>>
                (() => new ObservableCollection<VpListEntryViewModel>());
        }

        public ObservableCollection<VpEntryView<VPEntry>> Entries
        {
            get { return this._entryViews.Value; }
        }

        public ObservableCollection<VpListEntryViewModel> SelectedEntries
        {
            get { return this._selectedEntries.Value; }
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

        private VpEntryView<VPEntry> GetEntryItem(VPEntry item)
        {
            if (item is VPFileEntry)
            {
                return new VpListEntryViewModel(this, item);
            }
            else
            {
                return new VpListDirEntryModel(this, this.GetTreeEntryForDirectory, (VPDirectoryEntry) item);
            }
        }

        private VPTreeEntryViewModel GetTreeEntryForDirectory(VPDirectoryEntry searched)
        {
            return this.dirEntry.Children.FirstOrDefault(treeEntry => treeEntry.Entry == searched);
        }
    }

    public class VpListEntryViewModel : VpEntryView<VPEntry>
    {
        public VpListEntryViewModel(VpDirectoryListModel parentModel, VPEntry entry) : base(entry)
        {
            this.ParentViewModel = parentModel;
        }

        protected override void EntryChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            base.EntryChanged(sender, propertyChangedEventArgs);

            var str = propertyChangedEventArgs.PropertyName;
            if (str == "LastModified" || str == "LastModified")
            {
                OnPropertyChanged("LastEdit");
                OnPropertyChanged("LastEditString");
            }
            else if (str == "FileSize" || str == "AggregatedFileSize")
            {
                OnPropertyChanged("FileSize");
                OnPropertyChanged("FileSizeString");
            }
        }

        public VpDirectoryListModel ParentViewModel { get; set; }

        public override bool IsSelected
        {
            get { return base.IsSelected; }
            set
            {
                base.IsSelected = value;

                if (this.IsSelected)
                {
                    this.ParentViewModel.SelectedEntries.Add(this);
                }
                else
                {
                    this.ParentViewModel.SelectedEntries.Remove(this);
                }
            }
        }

        public string FileSizeString
        {
            get
            {
                return ((long) FileSize).HumanReadableByteCount(false);
            }
        }

        public int FileSize
        {
            get
            {
                var entry = this.Entry as VPDirectoryEntry;
                return entry != null ? entry.AggregatedFileSize : ((VPFileEntry) this.Entry).FileSize;
            }
        }

        public string LastEditString
        {
            get
            {
                return LastEdit.ToLocalTime().ToString(CultureInfo.CurrentUICulture);
            }
        }

        public DateTime LastEdit
        {
            get
            {
                var entry = this.Entry as VPDirectoryEntry;
                return entry != null ? entry.LastModified : ((VPFileEntry) this.Entry).LastModified;
            }
        }
    }

    internal class VpListDirEntryModel : VpListEntryViewModel
    {
        #region Delegates

        public delegate VPTreeEntryViewModel CreationDelegate(VPDirectoryEntry entry);

        #endregion

        private Lazy<VPTreeEntryViewModel> _treeModelEntry;

        public VpListDirEntryModel(VpDirectoryListModel parentModel,
                                   CreationDelegate creationFunc, VPDirectoryEntry entry)
            : base(parentModel, entry)
        {
            this._treeModelEntry = new Lazy<VPTreeEntryViewModel>(() => creationFunc((VPDirectoryEntry) this.Entry));
        }

        public VPTreeEntryViewModel TreeModelDirEntry
        {
            get { return this._treeModelEntry.Value; }
        }
    }
}
