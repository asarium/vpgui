using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using MahApps.Metro;
using Ookii.Dialogs.Wpf;
using VPGUI.Models;
using VPGUI.Properties;
using VPGUI.Services;

namespace VPGUI
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup_1(object sender, StartupEventArgs e)
        {
            InitializeSettings();

            string vpPath = null;

            if (e.Args.Length > 0)
            {
                vpPath = e.Args[e.Args.Length - 1];
            }
            else
            {
                var info = AppDomain.CurrentDomain.SetupInformation;
                var args = info.ActivationArguments;

                if (args != null)
                {
                    var activationData = args.ActivationData;
                    if (activationData != null && activationData.Length > 0)
                    {
                        vpPath = activationData[activationData.Length - 1];
                    }
                }
            }

            this.MainWindow = new MainWindow();

            var model = new MainModel((IInteractionService) this.MainWindow);
            if (vpPath != null)
            {
                model.OpenVpFile(vpPath);
            }

            ((MainWindow) this.MainWindow).ApplicationModel = model;

            MainWindow.WindowState = Settings.Default.Maximized ? WindowState.Maximized : WindowState.Normal;

            this.MainWindow.Show();

            ThemeManager.ChangeAppStyle(this.MainWindow, Settings.Default.Accent, Settings.Default.Theme);
        }

        private void InitializeSettings()
        {
            if (Settings.Default.Accent == null)
            {
                Settings.Default.Accent = ThemeManager.Accents.First(a => a.Name == "Blue");
            }

            if (Settings.Default.TempPath.Length <= 0 || !Directory.Exists(Settings.Default.TempPath))
            {
                Settings.Default.TempPath = Path.GetTempPath();
            }

            // Clamp the values so the user has the chance to move the window even when some sort of error
            // or action made the value invalid
            Settings.Default.Top = Math.Max(0, Math.Min(Settings.Default.Top, SystemParameters.PrimaryScreenHeight - 50));
            Settings.Default.Left = Math.Max(0, Math.Min(Settings.Default.Left, SystemParameters.PrimaryScreenWidth - 50));

            Settings.Default.PropertyChanged += this.Settings_PropertyChanged;
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var settings = sender as Settings;

            if (settings != null && (e.PropertyName == "ThemeStr" || e.PropertyName == "ThemeAccentStr"))
            {
                ThemeManager.ChangeAppStyle(this.MainWindow, settings.Accent, settings.Theme);
            }
        }

        private void Application_Exit_1(object sender, ExitEventArgs e)
        {
            Settings.Default.Save();
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var dlg = new TaskDialog();
            dlg.ButtonStyle = TaskDialogButtonStyle.Standard;
            dlg.Content = "An unexpected error has occured. The application can not continue to execute in this state!\n" +
                          "See below for more information. Please report this error along with the generated informations. " +
                          "All relevant informations are automatically copied to your clipboard.";
            dlg.ExpandedInformation = e.Exception.ToString();
            dlg.MainInstruction = "Unhandled Exception!";
            dlg.MainIcon = TaskDialogIcon.Error;
            dlg.WindowTitle = "Unhandled Exception!";
            dlg.ExpandedControlText = "Show more informations";

            dlg.VerificationText = "Save error to clipboard";
            dlg.IsVerificationChecked = true;

            dlg.Buttons.Add(new TaskDialogButton(ButtonType.Close));

            dlg.ShowDialog(this.MainWindow);

            if (dlg.IsVerificationChecked)
            {
                Clipboard.SetText(e.Exception.ToString(), TextDataFormat.UnicodeText);
            }

            Process.GetCurrentProcess().Kill();
        }
    }
}
