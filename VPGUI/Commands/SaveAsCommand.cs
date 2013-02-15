using System;
using System.IO;
using System.Threading.Tasks;
using VPGUI.Models;
using VPGUI.Services;
using VPGUI.Utilities;

namespace VPGUI.Commands
{
    public class SaveAsCommand : SaveCommand
    {
        public SaveAsCommand(MainModel mainModel) : base(mainModel)
        {
        }

        public override void Execute(object parameter)
        {
            if (this.FileInstance != null)
            {
                this.ApplicationModel.InteractionService.SaveFileDialog("Save VP-file", path =>
                    {
                        if (path != null)
                        {
                            this.ApplicationModel.IsBusy = true;
                            this.ApplicationModel.BusyMessage = "Saving to " + new FileInfo(path).Name + "...";

                            this.FileInstance.WriteVPAsync(path, this.BackupCallback).ContinueWith((task) =>
                                {
                                    if (task.Exception != null)
                                    {
                                        this.ApplicationModel.InteractionService.ShowMessage(MessageType.Error,
                                                                                             "Error while writing file",
                                                                                             "Error while writing VP file:" +
                                                                                             Util
                                                                                                 .GetAggregateExceptionMessage
                                                                                                 (
                                                                                                     task.Exception));

                                        this.ApplicationModel.StatusMessage = "Failed to save VP-file.";
                                    }
                                    else
                                    {
                                        this.ApplicationModel.InteractionService.ShowMessage(MessageType.Information,
                                                                                             "Writing completed",
                                                                                             "VP-file was successfully written.");

                                        this.ApplicationModel.StatusMessage = String.Format(
                                            "VP-file successfully saved to {0}.", task.Result);
                                    }

                                    this.ApplicationModel.IsBusy = false;
                                }, TaskScheduler.FromCurrentSynchronizationContext());
                        }
                    }, new FileFilter("VP-file", "*.vp"), new FileFilter("All files", "*.*"));
            }
        }

        public override bool CanExecute(object parameter)
        {
            if (this.FileInstance != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
