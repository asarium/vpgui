using System;
using System.Collections.Generic;

namespace VPGUI.Services
{
    public enum MessageType
    {
        Warning,
        Error,
        Information
    }

    public class FileFilter
    {
        public FileFilter(string description, string filter)
        {
            this.Description = description;
            this.Filter = filter;
        }

        public string Description { get; set; }

        public string Filter { get; set; }
    }

    public delegate void PathsSelectedDelegate(string[] paths);
    public delegate void PathSelectedDelegate(string path);

    public interface IInteractionService
    {
        void ShowMessage(MessageType type, string title, string text);

        void OpenFileDialog(string title, bool multiple, PathsSelectedDelegate selectedCallback, params FileFilter[] filters);

        void SaveFileDialog(string title, PathSelectedDelegate selectedCallback, params FileFilter[] filters);

        void SaveDirectoryDialog(string title, PathSelectedDelegate selectedCallback);
    }
}
