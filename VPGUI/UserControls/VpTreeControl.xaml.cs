using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VPGUI.Models;
using VPGUI.Utilities;

namespace VPGUI.UserControls
{
    /// <summary>
    /// Interaction logic for VpTreeControl.xaml
    /// </summary>
    public partial class VpTreeControl : UserControl
    {
        public VpTreeControl()
        {
            InitializeComponent();
        }

        #region Dependency Properties

        // Using a DependencyProperty as the backing store for StatusBarText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ApplicationModelProperty = DependencyProperty.Register("ApplicationModel", typeof(MainModel),
            typeof(VpTreeControl));

        public MainModel ApplicationModel
        {
            get
            {
                return (MainModel) this.GetValue(ApplicationModelProperty);
            }
            set
            {
                this.SetValue(ApplicationModelProperty, value);
            }
        }

        #endregion

        #region Event Handlers

        private object _currentTreeItem;
        private DateTime _dragHoverBegin;

        private void treeItem_DragEnter(object sender, DragEventArgs e)
        {
            if (this._currentTreeItem != sender)
            {
                this._currentTreeItem = sender;
                this._dragHoverBegin = DateTime.UtcNow;
            }

            e.Handled = true;
            e.Effects = DragDropEffects.Copy;
        }

        private void treeItem_DragLeave(object sender, DragEventArgs e)
        {
            this._currentTreeItem = null;

            e.Handled = true;
        }

        private void treeItem_DragOver(object sender, DragEventArgs e)
        {
            VPTreeEntryViewModel entry = (((FrameworkElement) sender).DataContext) as VPTreeEntryViewModel;

            if (entry != null)
            {
                if (this._currentTreeItem == sender && (DateTime.UtcNow - this._dragHoverBegin).TotalMilliseconds > 1000)
                {
                    entry.IsExpanded = true;
                }

                entry.IsSelected = true;

                e.Handled = true;
                e.Effects = DragDropEffects.Copy;
            }
        }

        private void treeItem_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                foreach (string dropPath in (string[]) e.Data.GetData(DataFormats.FileDrop))
                {
                    this.ApplicationModel.AddFilePath(dropPath).ContinueWith(task =>
                    {
                        if (task.Exception != null)
                        {
                            this.ApplicationModel.StatusMessage = "Couldn't add entry: " +
                                                                  Util.GetAggregateExceptionMessage(task.Exception);
                        }
                    });
                }
            }
        }

        private void VpTreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var val = e.NewValue as VPTreeEntryViewModel;

            this.ApplicationModel.SelectedHistory.AddHistoryEntry(val);
        }

        #endregion

    }
}
