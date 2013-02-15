using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using MahApps.Metro;
using VPGUI.Properties;
using VPGUI.Utilities.Settings;

namespace VPGUI.Models
{
    public class OptionsModel : INotifyPropertyChanged
    {
        private readonly Settings _settings;
        private AccentView _currentAccentView;

        public OptionsModel() : this(Settings.Default)
        {
        }

        public OptionsModel(Settings settingsInstance)
        {
            this._settings = settingsInstance;

            this._settings.PropertyChanged += this.SettingsOnPropertyChanged;

            this.AvailableAccents = ThemeManager.DefaultAccents.Select(item => new AccentView(item));
            this._currentAccentView = this.AvailableAccents.First(item => item.Accent == this._settings.ThemeAccent);
        }

        public Theme Theme
        {
            get { return this._settings.Theme; }

            set
            {
                if (this.Theme != value)
                {
                    this._settings.Theme = value;
                }
            }
        }

        public AccentView ThemeAccent
        {
            get { return this._currentAccentView; }

            set
            {
                if (!Equals(this._currentAccentView, value))
                {
                    this._settings.ThemeAccent = value.Accent;
                }
            }
        }

        public IEnumerable<AccentView> AvailableAccents { get; private set; }

        public ExtractLocation ExtractLocation
        {
            get { return this._settings.ExtractLocation; }

            set { this._settings.ExtractLocation = value; }
        }

        public BackupMode BackupMode
        {
            get { return this._settings.BackupMode; }

            set { this._settings.BackupMode = value; }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ThemeAccent")
            {
                this._currentAccentView = this.AvailableAccents.First(item => item.Accent == this._settings.ThemeAccent);
            }

            this.OnPropertyChanged(e.PropertyName);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class AccentView
    {
        public readonly Accent Accent;

        public AccentView(Accent accent)
        {
            this.Accent = accent;
        }

        public string Name
        {
            get { return this.Accent.Name; }
        }

        public ResourceDictionary Resources
        {
            get { return this.Accent.Resources; }
        }

        public override int GetHashCode()
        {
            return this.Accent.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            if (!(obj is AccentView))
            {
                return false;
            }

            return this.Accent.Name == ((AccentView) obj).Name;
        }
    }
}
