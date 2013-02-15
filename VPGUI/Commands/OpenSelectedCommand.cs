using System;
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

            foreach (VpListEntryViewModel selected in this.ApplicationModel.DirectoryListModel.SelectedEntries)
            {
                this.ApplicationModel.OpenEntry(selected.Entry, selected);

                if (selected.Entry is VPDirectoryEntry)
                {
                    break;
                }
            }
        }
    }
}
