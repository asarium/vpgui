using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;
using VPGUI.Models;

namespace VPGUI.Commands
{
    public abstract class MainModelCommand : ICommand
    {
        protected MainModelCommand(MainModel applicationModel)
        {
            this.ApplicationModel = applicationModel;

            this.Dispatcher = Dispatcher.CurrentDispatcher;
        }

        private Dispatcher Dispatcher
        {
            get; set;
        }

        protected MainModel ApplicationModel { get; private set; }

        #region ICommand Members

        public abstract bool CanExecute(object parameter);

        public abstract void Execute(object parameter);

        public event EventHandler CanExecuteChanged;

        #endregion

        protected void FireCanExecuteChanged()
        {
            if (this.CanExecuteChanged != null)
            {
                if (Dispatcher.CheckAccess())
                {
                    CanExecuteChanged(this, EventArgs.Empty);
                }
                else
                {
                    Dispatcher.InvokeAsync(() => CanExecuteChanged(this, EventArgs.Empty));
                }
            }
        }
    }
}
