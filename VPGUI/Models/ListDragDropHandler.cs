using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GongSolutions.Wpf.DragDrop;
using VPSharp.Entries;

namespace VPGUI.Models
{
    public class ListDragDropHandler : IDropTarget, IDragSource
    {
        private readonly MainModel ApplicationModel;

        public ListDragDropHandler(MainModel applicationModel)
        {
            this.ApplicationModel = applicationModel;
        }

        public void StartDrag(IDragInfo dragInfo)
        {
        }

        public void Dropped(IDropInfo dropInfo)
        {
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (ApplicationModel.CurrentVpFile == null)
            {
                dropInfo.Effects = DragDropEffects.None;
            }

            var targetItem = dropInfo.TargetItem as VPTreeEntryViewModel;

            var obj = dropInfo.Data as IDataObject;
            if (obj != null)
            {
                if (obj.GetDataPresent(DataFormats.FileDrop))
                {
                    dropInfo.Effects = DragDropEffects.Copy;
                }
                else
                {
                    dropInfo.Effects = DragDropEffects.None;
                }
            }
            else
            {
                var entryView = dropInfo.Data as VpEntryView<VPEntry>;

                if (entryView != null)
                {
                    if (targetItem != null && entryView.Entry == targetItem.Entry)
                    {
                        dropInfo.Effects = DragDropEffects.None;
                    }
                    else if (dropInfo.KeyStates == DragDropKeyStates.ControlKey)
                    {
                        dropInfo.Effects = DragDropEffects.Copy;
                    }
                    else
                    {
                        dropInfo.Effects = DragDropEffects.Copy;
                    }
                }
                else
                {
                    dropInfo.Effects = DragDropEffects.None;
                }
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            var obj = dropInfo.Data as IDataObject;
            if (obj != null)
            {
                ApplicationModel.AddFilePaths((string[]) obj.GetData(DataFormats.FileDrop));
            }
        }
    }
}
