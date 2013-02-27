using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPGUI.Models;

namespace VPGUI.Commands
{
    class InvertSelectionCommand : MainModelCommand
    {
        public InvertSelectionCommand(MainModel applicationModel) : base(applicationModel)
        {
        }

        public override bool CanExecute(object parameter)
        {
            return ApplicationModel.DirectoryListModel != null;
        }

        public override void Execute(object parameter)
        {
            foreach (var item in ApplicationModel.DirectoryListModel.Entries)
            {
                item.IsSelected = !item.IsSelected;
            }
        }
    }
}
