﻿using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using VPGUI.Models;
using VPGUI.Properties;
using VPGUI.Utilities;
using VPGUI.Utilities.Settings;
using VPSharp;
using MessageType = VPGUI.Services.MessageType;

namespace VPGUI.Commands
{
    public class SaveCommand : MainModelCommand
    {
        protected VPFile FileInstance;

        public SaveCommand(MainModel mainModel) : base(mainModel)
        {
            this.ApplicationModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == "CurrentVpFile")
                    {
                        if (this.FileInstance != null)
                        {
                            this.FileInstance.RootNode.PropertyChanged -= this.RootNodeOnPropertyChanged;
                        }

                        this.FileInstance = this.ApplicationModel.CurrentVpFile;

                        if (FileInstance != null)
                        {
                            this.FileInstance.RootNode.PropertyChanged += this.RootNodeOnPropertyChanged;
                        }

                        FireCanExecuteChanged();
                    }
                };
        }

        private void RootNodeOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "Changed")
            {
                this.FireCanExecuteChanged();
            }
        }

        public override bool CanExecute(object parameter)
        {
            return this.FileInstance != null && this.FileInstance.RootNode.Changed;
        }

        public override void Execute(object parameter)
        {
            ApplicationModel.SaveVpFile(null, BackupCallback);
        }

        protected bool BackupCallback(FileInfo info)
        {
            if (Settings.Default.CreateBackups)
            {
                var current = Directory.GetCurrentDirectory();

                Directory.CreateDirectory(current + @"\backups\");

                var backupName = current + @"\backups\" + info.Name;

                if (File.Exists(backupName))
                {
                    File.Delete(backupName);
                }

                File.Copy(info.FullName, backupName);
            }

            return true;
        }
    }
}
