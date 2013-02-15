using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPGUI.Models;

namespace VPGUI.Commands
{
    class DeleteCommand : ExtractFilesCommand
    {
        public DeleteCommand(MainModel applicationModel) : base(applicationModel)
        {
        }

        public override void Execute(object parameter)
        {
            foreach (var selected in ApplicationModel.DirectoryListModel.SelectedEntries)
            {
                var entry = selected.Entry;

                entry.Parent.RemoveChild(entry);
            }
        }
    }
}
