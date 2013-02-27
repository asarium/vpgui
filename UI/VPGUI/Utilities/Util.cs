using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using IWin32Window = System.Windows.Forms.IWin32Window;

namespace VPGUI.Utilities
{
    // From http://stackoverflow.com/questions/1256793/mvvm-sync-collections

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
