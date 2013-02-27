using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using VPGUI.Utilities;
using VPSharp.Entries;

namespace VPGUI.Models
{
    public class VpDirectoryListModel : INotifyPropertyChanged
    {
        private readonly Lazy<ListCollectionView> _entryViewSource; 

        private readonly Lazy<ObservableCollection<VpListEntryViewModel>> _selectedEntries;
        private VpTreeEntryViewModel dirEntry;

        public VpDirectoryListModel(VpTreeEntryViewModel entry)
        {
            this.dirEntry = entry;

            this.Entries = new ObservableViewModelCollection
                <IEntryView<VPEntry>, VPEntry>(this.dirEntry.Entry.Children, GetEntryItem);

            this._selectedEntries = new Lazy<ObservableCollection<VpListEntryViewModel>>
                (() => new ObservableCollection<VpListEntryViewModel>());

            _entryViewSource = new Lazy<ListCollectionView>(() =>
                {
                    ListCollectionView listCollection = null;

                    // The CollectionViewSource has to be created on the UI thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var source = CollectionViewSource.GetDefaultView(Entries);

                        source.Filter = item =>
                        {
                            var entryView = item as IEntryView<VPEntry>;
                            return entryView != null && entryView.Name.
                                ToLowerInvariant().Contains(this.SearchText.ToLowerInvariant());
                        };

                        listCollection = source as ListCollectionView;

                        if (listCollection != null)
                        {
                            listCollection.CustomSort = new EntrySorter();
                            listCollection.IsLiveSorting = true;

                            listCollection.IsLiveFiltering = true;

                            PropertyChanged += (sender, args) =>
                            {
                                if (args.PropertyName == "SearchText")
                                {
                                    listCollection.Refresh();
                                }
                            };
                        }
                    });

                    return listCollection;
                });
        }

        public ObservableCollection<IEntryView<VPEntry>> Entries
        { get; private set; }

        public ListCollectionView EntriesView
        {
            get { return _entryViewSource.Value; }
        }

        private string _searchText = "";
        public string SearchText {
            get { return _searchText; }

            set 
            { 
                _searchText = value;
            
                OnPropertyChanged();
            }
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

        private IEntryView<VPEntry> GetEntryItem(VPEntry item)
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

        private VpTreeEntryViewModel GetTreeEntryForDirectory(VPDirectoryEntry searched)
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

        public override void BeginEdit()
        {
            // Avoid infinite recursion
            if (IsEditing)
            {
                return;
            }

            base.BeginEdit();

            ParentViewModel.EntriesView.EditItem(this);
        }

        public override void EndEdit()
        {
            if (!IsEditing)
            {
                return;
            }

            base.EndEdit();

            ParentViewModel.EntriesView.CommitEdit();
        }

        public override void CancelEdit()
        {
            if (!IsEditing)
            {
                return;
            }

            base.CancelEdit();

            ParentViewModel.EntriesView.CancelEdit();
        }
    }

    internal class VpListDirEntryModel : VpListEntryViewModel
    {
        #region Delegates

        public delegate VpTreeEntryViewModel CreationDelegate(VPDirectoryEntry entry);

        #endregion

        private Lazy<VpTreeEntryViewModel> _treeModelEntry;

        public VpListDirEntryModel(VpDirectoryListModel parentModel,
                                   CreationDelegate creationFunc, VPDirectoryEntry entry)
            : base(parentModel, entry)
        {
            this._treeModelEntry = new Lazy<VpTreeEntryViewModel>(() => creationFunc((VPDirectoryEntry) this.Entry));
        }

        public VpTreeEntryViewModel TreeModelDirEntry
        {
            get { return this._treeModelEntry.Value; }
        }
    }
}
