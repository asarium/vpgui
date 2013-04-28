using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ookii.Dialogs.Wpf;
using VPGUI.Models;
using VPGUI.Properties;

namespace VPGUI.UserControls
{
    /// <summary>
    ///     Interaction logic for OptionsControl.xaml
    /// </summary>
    public partial class OptionsControl : UserControl
    {
        public OptionsControl()
        {
            this.InitializeComponent();
        }

        private void TempPath_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var box = (TextBox) sender;

            var dlg = new VistaFolderBrowserDialog
            {
                SelectedPath = Settings.Default.TempPath
            };

            var b = dlg.ShowDialog(Window.GetWindow(this));

            if (b.HasValue && b.Value)
            {
                Settings.Default.TempPath = dlg.SelectedPath;
            }
        }
    }
}
