using System;
using System.Collections.Generic;
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

            TreeDNDHandler = new TreeDragDropHandler(this);
            ListDNDHandler = new ListDragDropHandler(this);
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

        public TreeDragDropHandler TreeDNDHandler { get; private set; }

        public ListDragDropHandler ListDNDHandler { get; private set; }

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

                    if (_treeViewModel != null)
                    {
                        this._treeViewModel.PropertyChanged += this.TreeViewModelOnPropertyChanged;
                    }

                    this.OnPropertyChanged();
                }
            }
        }

        public VpDirectoryListModel DirectoryListModel
        {
            get { return this._directoryListModel; }

            private set
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

            private set
            {
                if (this._selectedHistory != null)
                {
                    this._selectedHistory.PropertyChanged -= this.SelectedHistoryOnPropertyChanged;
                }

                this._selectedHistory = value;

                if (_selectedHistory != null)
                {
                    this._selectedHistory.PropertyChanged += this.SelectedHistoryOnPropertyChanged;
                }

                this.OnPropertyChanged();
            }
        }

        public VPMessagesViewModel FileMessagesModel
        {
            get { return this._messagesModel; }

            private set
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
        private ICommand _closeFileCommand;
        private ICommand _saveCommand;
        private ICommand _saveAsCommand;

        // Edit menu
        private ICommand _extractFilesCommand;
        private ICommand _addEntriesCommand;
        private ICommand _newDirectoryCommand;
        private ICommand _deleteSelectedCommand;
        private ICommand _renameCommand;

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

        public ICommand RenameCommand
        {
            get
            {
                if (this._renameCommand == null)
                {
                    this._renameCommand = new RenameCommand(this);
                }

                return this._renameCommand;
            }
        }

        public ICommand CloseFileCommand
        {
            get
            {
                if (_closeFileCommand == null)
                {
                    _closeFileCommand = new CloseFileCommand(this);
                }

                return _closeFileCommand;
            }
        }

        public ICommand NewDirectoryCommand
        {
            get
            {
                if (_newDirectoryCommand == null)
                {
                    _newDirectoryCommand = new NewDirectoryCommand(this);
                }

                return _newDirectoryCommand;
            }
        }

        private readonly UpdateStatusModel _updateStatus = new UpdateStatusModel();
        public object UpdateStatus
        {
            get
            {
                return _updateStatus;
            }
        }

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

        public void SaveVpFile(string path = null, VPFile.OverwriteCallback callback = null)
        {
            if (this.CurrentVpFile == null)
            {
                return;
            }

            this.IsBusy = true;
            this.BusyMessage = path == null ? "Saving file..." :
                "Saving file to " + new FileInfo(path).Name + "...";

            this.CurrentVpFile.WriteVPAsync(null, callback).ContinueWith(task =>
            {
                this.IsBusy = false;

                if (task.Exception != null)
                {
                    this.InteractionService.ShowMessage(MessageType.Error,
                                                                            "Error while writing file",
                                                                            "Error while writing VP file:" +
                                                                            Util.GetAggregateExceptionMessage(
                                                                                task.Exception));

                    this.StatusMessage = "Failed to save VP-file.";
                }
                else
                {
                    this.InteractionService.ShowMessage(MessageType.Information,
                                                                            "Writing completed",
                                                                            "VP-file was successfully written.");

                    this.StatusMessage = String.Format("VP-file successfully saved to {0}.",
                                                                        task.Result);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void MainModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentVpFile":
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
                    break;
                case "TreeViewModel":
                    if (this.TreeViewModel != null)
                    {
                        this.SelectedHistory = new SelectedHistory();

                        this.DirectoryListModel = new VpDirectoryListModel(this.TreeViewModel.SelectedItem);
                    }
                    else
                    {
                        this.SelectedHistory = null;

                        this.DirectoryListModel = null;
                    }
                    break;
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
                    return Path.Combine(Directory.GetCurrentDirectory(), file.VPFileInfo.Name);
                case ExtractLocation.VpLocation:
                    if (file.VPFileInfo.Directory != null)
                    {
                        return file.VPFileInfo.Directory.FullName;
                    }
                    else
                    {
                        return Path.Combine(Directory.GetCurrentDirectory(), file.VPFileInfo.Name);
                    }
                case ExtractLocation.TempPath:
                    return Path.Combine(Settings.Default.TempPath, file.VPFileInfo.Name);
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

            var subPath = entry.Path;
            if (parentEntry != null)
            {
                subPath = subPath.Remove(0, parentEntry.Path.Length);
            }

            var path = extractPath + subPath;

            var directoryInfo = new FileInfo(path).Directory;
            if (directoryInfo != null && !directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            var fileEntry = entry as VPFileEntry;

            if (fileEntry != null)
            {
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    using (var st = fileEntry.OpenFileStream())
                    {
                        await st.CopyToAsync(stream);
                    }
                }
            }
            else
            {
                var dirEntry = entry as VPDirectoryEntry;

                if (dirEntry != null)
                {
                    foreach (var child in dirEntry.Children)
                    {
                        await this.ExtractEntryAsync(child, extractPath, parentEntry);
                    }
                }
            }

            return path;
        }

        public void OpenEntry(VPEntry entry, IEntryView<VPEntry> viewEntry = null)
        {
            var fileEntry = entry as VPFileEntry;
            if (fileEntry != null)
            {
                this.OpenFileEntry(fileEntry);
            }
            else
            {
                var directoryEntry = entry as VPDirectoryEntry;

                if (directoryEntry != null)
                {
                    var modelEntry = viewEntry as VpListDirEntryModel;

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

        public void AddFilePaths(IEnumerable<string> paths, VPDirectoryEntry parent = null)
        {
            IsBusy = true;
            BusyMessage = "Adding files...";

            Task.Run(async () =>
            {
                foreach (var path in paths)
                {
                    BusyMessage = "Adding " + new FileInfo(path).Name + "...";

                    await AddFilePath(path, parent);
                }
            }).ContinueWith(task => IsBusy = false);
        }

        private async Task AddFilePath(string filePath, VPDirectoryEntry parent = null, bool showMessage = true)
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
                VpTreeEntryViewModel currentDir = this.TreeViewModel.SelectedItem;

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

        public IEntryView<VPEntry> CreateDirectory(VPDirectoryEntry parent = null, string name = null)
        {
            if (parent == null)
            {
                parent = TreeViewModel.SelectedItem.Entry;
            }

            var newEntry = new VPDirectoryEntry(parent);

            if (name != null)
            {
                newEntry.Name = name;
            }
            else
            {
                int i = 0;
                string newName = null;

                do
                {
                    if (i == 0)
                    {
                        newName = "New directory";
                    }
                    else
                    {
                        newName = "New directory (" + i + ")";
                    }
                } while (parent.Children.Any(child => child.Name == newName));

                newEntry.Name = newName;
            }

            parent.AddChild(newEntry);

            return DirectoryListModel.FindEntryView(newEntry);
        }

        public async Task<IEnumerable<string>> ExtractEntriesAsync(string path = null, IEnumerable<VPEntry> entries = null)
        {
            if (entries == null)
            {
                entries = DirectoryListModel.SelectedEntries.Select(view => view.Entry);
            }

            var entriesArray = entries as VPEntry[] ?? entries.ToArray();

            if (entriesArray.Length <= 0)
            {
                return new string[0];
            }

            IsBusy = true;
            var paths = new List<string>(entriesArray.Count());
            BusyMessage = "Extracting files...";

            int extractCount = 0;
            try
            {
                foreach (var entry in entriesArray)
                {
                    this.BusyMessage = "Extracting " + entry.Name + "...";

                    try
                    {
                        paths.Add(await ExtractEntryAsync(entry));
                        extractCount++;
                    }
                    catch (Exception e)
                    {
                        InteractionService.ShowMessage(MessageType.Error, "Error while extracting", "Error while extracting '" + entry.Name + "':\n" + e.Message);
                    }
                }
            }
            finally
            {
                IsBusy = false;
            }

            StatusMessage = "Extracted " + extractCount + " entries...";

            return paths;
        }
    }
}
