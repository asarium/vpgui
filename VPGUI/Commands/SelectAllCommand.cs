using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPGUI.Models;

namespace VPGUI.Commands
{
    class SelectAllCommand : MainModelCommand
    {
        public SelectAllCommand(MainModel applicationModel) : base(applicationModel)
        {
        }

        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            if (ApplicationModel.DirectoryListModel == null)
            {
                return;
            }

            foreach (var item in ApplicationModel.DirectoryListModel.Entries)
            {
                item.IsSelected = true;
            }
        }
    }
}
