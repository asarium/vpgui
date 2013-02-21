using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using VPSharp.Entries;

namespace VPGUI.Models
{
    public class ListDragDropHandler : IDropTarget
    {
        private readonly MainModel ApplicationModel;

        public ListDragDropHandler(MainModel applicationModel)
        {
            this.ApplicationModel = applicationModel;
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (ApplicationModel.CurrentVpFile == null)
            {
                dropInfo.Effects = DragDropEffects.None;
            }

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
                var source = dropInfo.DragInfo.SourceItems;
                var castItems = from object entry in source select entry as VpEntryView<VPEntry>;

                var vpEntryViews = castItems as IList<VpEntryView<VPEntry>> ?? castItems.ToList();
                if (vpEntryViews.Any(entryView => entryView == null))
                {
                    dropInfo.DragInfo.Effects = DragDropEffects.None;
                    return;
                }

                dropInfo.Effects = dropInfo.KeyStates.HasFlag(DragDropKeyStates.ControlKey)
                                       ? DragDropEffects.Copy : DragDropEffects.Move;

                var target = ApplicationModel.TreeViewModel.SelectedItem.Entry;

                if (vpEntryViews.Any(entry => entry.Entry == target))
                {
                    dropInfo.DragInfo.Effects = DragDropEffects.None;
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
            else
            {
                var copy = dropInfo.KeyStates.HasFlag(DragDropKeyStates.ControlKey);
                var source = dropInfo.DragInfo.SourceItems;
                var castItems = from object entry in source select (entry as VpEntryView<VPEntry>).Entry;

                var target = ApplicationModel.TreeViewModel.SelectedItem.Entry;
                foreach (var entry in castItems)
                {
                    target.AddChild(entry, !copy);
                }
            }
        }
    }
}
