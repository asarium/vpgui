using System;
using System.ComponentModel;
using System.Configuration;
using MahApps.Metro;

namespace VPGUI.Properties
{
    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
    public sealed partial class Settings
    {
        public AppTheme Theme
        {
            get => ThemeManager.GetAppTheme(ThemeStr) ?? ThemeManager.GetAppTheme("BaseDark");
            set => ThemeStr = value.Name;
        }

        public Accent Accent
        {
            get => ThemeManager.GetAccent(ThemeAccentStr);
            set => ThemeAccentStr = value.Name;
        }
    }
}
