using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Etier.IconHelper;
using VPGUI.IconHelper;
using VPGUI.Utilities;
using VPSharp.Entries;

namespace VPGUI.Models
{
    public class VpTreeViewModel : INotifyPropertyChanged
    {
        private VPTreeEntryViewModel _rootEntry;

        private VPTreeEntryViewModel _selectedItem;

        public VpTreeViewModel(VPDirectoryEntry root)
        {
            this._rootEntry = new VPTreeEntryViewModel(this, root);

            if (this._rootEntry.Children.Count > 0)
            {
                this._rootEntry.Children[0].IsSelected = true;
            }
        }

        public ObservableCollection<VPTreeEntryViewModel> TopLevel
        {
            get { return this._rootEntry.Children; }
        }

        public VPTreeEntryViewModel SelectedItem
        {
            get { return this._selectedItem; }

            internal set
            {
                if (this._selectedItem != value)
                {
                    this._selectedItem = value;

                    this.OnPropertyChanged();
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

    public class VPTreeEntryViewModel : VpEntryView<VPDirectoryEntry>
    {
        private readonly Lazy<ObservableCollection<VPTreeEntryViewModel>> _treeEntryViews;
        private bool _isExpanded;

        public VPTreeEntryViewModel(VpTreeViewModel viewModel, VPDirectoryEntry entry,
                                    VPTreeEntryViewModel modelParent = null)
            : base(entry)
        {
            this.ViewModel = viewModel;

            this.ModelParent = modelParent;

            Func<VPEntry, VPTreeEntryViewModel> viewModelCreator =
                model => new VPTreeEntryViewModel(this.ViewModel, (VPDirectoryEntry) model, this);
            this._treeEntryViews = new Lazy<ObservableCollection<VPTreeEntryViewModel>>(
                () => new ObservableViewModelCollection<VPTreeEntryViewModel, VPDirectoryEntry>
                          (entry.SubDirectories, viewModelCreator));
        }

        public VpTreeViewModel ViewModel { get; internal set; }

        public VPTreeEntryViewModel ModelParent { get; internal set; }

        public ObservableCollection<VPTreeEntryViewModel> Children
        {
            get { return this._treeEntryViews.Value; }
        }

        public override bool IsSelected
        {
            get { return base.IsSelected; }
            set
            {
                base.IsSelected = value;

                if (this.IsSelected)
                {
                    this.ViewModel.SelectedItem = this;
                }
            }
        }

        /// <summary>
        ///     Gets/sets whether the TreeViewItem
        ///     associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return this._isExpanded; }
            set
            {
                if (value != this._isExpanded)
                {
                    this._isExpanded = value;
                    this.OnPropertyChanged();

                    this.OnPropertyChanged("EntryIcon");
                }

                // Expand all the way up to the root.
                if (this._isExpanded && this.ModelParent != null)
                {
                    this.ModelParent.IsExpanded = true;
                }
            }
        }

        public override ImageSource EntryIcon
        {
            get
            {
                if (this.IsExpanded)
                {
                    return IconReader.GetFolderIcon(IconReader.IconSize.Large,
                                                    IconReader.FolderType.Open, "Something").ToImageSource();
                }
                else
                {
                    return IconReader.GetFolderIcon(IconReader.IconSize.Large,
                                                    IconReader.FolderType.Closed, "Something").ToImageSource();
                }
            }
        }
    }
}
