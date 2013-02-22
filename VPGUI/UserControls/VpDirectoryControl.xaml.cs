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
            var item = sender as ListViewItem;

            if (item != null && item.Content is IEntryView<VPEntry>)
            {
                var vpEntryView = item.Content as IEntryView<VPEntry>;
                this.ApplicationModel.OpenEntry(vpEntryView.Entry, vpEntryView);
            }
        }

        #region ListViewEditBox

        private void ListViewEditBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var box = sender as TextBox;

            if (box != null)
            {
                var item = box.DataContext as IEntryView<VPEntry>;

                if (item != null)
                {
                    item.IsEditing = false;
                }
            }
        }

        private void ListViewEditBox_OnIsVisibleChange(object sender, DependencyPropertyChangedEventArgs e)
        {
            var box = sender as TextBox;

            if (box != null)
            {
                box.Focus();
                box.SelectAll();
            }
        }

        private void ListViewEditBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.VpListView.Focus();
            }
        }

        #endregion

        #endregion
    }
}
