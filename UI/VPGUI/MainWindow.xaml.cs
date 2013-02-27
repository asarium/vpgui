using System;
using System.ComponentModel;
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

        private bool _closingWindow = false;

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

        private void CloseItem_Click(object sender, RoutedEventArgs e)
        {
            HandleClose();

            _closingWindow = true;
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!_closingWindow)
            {
                HandleClose(b => e.Cancel = !b);
            }
        }

        #endregion

        public void HandleClose(Action<bool> callback = null)
        {
            if (ApplicationModel.CurrentVpFile != null && ApplicationModel.CurrentVpFile.RootNode.Changed)
            {
                this.ShowQuestion(MessageType.Question, QuestionType.YesNo, "Unsaved changes", 
                    "There are unsaved changes in the file. Do you really want to quit?", answer =>
                        {
                            if (callback != null)
                            {
                                callback(answer == QuestionAnswer.Yes);
                            }

                            if (answer == QuestionAnswer.Yes)
                            {
                                Application.Current.Shutdown();
                            }
                        });
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

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
                case MessageType.Question:
                    image = MessageBoxImage.Question;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }

            MessageBox.Show(this, text, title, MessageBoxButton.OK, image);
        }

        public void ShowQuestion(MessageType type, QuestionType questionType, string title, string text, QuestionDelegate callback)
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
                case MessageType.Question:
                    image = MessageBoxImage.Question;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }

            var buttons = MessageBoxButton.YesNo;

            switch (questionType)
            {
                case QuestionType.YesNo:
                    buttons = MessageBoxButton.YesNo;
                    break;
                case QuestionType.YesNoCancel:
                    buttons = MessageBoxButton.YesNoCancel;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("questionType");
            }

            var result = MessageBox.Show(this, text, title, buttons, image, MessageBoxResult.No);

            QuestionAnswer answer;

            switch (result)
            {
                case MessageBoxResult.None:
                    answer = QuestionAnswer.Cancel;
                    break;
                case MessageBoxResult.OK:
                    answer = QuestionAnswer.Yes;
                    break;
                case MessageBoxResult.Cancel:
                    answer = QuestionAnswer.Cancel;
                    break;
                case MessageBoxResult.Yes:
                    answer = QuestionAnswer.Yes;
                    break;
                case MessageBoxResult.No:
                    answer = QuestionAnswer.No;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            callback(answer);
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
