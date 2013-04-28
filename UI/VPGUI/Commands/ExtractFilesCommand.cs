using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using VPGUI.Models;
using VPGUI.Services;
using VPGUI.Utilities;

namespace VPGUI.Commands
{
    internal class ExtractFilesCommand : MainModelCommand
    {
        private VpDirectoryListModel _currentListModel;

        public ExtractFilesCommand(MainModel applicationModel) : base(applicationModel)
        {
            this.ApplicationModel.PropertyChanged += this.ApplicationModelOnPropertyChanged;

            this.CurrentListModel = this.ApplicationModel.DirectoryListModel;
        }

        private VpDirectoryListModel CurrentListModel
        {
            get { return this._currentListModel; }

            set
            {
                if (this._currentListModel != null)
                {
                    this._currentListModel.SelectedEntries.CollectionChanged -= this.SelectedEntriesOnCollectionChanged;
                }

                this._currentListModel = value;

                if (this._currentListModel != null)
                {
                    this._currentListModel.SelectedEntries.CollectionChanged += this.SelectedEntriesOnCollectionChanged;
                }

                this.FireCanExecuteChanged();
            }
        }

        private void ApplicationModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "DirectoryListModel")
            {
                this.CurrentListModel = this.ApplicationModel.DirectoryListModel;
            }
        }

        private void SelectedEntriesOnCollectionChanged(object sender,
                                                        NotifyCollectionChangedEventArgs
                                                            notifyCollectionChangedEventArgs)
        {
            this.FireCanExecuteChanged();
        }

        public override bool CanExecute(object parameter)
        {
            return this.CurrentListModel != null && this.CurrentListModel.SelectedEntries.Count > 0;
        }

        public override void Execute(object parameter)
        {
            this.ApplicationModel.InteractionService.SaveDirectoryDialog(null, path => ApplicationModel.ExtractEntriesAsync(path));
        }
    }
}
