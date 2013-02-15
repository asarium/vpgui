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
            ApplicationModel.InteractionService.OpenFileDialog("Add files to VP", true, paths =>
                {
                    ApplicationModel.IsBusy = true;
                    ApplicationModel.BusyMessage = "Adding files...";

                    Task.Run(async () =>
                        {
                            foreach (var path in paths)
                            {
                                ApplicationModel.BusyMessage = "Adding " + new FileInfo(path).Name + "...";

                                await ApplicationModel.AddFilePath(path);
                            }
                        }).ContinueWith(task => ApplicationModel.IsBusy = false);
                }, new FileFilter("All files", "*.*"));
        }
    }
}
