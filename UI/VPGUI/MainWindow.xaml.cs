using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using VPGUI.Models;
using VPGUI.Services;
using VPGUI.Utilities;
using VPSharp.Entries;

namespace VPGUI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IInteractionService
    {
        #region Members

        private MainModel _applicationModel;

        public MainModel ApplicationModel
        {
            get { return this._applicationModel; }

            set
            {
                this._applicationModel = value;
                this.DataContext = this.ApplicationModel;
            }
        }

        #endregion

        #region DependencyProperties

        // Using a DependencyProperty as the backing store for StatusBarText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusBarTextProperty =
            DependencyProperty.Register("StatusBarText", typeof (string), typeof (MainWindow),
                                        new UIPropertyMetadata(""));

        public string StatusBarText
        {
            get { return (string) this.GetValue(StatusBarTextProperty); }
            set { this.SetValue(StatusBarTextProperty, value); }
        }

        #endregion

        #region Constructors

        public MainWindow()
        {
            this.InitializeComponent();

            Binding b = new Binding("StatusMessage");
            this.SetBinding(StatusBarTextProperty, b);
        }

        #endregion

        #region Event Handlers

        private void VPViewElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.XButton1)
            {
                // Back
                SelectedHistory history = this.ApplicationModel.SelectedHistory;
                history.CurrentIndex = history.CurrentIndex - e.ClickCount;
            }
            else if (e.ChangedButton == MouseButton.XButton2)
            {
                // Forward
                SelectedHistory history = this.ApplicationModel.SelectedHistory;
                history.CurrentIndex = history.CurrentIndex + e.ClickCount;
            }
        }

        #endregion

        #region IInteractionService implementation

        public void ShowMessage(MessageType type, string title, string text)
        {
            var image = MessageBoxImage.None;

            switch (type)
            {
                case MessageType.Warning:
                    image = MessageBoxImage.Warning;
                    break;
                case MessageType.Error:
                    image = MessageBoxImage.Error;
                    break;
                case MessageType.Information:
                    image = MessageBoxImage.Information;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }

            MessageBox.Show(this, text, title, MessageBoxButton.OK, image);
        }

        public void OpenFileDialog(string title, bool multiple, PathsSelectedDelegate selectedCallback, params FileFilter[] filters)
        {
            var dialog = new OpenFileDialog
            {
                Title = title,
                Filter = this.BuilderFilter(filters),
                Multiselect = multiple
            };

            var b = dialog.ShowDialog(this);

            if (b.HasValue && b.Value)
            {
                selectedCallback(dialog.FileNames);
            }
        }

        public void SaveFileDialog(string title, PathSelectedDelegate selectedCallback, params FileFilter[] filters)
        {
            var dialog = new SaveFileDialog
            {
                Title = title,
                OverwritePrompt = true,
                Filter = this.BuilderFilter(filters),
            };

            var b = dialog.ShowDialog(this);

            if (b.HasValue && b.Value)
            {
                selectedCallback(dialog.FileName);
            }
        }

        public void SaveDirectoryDialog(string title, PathSelectedDelegate selectedCallback)
        {
            var dlg = new VistaFolderBrowserDialog {Description = title};

            var b = dlg.ShowDialog(this);

            if (b.HasValue && b.Value)
            {
                selectedCallback(dlg.SelectedPath);
            }
        }

        private string BuilderFilter(FileFilter[] filters)
        {
            var builder = new StringBuilder();

            for (int i = 0; i < filters.Length; i++)
            {
                FileFilter f = filters[i];

                builder.Append(f.Description);
                builder.Append('|');
                builder.Append(f.Filter);

                if (i < filters.Length - 1)
                {
                    builder.Append('|');
                }
            }

            return builder.ToString();
        }

        #endregion
    }
}
