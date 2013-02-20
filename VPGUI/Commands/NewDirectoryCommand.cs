using System;
using System.Collections.Generic;
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
        }

        public override bool CanExecute(object parameter)
        {
            return ApplicationModel.TreeViewModel.SelectedItem != null;
        }

        public override void Execute(object parameter)
        {
            ApplicationModel.CreateDirectory();
        }
    }
}
