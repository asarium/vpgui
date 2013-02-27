using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GongSolutions.Wpf.DragDrop;
using VPSharp.Entries;

namespace VPGUI.Models
{
    public abstract class AbstractDropHandler : IDropTarget
    {
        protected readonly MainModel ApplicationModel;

        protected AbstractDropHandler(MainModel applicationModel)
        {
            this.ApplicationModel = applicationModel;
        }

        public virtual void DragOver(IDropInfo dropInfo)
        {
            if (ApplicationModel.CurrentVpFile == null)
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }
            
            var target = GetTargetItem(dropInfo);
            var obj = dropInfo.Data as IDataObject;
            if (obj != null)
            {
                if (obj.GetDataPresent(DataFormats.FileDrop))
                {
                    var paths = (string[]) obj.GetData(DataFormats.FileDrop);
                    if (target.Children.Any(item => paths.Any(path => path == item.Name)))
                    {
                        dropInfo.Effects = DragDropEffects.None;
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
            else
            {
                var entries = GetDragItems(dropInfo);

                if (entries == null || entries.Any(view => view == null))
                {
                    dropInfo.DragInfo.Effects = DragDropEffects.None;
                }
                else
                {
                    if (entries.Any(entry => entry.Entry.Name == target.Name))
                    {
                        dropInfo.DragInfo.Effects = DragDropEffects.None;
                    }
                    else if (entries.Any(item => item.Entry is VPDirectoryEntry && ((VPDirectoryEntry) item.Entry).HasSubdirectory(target)))
                    {
                        dropInfo.DragInfo.Effects = DragDropEffects.None;
                    }
                    else if (entries.Any(item => target.Children.Any(child => child.Name == item.Entry.Name)))
                    {
                        dropInfo.DragInfo.Effects = DragDropEffects.None;
                    }
                    else
                    {
                        dropInfo.Effects = dropInfo.KeyStates.HasFlag(DragDropKeyStates.ControlKey)
                                                ? DragDropEffects.Copy : DragDropEffects.Move;
                    }
                }
            }
        }

        protected abstract VPDirectoryEntry GetTargetItem(IDropInfo dropInfo);

        public virtual void Drop(IDropInfo dropInfo)
        {
            var parent = GetTargetItem(dropInfo);

            var obj = dropInfo.Data as IDataObject;

            if (obj != null)
            {
                ApplicationModel.AddFilePaths((string[]) obj.GetData(DataFormats.FileDrop), parent);
            }
            else
            {
                var copy = dropInfo.KeyStates.HasFlag(DragDropKeyStates.ControlKey);
                var source = GetDragItems(dropInfo);
                var castItems = from object entry in source
                                select ((IEntryView<VPEntry>) entry).Entry;

                foreach (var entry in castItems)
                {
                    parent.AddChild(entry, !copy);
                }
            }
        }

        protected static IEntryView<VPEntry>[] GetDragItems(IDropInfo dropInfo)
        {
            var enumerable = dropInfo.Data as IEnumerable;
            IEntryView<VPEntry>[] entries = null;

            if (enumerable != null)
            {
                entries = enumerable.Cast<IEntryView<VPEntry>>().ToArray();
            }
            else
            {
                var view = dropInfo.Data as IEntryView<VPEntry>;
                if (view != null)
                {
                    entries = new[] { view };
                }
            }

            return entries;
        }
    }
}
