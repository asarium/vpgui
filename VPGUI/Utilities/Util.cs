using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using IWin32Window = System.Windows.Forms.IWin32Window;

namespace VPGUI.Utilities
{
    // From http://stackoverflow.com/questions/1256793/mvvm-sync-collections
    public sealed class ObservableViewModelCollection<TViewModel, TModel> : ObservableCollection<TViewModel>
    {
        private readonly ObservableCollection<TModel> _source;
        private readonly Func<TModel, TViewModel> _viewModelFactory;

        public ObservableViewModelCollection(ObservableCollection<TModel> source,
                                             Func<TModel, TViewModel> viewModelFactory)
            : base(source.Select(viewModelFactory))
        {
            Contract.Requires(source != null);
            Contract.Requires(viewModelFactory != null);

            this._source = source;
            this._viewModelFactory = viewModelFactory;
            this._source.CollectionChanged += this.OnSourceCollectionChanged;
        }

        private TViewModel CreateViewModel(TModel model)
        {
            return this._viewModelFactory(model);
        }

        private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        this.Add(this.CreateViewModel((TModel) e.NewItems[i]));
                    }
                    break;

                default:
                    break;
            }
        }
    }

    public static class Util
    {
        public static string GetAggregateExceptionMessage(AggregateException except)
        {
            StringBuilder builder = new StringBuilder();

            foreach (Exception e in except.InnerExceptions)
            {
                builder.Append("\n");
                builder.Append(e.Message);
            }

            return builder.ToString();
        }

        public static string GetExtensionString(string name)
        {
            int index = name.LastIndexOf('.');
            if (index < 0)
            {
                return name;
            }
            else
            {
                return name.Substring(name.LastIndexOf('.'));
            }
        }

        public static String HumanReadableByteCount(this long bytes, bool si)
        {
            int unit = si ? 1000 : 1024;
            if (bytes < unit)
            {
                return bytes + " B";
            }

            int exp = (int) (Math.Log(bytes)/Math.Log(unit));
            string pre = (si ? "kMGTPE" : "KMGTPE")[exp - 1] + (si ? "" : "i");

            return String.Format("{0:0.0} {1}B", bytes/Math.Pow(unit, exp), pre);
        }
    }

    public class RelayCommand : ICommand
    {
        #region Fields readonly

        private readonly Predicate<object> _canExecute;
        private Action<object> _execute;

        #endregion

        #region Constructors

        public RelayCommand(Action<object> execute) : this(execute, null)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }
            this._execute = execute;
            this._canExecute = canExecute;
        }

        #endregion // Constructors

        #region

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return this._canExecute == null || this._canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            this._execute(parameter);
        }

        #endregion

        // Fields 

        // ICommand Members 
    }

    /// <summary>
    ///     Copied from http://stackoverflow.com/questions/315164/how-to-use-a-folderbrowserdialog-from-a-wpf-application
    /// </summary>
    public static class MyWpfExtensions
    {
        public static IWin32Window GetIWin32Window(this Visual visual)
        {
            HwndSource source = PresentationSource.FromVisual(visual) as HwndSource;
            IWin32Window win = new OldWindow(source.Handle);
            return win;
        }

        #region Nested type: OldWindow

        private class OldWindow : IWin32Window
        {
            private readonly IntPtr _handle;

            public OldWindow(IntPtr handle)
            {
                this._handle = handle;
            }

            #region IWin32Window Members

            IntPtr IWin32Window.Handle
            {
                get { return this._handle; }
            }

            #endregion
        }

        #endregion
    }
}
