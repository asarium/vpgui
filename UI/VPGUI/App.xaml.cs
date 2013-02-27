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
            InitializeSettings();

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

            MainWindow.WindowState = Settings.Default.Maximized ? WindowState.Maximized : WindowState.Normal;

            this.MainWindow.Show();
        }

        private void InitializeSettings()
        {
            if (Settings.Default.ThemeAccent == null)
            {
                Settings.Default.ThemeAccent = ThemeManager.DefaultAccents.First(a => a.Name == "Blue");
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
            MessageBox.Show("Unhandled Exception:\n" + e.Exception.ToString(),
                            "Unhandled Exception!");

            Process.GetCurrentProcess().Kill();
        }
    }
}
