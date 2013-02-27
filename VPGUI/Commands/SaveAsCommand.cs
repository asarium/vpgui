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
                            ApplicationModel.SaveVpFile(path, BackupCallback);
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
