using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Threading;

namespace VPGUI.Utilities
{
    public sealed class ObservableViewModelCollection<TViewModel, TModel> : ObservableCollection<TViewModel>
    {
        private readonly ObservableCollection<TModel> _source;
        private readonly Func<TModel, TViewModel> _viewModelFactory;

        private Dispatcher EventDispatcher
        {
            get; set;
        }

        public ObservableViewModelCollection(ObservableCollection<TModel> source,
                                             Func<TModel, TViewModel> viewModelFactory)
            : base(source.Select(viewModelFactory))
        {
            Contract.Requires(source != null);
            Contract.Requires(viewModelFactory != null);

            this._source = source;
            this._viewModelFactory = viewModelFactory;
            this._source.CollectionChanged += this.OnSourceCollectionChanged;

            EventDispatcher = Dispatcher.CurrentDispatcher;
        }

        private TViewModel CreateViewModel(TModel model)
        {
            return this._viewModelFactory(model);
        }

        private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Do this in the UI thread or else an exception is thrown.
            EventDispatcher.InvokeAsync(() =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            this.Insert(e.NewStartingIndex + i, this.CreateViewModel((TModel) e.NewItems[i]));
                        }
                        break;

                    case NotifyCollectionChangedAction.Move:
                        if (e.OldItems.Count == 1)
                        {
                            this.Move(e.OldStartingIndex, e.NewStartingIndex);
                        }
                        else
                        {
                            var items = this.Skip(e.OldStartingIndex).Take(e.OldItems.Count).ToList();
                            for (int i = 0; i < e.OldItems.Count; i++)
                            {
                                this.RemoveAt(e.OldStartingIndex);
                            }

                            for (int i = 0; i < items.Count; i++)
                            {
                                this.Insert(e.NewStartingIndex + i, items[i]);
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            this.RemoveAt(e.OldStartingIndex);
                        }
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        // remove
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            this.RemoveAt(e.OldStartingIndex);
                        }

                        // add
                        goto case NotifyCollectionChangedAction.Add;

                    case NotifyCollectionChangedAction.Reset:
                        this.Clear();
                        foreach (var t in e.NewItems)
                        {
                            this.Add(this.CreateViewModel((TModel) t));
                        }
                        break;

                    default:
                        break;
                }
            }, DispatcherPriority.Background);
        }
    }
}