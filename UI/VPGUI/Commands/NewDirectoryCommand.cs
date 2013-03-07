using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPGUI.Models;

namespace VPGUI.Commands
{
    class NewDirectoryCommand : MainModelCommand
    {
        public NewDirectoryCommand(MainModel applicationModel) : base(applicationModel)
        {
            applicationModel.PropertyChanged += ApplicationModelOnPropertyChanged;
        }

        private void ApplicationModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DirectoryListModel")
            {
                FireCanExecuteChanged();
            }
        }

        public override bool CanExecute(object parameter)
        {
            if (ApplicationModel.CurrentVpFile == null)
            {
                return false;
            }
            else
            {
                return ApplicationModel.TreeViewModel.SelectedItem != null;
            }
        }

        public override void Execute(object parameter)
        {
            var view = ApplicationModel.CreateDirectory();

            if (view != null)
            {
                view.IsSelected = true;
                view.BeginEdit();
            }
        }
    }
}
