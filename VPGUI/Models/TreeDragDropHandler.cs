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
    public class TreeDragDropHandler : IDragSource, IDropTarget
    {
        private readonly MainModel ApplicationModel;

        public TreeDragDropHandler(MainModel applicationModel)
        {
            this.ApplicationModel = applicationModel;
        }

        public void StartDrag(IDragInfo dragInfo)
        {
        }

        public void Dropped(IDropInfo dropInfo)
        {
        }

        private DateTime _dragOverBegin = DateTime.MinValue;
        private object _dragOverItem = null;

        public void DragOver(IDropInfo dropInfo)
        {
            if (ApplicationModel.CurrentVpFile == null)
            {
                dropInfo.Effects = DragDropEffects.None;
            }

            var targetItem = dropInfo.TargetItem as VPTreeEntryViewModel;
            if (targetItem != null)
            {
                targetItem.IsSelected = true;
            }


            if (_dragOverItem != dropInfo.TargetItem)
            {
                _dragOverItem = dropInfo.TargetItem;

                this._dragOverBegin = dropInfo.TargetItem != null ? DateTime.UtcNow : DateTime.MinValue;
            }
            else if (_dragOverItem != null && (DateTime.UtcNow - this._dragOverBegin).TotalMilliseconds > 1000)
            {
                if (targetItem != null)
                {
                    targetItem.IsExpanded = true;
                }
            }

            var obj = dropInfo.Data as IDataObject;
            if (obj != null)
            {
                if (obj.GetDataPresent(DataFormats.FileDrop))
                {
                    dropInfo.Effects = DragDropEffects.Copy;

                    if (dropInfo.TargetItem != null)
                    {
                        dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                    }
                    else
                    {
                        dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                    }
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
            var targetItem = (VPTreeEntryViewModel) dropInfo.TargetItem;

            var obj = dropInfo.Data as IDataObject;
            if (obj != null)
            {
                var paths = (string[]) obj.GetData(DataFormats.FileDrop);

                if (targetItem == null)
                {
                    VPDirectoryEntry parent = null;
                    var enumerator = dropInfo.TargetCollection.GetEnumerator();

                    if (enumerator.MoveNext())
                    {
                        parent = ((VPTreeEntryViewModel) enumerator.Current).Entry.Parent;
                    }

                    if (parent == null)
                    {
                        parent = ApplicationModel.CurrentVpFile.RootNode;
                    }

                    ApplicationModel.AddFilePaths(paths, parent);
                }
            }
        }
    }
}
