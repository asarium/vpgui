using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VPGUI.Models
{
    public class SelectedHistory : INotifyPropertyChanged
    {
        private int _currentIndex;

        public SelectedHistory()
        {
            this.SelectedEntries = new ObservableCollection<VpTreeEntryViewModel>();
            this.CurrentIndex = 0;
        }

        public ObservableCollection<VpTreeEntryViewModel> SelectedEntries { get; internal set; }

        public int CurrentIndex
        {
            get { return this._currentIndex; }

            set
            {
                if (this._currentIndex != value)
                {
                    this._currentIndex = Math.Max(0, Math.Min(value, this.SelectedEntries.Count - 1));

                    this.OnPropertyChanged();

                    this.OnPropertyChanged("CurrentEntry");
                }
            }
        }

        public VpTreeEntryViewModel CurrentEntry
        {
            get
            {
                if (this.CurrentIndex < this.SelectedEntries.Count && this.CurrentIndex >= 0)
                {
                    return this.SelectedEntries[this.CurrentIndex];
                }
                else
                {
                    return null;
                }
            }
        }

        public bool SuppressNewEntries { private get; set; }

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

        public void AddHistoryEntry(VpTreeEntryViewModel viewModel)
        {
            if (!this.SuppressNewEntries)
            {
                while (this.SelectedEntries.Count > this.CurrentIndex + 1)
                {
                    this.SelectedEntries.RemoveAt(this.SelectedEntries.Count - 1);
                }

                this.SelectedEntries.Add(viewModel);

                this.CurrentIndex = this.SelectedEntries.Count - 1;
            }
        }
    }
}
