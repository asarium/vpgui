using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using MahApps.Metro;
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
            if (Settings.Default.ThemeAccent == null)
            {
                Settings.Default.ThemeAccent = ThemeManager.DefaultAccents.First(a => a.Name == "Blue");
            }

            Settings.Default.PropertyChanged += this.Settings_PropertyChanged;

            string vpPath = null;

            if (e.Args.Length > 0)
            {
                vpPath = e.Args[e.Args.Length - 1];
            }

            this.MainWindow = new MainWindow();

            ThemeManager.ChangeTheme(this.MainWindow, Settings.Default.ThemeAccent, Settings.Default.Theme);

            var model = new MainModel((IInteractionService) this.MainWindow);
            if (vpPath != null)
            {
                model.OpenVpFile(vpPath);
            }

            ((MainWindow) this.MainWindow).ApplicationModel = model;

            this.MainWindow.Show();
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var settings = sender as Settings;

            if (settings != null && (e.PropertyName == "Theme" || e.PropertyName == "ThemeAccent"))
            {
                ThemeManager.ChangeTheme(this.MainWindow, settings.ThemeAccent, settings.Theme);
            }
        }

        private void Application_Exit_1(object sender, ExitEventArgs e)
        {
            Settings.Default.Save();
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Unhandled Exception:\n" + e.Exception.Message + "\n" + e.Exception.StackTrace,
                            "Unhandled Exception!");

            Process.GetCurrentProcess().Kill();
        }
    }
}
