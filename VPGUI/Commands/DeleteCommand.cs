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
            var deleted = new LinkedList<VpListEntryViewModel>();

            foreach (var selected in ApplicationModel.DirectoryListModel.SelectedEntries)
            {
                var entry = selected.Entry;

                if (entry.Parent.RemoveChild(entry))
                {
                    deleted.AddLast(selected);
                }
            }

            foreach (var item in deleted)
            {
                item.IsSelected = false;
            }
        }
    }
}
