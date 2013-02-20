using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPGUI.Models;

namespace VPGUI.Commands
{
    class RenameCommand : ExtractFilesCommand
    {
        public RenameCommand(MainModel applicationModel) : base(applicationModel)
        {
        }

        public override void Execute(object parameter)
        {
            // Deselect all items except the first one
            ICollection<VpListEntryViewModel> selected = ApplicationModel.DirectoryListModel.SelectedEntries;
            while (selected.Count > 1)
            {
                selected.ElementAt(1).IsSelected = false;
            }

            ApplicationModel.DirectoryListModel.SelectedEntries.ElementAt(0).IsEditing = true;
        }
    }
}
