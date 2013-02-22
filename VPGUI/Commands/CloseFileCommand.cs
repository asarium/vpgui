using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPGUI.Models;

namespace VPGUI.Commands
{
    class CloseFileCommand : MainModelCommand
    {
        public CloseFileCommand(MainModel applicationModel) : base(applicationModel)
        {
        }

        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            ApplicationModel.CurrentVpFile = null;
        }
    }
}
