using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GongSolutions.Wpf.DragDrop;
using VPSharp.Entries;

namespace VPGUI.Models
{
    public class TreeDragDropHandler : AbstractDropHandler
    {
        private const int ExpandTime = 500;
        
        public TreeDragDropHandler(MainModel applicationModel) : base(applicationModel)
        {
        }

        private DateTime _dragOverBegin = DateTime.MinValue;
        private object _dragOverItem = null;

        protected override VPDirectoryEntry GetTargetItem(IDropInfo dropInfo)
        {
            var targetItem = dropInfo.TargetItem as VpTreeEntryViewModel;
            VPDirectoryEntry parent = null;

            if (targetItem == null)
            {
                var enumerator = dropInfo.TargetCollection.GetEnumerator();

                if (enumerator.MoveNext())
                {
                    parent = ((VpTreeEntryViewModel) enumerator.Current).Entry.Parent;
                }

                if (parent == null)
                {
                    parent = this.ApplicationModel.CurrentVpFile.RootNode;
                }
            }
            else
            {
                parent = targetItem.Entry;
            }

            return parent;
        }

        public override void DragOver(IDropInfo dropInfo)
        {
            if (ApplicationModel.CurrentVpFile == null)
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            var targetItem = dropInfo.TargetItem as VpTreeEntryViewModel;
            if (targetItem != null)
            {
                targetItem.IsSelected = true;
            }


            if (_dragOverItem != dropInfo.TargetItem)
            {
                _dragOverItem = dropInfo.TargetItem;

                this._dragOverBegin = dropInfo.TargetItem != null ? DateTime.UtcNow : DateTime.MinValue;
            }
            else if (_dragOverItem != null && (DateTime.UtcNow - this._dragOverBegin).TotalMilliseconds > ExpandTime)
            {
                if (targetItem != null)
                {
                    targetItem.IsExpanded = true;
                }
            }

            base.DragOver(dropInfo);

            dropInfo.DropTargetAdorner = dropInfo.TargetItem != null ? DropTargetAdorners.Highlight : DropTargetAdorners.Insert;
        }
    }
}
