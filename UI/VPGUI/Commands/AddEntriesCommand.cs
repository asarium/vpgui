using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPGUI.Models;
using VPGUI.Services;

namespace VPGUI.Commands
{
    class AddEntriesCommand : MainModelCommand
    {
        public AddEntriesCommand(MainModel applicationModel) : base(applicationModel)
        {
        }

        public override bool CanExecute(object parameter)
        {
            return ApplicationModel.DirectoryListModel != null;
        }

        public override void Execute(object parameter)
        {
            ApplicationModel.InteractionService.OpenFileDialog("Add files to VP", true, 
                paths => this.ApplicationModel.AddFilePaths(paths), new FileFilter("All files", "*.*"));
        }
    }
}
