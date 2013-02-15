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
using VPSharp.Entries;

namespace VPGUI.UserControls
{
    /// <summary>
    /// Interaction logic for VpDirectoryControl.xaml
    /// </summary>
    public partial class VpDirectoryControl : UserControl
    {

        // Using a DependencyProperty as the backing store for StatusBarText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ApplicationModelProperty = DependencyProperty.Register("ApplicationModel", typeof(MainModel),
            typeof(VpDirectoryControl));

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

        public VpDirectoryControl()
        {
            InitializeComponent();
        }

        #region Event handlers

        private void vpListViewEntry_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = sender as ListViewItem;

            if (item != null && item.Content is VpEntryView<VPEntry>)
            {
                var vpEntryView = item.Content as VpEntryView<VPEntry>;
                this.ApplicationModel.OpenEntry(vpEntryView.Entry, vpEntryView);
            }
        }

        private void vpListView_Drop_1(object sender, DragEventArgs e)
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

        private void vpListView_DragEnter_1(object sender, DragEventArgs e)
        {
            e.Handled = true;
            e.Effects = DragDropEffects.Copy;
        }

        private void vpListView_DragOver_1(object sender, DragEventArgs e)
        {
            e.Handled = true;
            e.Effects = DragDropEffects.Copy;
        }


        private void ListViewEditBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var box = sender as TextBox;

            if (box != null)
            {
                var item = box.DataContext as VpEntryView<VPEntry>;

                item.IsEditing = false;
            }
        }

        #endregion
    }
}
