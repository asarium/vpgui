using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VPGUI.Commands;
using VPGUI.Properties;
using VPGUI.Services;
using VPGUI.Utilities;
using VPGUI.Utilities.Settings;
using VPSharp;
using VPSharp.Entries;
using MessageType = VPGUI.Services.MessageType;

namespace VPGUI.Models
{
    public class MainModel : INotifyPropertyChanged
    {
        private const string DefaultTitle = "VP Viewer Test V0.3";
        private string _busyMessage;
        private VPFile _currentVpFile;
        private VpDirectoryListModel _directoryListModel;
        private bool _fileMessagesShown;

        private bool _isBusy;
        private VPMessagesViewModel _messagesModel;
        private bool _optionsShown;
        private SelectedHistory _selectedHistory;
        private string _statusMessage;
        private string _title = DefaultTitle;
        private VpTreeViewModel _treeViewModel;

        public MainModel(IInteractionService interactionService)
        {
            this.InteractionService = interactionService;
            this.Options = new OptionsModel();

            this.IsBusy = false;

            this.PropertyChanged += this.MainModel_PropertyChanged;
        }

        public bool IsBusy
        {
            get { return this._isBusy; }

            set
            {
                if (this._isBusy != value)
                {
                    this._isBusy = value;

                    this.OnPropertyChanged();
                }
            }
        }

        public string BusyMessage
        {
            get { return this._busyMessage; }

            set
            {
                this._busyMessage = value;

                this.OnPropertyChanged();
            }
        }

        public IInteractionService InteractionService { get; internal set; }

        public string Title
        {
            get { return this._title; }

            internal set
            {
                if (this._title != value)
                {
                    this._title = value;

                    this.OnPropertyChanged();
                }
            }
        }

        public bool FileMessagesShown
        {
            get { return this._fileMessagesShown; }

            set
            {
                if (this._fileMessagesShown != value)
                {
                    this._fileMessagesShown = value;

                    this.OnPropertyChanged();
                }
            }
        }

        public VPFile CurrentVpFile
        {
            get { return this._currentVpFile; }

            set
            {
                if (this._currentVpFile != value)
                {
                    if (this._currentVpFile != null)
                    {
                        this._currentVpFile.Dispose();
                        this._currentVpFile = null;
                    }

                    this._currentVpFile = value;

                    this.OnPropertyChanged();
                }
            }
        }

        public string StatusMessage
        {
            get { return this._statusMessage; }

            set
            {
                if (this._statusMessage != value)
                {
                    this._statusMessage = value;

                    this.OnPropertyChanged();
                }
            }
        }

        public VpTreeViewModel TreeViewModel
        {
            get { return this._treeViewModel; }

            set
            {
                if (this._treeViewModel != value)
                {
                    if (this._treeViewModel != null)
                    {
                        this._treeViewModel.PropertyChanged -= this.TreeViewModelOnPropertyChanged;
                    }

                    this._treeViewModel = value;

                    this._treeViewModel.PropertyChanged += this.TreeViewModelOnPropertyChanged;

                    this.OnPropertyChanged();
                }
            }
        }

        public VpDirectoryListModel DirectoryListModel
        {
            get { return this._directoryListModel; }

            set
            {
                if (this._directoryListModel != value)
                {
                    this._directoryListModel = value;

                    this.OnPropertyChanged();
                }
            }
        }

        public SelectedHistory SelectedHistory
        {
            get { return this._selectedHistory; }

            internal set
            {
                if (this._selectedHistory != null)
                {
                    this._selectedHistory.PropertyChanged -= this.SelectedHistoryOnPropertyChanged;
                }

                this._selectedHistory = value;

                this._selectedHistory.PropertyChanged += this.SelectedHistoryOnPropertyChanged;

                this.OnPropertyChanged();
            }
        }

        public VPMessagesViewModel FileMessagesModel
        {
            get { return this._messagesModel; }

            set
            {
                if (this._messagesModel != value)
                {
                    this._messagesModel = value;

                    this.OnPropertyChanged();
                }
            }
        }

        public bool OptionsShown
        {
            get { return this._optionsShown; }

            set
            {
                if (this._optionsShown != value)
                {
                    this._optionsShown = value;

                    this.OnPropertyChanged();
                }
            }
        }

        public OptionsModel Options { get; internal set; }

        #region Commands

        // File menu
        private ICommand _openCommand;
        private ICommand _saveCommand;
        private ICommand _saveAsCommand;
        private ICommand _closeCommand;

        // Edit menu
        private ICommand _extractFilesCommand;
        private ICommand _addEntriesCommand;
        private ICommand _deleteSelectedCommand;

        // Select menu
        private ICommand _selectAllCommand;
        private ICommand _invertSelectionCommand;

        // No menu
        private ICommand _openSelectedCommand;
        private ICommand _optionsCommand;

