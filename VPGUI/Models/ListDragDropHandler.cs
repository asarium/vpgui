using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GongSolutions.Wpf.DragDrop;

namespace VPGUI.Models
{
    public class ListDragDropHandler : IDropTarget, IDragSource
    {
        private MainModel ApplicationModel;

        public ListDragDropHandler(MainModel applicationModel)
        {
            this.ApplicationModel = applicationModel;
        }

        public void DragOver(IDropInfo dropInfo)
        {

        }

        public void Drop(IDropInfo dropInfo)
        {
        }

        public void StartDrag(IDragInfo dragInfo)
        {
        }

        public void Dropped(IDropInfo dropInfo)
        {
        }
    }
}
