using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPGUI.Models;
using VPGUI.Services;

namespace VPGUI.Commands
{
    class CloseFileCommand : MainModelCommand
    {
        public CloseFileCommand(MainModel applicationModel) : base(applicationModel)
        {
        }

        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            if (ApplicationModel.CurrentVpFile == null)
            {
                return;
            }

            if (ApplicationModel.CurrentVpFile.RootNode.Changed)
            {
                ApplicationModel.InteractionService.ShowQuestion(MessageType.Question, QuestionType.YesNo, "Unsaved changes",
                    "There are unsaved changes. Do you really want to close the file?", answer =>
                        {
                            if (answer == QuestionAnswer.Yes)
                            {
                                ApplicationModel.CurrentVpFile = null;
                            }
                        });
            }
            else
            {
                ApplicationModel.CurrentVpFile = null;
            }
        }
    }
}
