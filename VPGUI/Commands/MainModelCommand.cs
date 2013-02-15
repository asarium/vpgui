using System;
using System.Windows.Input;
using VPGUI.Models;

namespace VPGUI.Commands
{
    public abstract class MainModelCommand : ICommand
    {
        protected MainModelCommand(MainModel applicationModel)
        {
            this.ApplicationModel = applicationModel;
        }

        protected MainModel ApplicationModel { get; set; }

        #region ICommand Members

        public abstract bool CanExecute(object parameter);

        public abstract void Execute(object parameter);

        public event EventHandler CanExecuteChanged;

        #endregion

        protected void FireCanExecuteChanged()
        {
            if (this.CanExecuteChanged != null)
            {
                this.CanExecuteChanged(this, EventArgs.Empty);
            }
        }
    }
}
