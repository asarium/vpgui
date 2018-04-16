using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using MahApps.Metro;
using MahApps.Metro.Controls;
using VPGUI.Properties;
using VPGUI.Utilities.Settings;

namespace VPGUI.Models
{
    public class OptionsModel : INotifyPropertyChanged
    {
        private readonly Settings _settings;
        private AccentView _currentAccentView;
        private ThemeView _currentThemeView;

        public OptionsModel() : this(Settings.Default)
        {
        }

        public OptionsModel(Settings settingsInstance)
        {
            this._settings = settingsInstance;

            this._settings.PropertyChanged += this.SettingsOnPropertyChanged;

            this.AvailableAccents = ThemeManager.Accents.Select(item => new AccentView(item));
            this._currentAccentView = this.AvailableAccents.First(item => item.Accent.Name == this._settings.Accent.Name);

            this.AvailableThemes = ThemeManager.AppThemes.Select(item => new ThemeView(item));
            this._currentThemeView = this.AvailableThemes.First(item => item.Theme.Name == this._settings.Theme.Name);
        }

        public ThemeView Theme
        {
            get => _currentThemeView;

            set
            {
                if (!Equals(_currentThemeView, value))
                {
                    this._settings.Theme = value.Theme;
                }
            }
        }

        public AccentView ThemeAccent
        {
            get => this._currentAccentView;

            set
            {
                if (!Equals(this._currentAccentView, value))
                {
                    this._settings.Accent = value.Accent;
                }
            }
        }

        public IEnumerable<AccentView> AvailableAccents { get; private set; }

        public IEnumerable<ThemeView> AvailableThemes { get; private set; }

        public ExtractLocation ExtractLocation
        {
            get { return this._settings.ExtractLocation; }

            set { this._settings.ExtractLocation = value; }
        }

        public bool CreateBackups
        {
            get { return this._settings.CreateBackups; }

            set { this._settings.CreateBackups = value; }
        }

        public double Left
        {
            get { return _settings.Left; }

            set { _settings.Left = value; }
        }

        public double Top
        {
            get
            {
                return _settings.Top;
            }

            set
            {
                _settings.Top = value;
            }
        }

        public double Width
        {
            get
            {
                return _settings.Width;
            }

            set
            {
                _settings.Width = value;
            }
        }

        public double Height
        {
            get
            {
                return _settings.Height;
            }

            set
            {
                _settings.Height = value;
            }
        }

        public bool Maximized
        {
            get
            {
                return _settings.Maximized;
            }

            set
            {
                _settings.Maximized = value;
            }
        }

        public bool CheckForUpdates
        {
            get
            {
                return _settings.CheckForUpdates;
            }

            set
            {
                _settings.CheckForUpdates = value;
            }
        }

        public string TempPath
        {
            get
            {
                return _settings.TempPath;
            }

            set
            {
                _settings.TempPath = value;
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ThemeAccentStr")
            {
                this._currentAccentView = this.AvailableAccents.First(item => item.Accent.Name == this._settings.Accent.Name);
                OnPropertyChanged("ThemeAccent");
                return;
            }
            if (e.PropertyName == "ThemeStr")
            {
                this._currentThemeView =
                    this.AvailableThemes.First(item => item.Theme.Name == this._settings.Accent.Name);
                OnPropertyChanged("Theme");
                return;
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

    public class ThemeView
    {
        public readonly AppTheme Theme;

        public ThemeView(AppTheme theme)
        {
            Theme = theme;
        }

        public string Name
        {
            get
            {
                if (Theme.Name.StartsWith("Base"))
                {
                    return Theme.Name.Substring("Base".Length);
                }
                else
                {
                    return Theme.Name;
                }
            }
        }

        protected bool Equals(ThemeView other)
        {
            return Equals(Theme, other.Theme);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ThemeView) obj);
        }

        public override int GetHashCode()
        {
            return (Theme != null ? Theme.GetHashCode() : 0);
        }
    }
}