        public ICommand OptionsCommand
        {
            get
            {
                if (this._optionsCommand == null)
                {
                    this._optionsCommand = new RelayCommand(param => this.OptionsShown = !this.OptionsShown);
                }

                return this._optionsCommand;
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                if (this._saveCommand == null)
                {
                    this._saveCommand = new SaveCommand(this);
                }

                return this._saveCommand;
            }
        }

        public ICommand SaveAsCommand
        {
            get
            {
                if (this._saveAsCommand == null)
                {
                    this._saveAsCommand = new SaveAsCommand(this);
                }

                return this._saveAsCommand;
            }
        }

        public ICommand OpenCommand
        {
            get
            {
                if (this._openCommand == null)
                {
                    this._openCommand = new RelayCommand(param => this.Open());
                }

                return this._openCommand;
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                if (this._closeCommand == null)
                {
                    this._closeCommand = new RelayCommand(param => this.Close());
                }

                return this._closeCommand;
            }
        }

        public ICommand ExtractFilesCommand
        {
            get
            {
                if (this._extractFilesCommand == null)
                {
                    this._extractFilesCommand = new ExtractFilesCommand(this);
                }

                return this._extractFilesCommand;
            }
        }

        public ICommand OpenSelectedCommand
        {
            get
            {
                if (this._openSelectedCommand == null)
                {
                    this._openSelectedCommand = new OpenSelectedCommand(this);
                }

                return this._openSelectedCommand;
            }
        }

        public ICommand DeleteSelectedCommand
        {
            get
            {
                if (this._deleteSelectedCommand == null)
                {
                    this._deleteSelectedCommand = new DeleteCommand(this);
                }

                return this._deleteSelectedCommand;
            }
        }

        public ICommand SelectAllCommand
        {
            get
            {
                if (this._selectAllCommand == null)
                {
                    this._selectAllCommand = new SelectAllCommand(this);
                }

                return this._selectAllCommand;
            }
        }

        public ICommand InvertSelectionCommand
        {
            get
            {
                if (this._invertSelectionCommand == null)
                {
                    this._invertSelectionCommand = new InvertSelectionCommand(this);
                }

                return this._invertSelectionCommand;
            }
        }

        public ICommand AddEntriesCommand
        {
            get
            {
                if (this._addEntriesCommand == null)
                {
                    this._addEntriesCommand = new AddEntriesCommand(this);
                }

                return this._addEntriesCommand;
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void Close()
        {
            Application.Current.Shutdown();
        }

        private void Open()
        {
            if (!this.IsBusy)
            {
                this.InteractionService.OpenFileDialog("Open VP-File", false, paths => this.OpenVpFile(paths[0]),
                    new FileFilter("VP-file", "*.vp"), new FileFilter("All files", "*.*"));
            }
            else
            {
                SystemSounds.Hand.Play();
            }
        }

        public bool OpenVpFile(string path)
        {
            if (this.IsBusy)
            {
                return false;
            }

            this.IsBusy = true;
            this.BusyMessage = "Loading " + new FileInfo(path).Name + "...";

            this.FileMessagesShown = false;

            this.LoadVPFile(path).ContinueWith(task =>
                {
                    if (task.Exception != null)
                    {
                        this.InteractionService.ShowMessage(MessageType.Error, "Error while reading file",
                                                            "Error while reading VP file:" +
                                                            Util.GetAggregateExceptionMessage(task.Exception));

                        this.CurrentVpFile = null;

                        this.StatusMessage = "Failed to load VP-file.";
                    }
                    else
                    {
                        this.CurrentVpFile = task.Result;

                        this.FileMessagesShown = this.CurrentVpFile.FileMessages.Messages.Count > 0;

                        this.StatusMessage = "VP-file successfully loaded.";
                    }

                    this.IsBusy = false;
                }, TaskScheduler.FromCurrentSynchronizationContext());

            return true;
        }

        private async Task<VPFile> LoadVPFile(string path)
        {
            VPFile file = null;

            try
            {
                file = new VPFile(path);

                await file.ReadIndexAsync();

                return file;
            }
            catch (Exception)
            {
                if (file != null)
                {
                    file.Dispose();
                }

                // Rethrow
                throw;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void MainModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentVpFile")
            {
                if (this.CurrentVpFile != null)
                {
                    this.Title = DefaultTitle + " - " + this.CurrentVpFile.VPFileInfo.Name;

                    this.TreeViewModel = new VpTreeViewModel(this.CurrentVpFile.RootNode);

                    this.FileMessagesModel = new VPMessagesViewModel(this.CurrentVpFile.FileMessages);
                }
                else
                {
                    this.TreeViewModel = null;

                    this.Title = DefaultTitle;

                    this.FileMessagesModel = null;
                }
            }
            else if (e.PropertyName == "TreeViewModel")
            {
                this.SelectedHistory = new SelectedHistory();

                this.DirectoryListModel = new VpDirectoryListModel(this.TreeViewModel.SelectedItem);
            }
        }

        private void TreeViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "SelectedItem")
            {
                this.DirectoryListModel = new VpDirectoryListModel(this.TreeViewModel.SelectedItem);
            }
        }

        private void SelectedHistoryOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "CurrentEntry")
            {
                // Don't add new entires when we navigate in the history
                this.SelectedHistory.SuppressNewEntries = true;

                this.SelectedHistory.CurrentEntry.IsSelected = true;

                this.SelectedHistory.SuppressNewEntries = false;
            }
        }

