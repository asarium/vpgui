using System;
using System.Linq;
using VPGUI.Models;
using VPSharp.Entries;

namespace VPGUI.Commands
{
    internal class OpenSelectedCommand : ExtractFilesCommand
    {
        public OpenSelectedCommand(MainModel applicationModel) : base(applicationModel)
        {
        }

        public override void Execute(object parameter)
        {
            if (this.ApplicationModel.DirectoryListModel == null)
            {
                return;
            }

            if (ApplicationModel.DirectoryListModel.SelectedEntries.Any(item => item.IsEditing))
            {
                return;
            }

            var openedDir = false;
            foreach (var selected in this.ApplicationModel.DirectoryListModel.SelectedEntries)
            {
                // Only open one one directory
                if (selected.Entry is VPDirectoryEntry && openedDir)
                {
                    continue;
                }

                this.ApplicationModel.OpenEntry(selected.Entry, selected);

                if (selected.Entry is VPDirectoryEntry)
                {
                    openedDir = true;
                }
            }
        }
    }
}
