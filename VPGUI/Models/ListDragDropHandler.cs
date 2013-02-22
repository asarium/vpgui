using System;
using System.Collections;
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
    public class ListDragDropHandler : AbstractDropHandler
    {
        public ListDragDropHandler(MainModel applicationModel) : base(applicationModel)
        {
        }

        protected override VPDirectoryEntry GetTargetItem(IDropInfo dropInfo)
        {
            return ApplicationModel.TreeViewModel.SelectedItem.Entry;
        }
    }
}
