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

        private void VpTreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var val = e.NewValue as VpTreeEntryViewModel;

            if (val != null && ApplicationModel.SelectedHistory != null)
            {
                this.ApplicationModel.SelectedHistory.AddHistoryEntry(val);
            }
        }

        #endregion

    }
}