        private static string GetExtractionLocation(VPFile file)
        {
            switch (Settings.Default.ExtractLocation)
            {
                case ExtractLocation.WorkingDir:
                    return Directory.GetCurrentDirectory() + "\\" + file.VPFileInfo.Name;
                case ExtractLocation.VpLocation:
                    if (file.VPFileInfo.Directory != null)
                    {
                        return file.VPFileInfo.Directory.FullName;
                    }
                    else
                    {
                        return Directory.GetCurrentDirectory() + "\\" + file.VPFileInfo.Name;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async Task<string> ExtractEntryAsync(VPEntry entry, string extractPath = null,
                                                    VPDirectoryEntry parentEntry = null)
        {
            if (extractPath == null)
            {
                extractPath = GetExtractionLocation(this.CurrentVpFile);
            }

            string subPath = entry.Path;
            if (parentEntry != null)
            {
                subPath = subPath.Remove(0, parentEntry.Path.Length);
            }

            string path = extractPath + subPath;

            DirectoryInfo directoryInfo = new FileInfo(path).Directory;
            if (directoryInfo != null && !directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            VPFileEntry fileEntry = entry as VPFileEntry;

            if (fileEntry != null)
            {
                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    using (Stream st = fileEntry.OpenFileStream())
                    {
                        await st.CopyToAsync(stream);
                    }
                }
            }
            else
            {
                VPDirectoryEntry dirEntry = entry as VPDirectoryEntry;

                if (dirEntry != null)
                {
                    foreach (VPEntry child in dirEntry.Children)
                    {
                        await this.ExtractEntryAsync(child, extractPath, parentEntry);
                    }
                }
            }

            return path;
        }

        public void OpenEntry(VPEntry entry, VpEntryView<VPEntry> viewEntry = null)
        {
            VPFileEntry fileEntry = entry as VPFileEntry;
            if (fileEntry != null)
            {
                this.OpenFileEntry(fileEntry);
            }
            else
            {
                VPDirectoryEntry directoryEntry = entry as VPDirectoryEntry;

                if (directoryEntry != null)
                {
                    VpListDirEntryModel modelEntry = viewEntry as VpListDirEntryModel;

                    if (modelEntry != null)
                    {
                        modelEntry =
                            this.DirectoryListModel.Entries.First(item => directoryEntry == item.Entry) as
                            VpListDirEntryModel;
                    }

                    if (modelEntry != null)
                    {
                        modelEntry.TreeModelDirEntry.IsSelected = true;

                        if (modelEntry.TreeModelDirEntry.ModelParent != null)
                        {
                            modelEntry.TreeModelDirEntry.ModelParent.IsExpanded = true;
                        }
                    }
                }
            }
        }

        private void OpenFileEntry(VPFileEntry vPFileEntry)
        {
            this.ExtractEntryAsync(vPFileEntry).ContinueWith(task =>
                {
                    if (task.Exception != null)
                    {
                        this.InteractionService.ShowMessage(MessageType.Error, "Error while writing VP entry",
                                                            "Error while saving VP entry for opening:" +
                                                            Util.GetAggregateExceptionMessage(task.Exception));
                    }
                    else
                    {
                        Process.Start(task.Result);
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        internal async Task AddFilePath(string filePath, VPDirectoryEntry parent = null, bool showMessage = true)
        {
            if (this.CurrentVpFile == null || this.TreeViewModel == null || this.TreeViewModel.SelectedItem == null)
            {
                return;
            }

            if (!File.Exists(filePath) && !Directory.Exists(filePath))
            {
                return;
            }

            VPDirectoryEntry dir;

            if (parent == null)
            {
                VPTreeEntryViewModel currentDir = this.TreeViewModel.SelectedItem;

                dir = currentDir.Entry;
            }
            else
            {
                dir = parent;
            }

            FileAttributes attr = File.GetAttributes(filePath);

            if ((attr & FileAttributes.Directory) != 0)
            {
                var info = new DirectoryInfo(filePath);
                // Directory operation...
                var dirEntry = new VPDirectoryEntry(dir) {Name = info.Name};

                dir.AddChild(dirEntry);

                foreach (var child in info.EnumerateFileSystemInfos())
                {
                    await this.AddFilePath(child.FullName, dirEntry, false);
                }
            }
            else
            {
                // File operation
                dir.AddChild(new VPFileSystemEntry(dir, filePath));
            }

            if (showMessage)
            {
                this.StatusMessage = "'" + filePath + "' has been added...";
            }
        }
    }
}
