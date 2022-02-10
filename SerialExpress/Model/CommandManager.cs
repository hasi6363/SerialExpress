using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SerialExpress.Model
{
    public class FileTreeItem
    {
        public FileInfo Info { get; }
        public ObservableCollection<FileTreeItem>? Children
        {
            get
            {
                if(IsDirectory)
                {
                    try
                    {
                        return GetDirectoryAndFiles(Info.FullName);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }

            }
        }
        public string Name
        {
            get { return Info.Name; }
        }
        public string Path
        {
            get { return Info.FullName; }
        }
        public bool IsDirectory
        {
            get
            {
                return Info.Attributes.HasFlag(FileAttributes.Directory); 
            }
        }
        public FileTreeItem(string path)
        {
            Info = new FileInfo(path);
        }
        public static ObservableCollection<FileTreeItem> GetDirectoryAndFiles(string path)
        {
            var dirInfo = new DirectoryInfo(path);
            var list = new List<FileTreeItem>();
            list.AddRange(dirInfo.EnumerateDirectories().Select(s => new FileTreeItem(s.FullName)));
            list.AddRange(dirInfo.EnumerateFiles("*.txt").Select(s => new FileTreeItem(s.FullName)));
            var collection = new ObservableCollection<FileTreeItem>(list);
            return collection;
        }
    }
    public class CommandItem
    {
        public int Index { get; }
        public string Command { get; }
        public string Description { get; }
        public CommandItem(int index, string command, string description)
        {
            Index = index;
            Command = command;
            Description = description;
        }
        public override string ToString()
        {
            return Command;
        }
    }
    public class CommandManager : BindableBase
    {
        public ObservableCollection<FileTreeItem>? CommandFileTreeRoot { get; private set; } = null;
        public ObservableCollection<CommandItem> CommandList { get;private set; }
        private FileSystemWatcher mFileSystemWatcher;
        public bool UseCommandPrefix { get; set; } = false;
        public string CommandPrefix { get; set; } = "";
        public bool UseCommandSuffix { get; set; } = false;
        public string CommandSuffix { get; set; } = "";

        public delegate void SendCommandEventDelegate(string command);
        public event SendCommandEventDelegate? SendCommandEvent = null;
        public DelegateCommand SelectedItemChanged { get; }
        public DelegateCommand EditFile { get; }
        public DelegateCommand SendCommand { get; }
        public CommandManager()
        {
            if (!Directory.Exists(Properties.Resources.CommandDirName))
            {
                Directory.CreateDirectory(Properties.Resources.CommandDirName);
            }
            RefreshComamndFileTree();
            CommandList = new ObservableCollection<CommandItem>();
            BindingOperations.EnableCollectionSynchronization(CommandList, new object());

            SelectedItemChanged = new DelegateCommand(
                (object? parameter) =>
                {
                    if (!(parameter is FileTreeItem)) return;
                    var item = parameter as FileTreeItem;
                    if(item != null)
                    {
                        LoadCommand(item.Path);
                    }
                },
                () =>
                {
                    return true;
                });
            EditFile = new DelegateCommand(
                (object? parameter) =>
                {
                    FileTreeItem? item = null;
                    if(parameter is null)
                    {
                        // if parameter is null, open command directory
                        var proc = new System.Diagnostics.Process();
                        proc.StartInfo.FileName = Properties.Resources.CommandDirName;
                        proc.StartInfo.UseShellExecute = true;
                        proc.Start();
                    }
                    else
                    {
                        if (!(parameter is FileTreeItem)) return;
                        item = parameter as FileTreeItem;
                    }

                    if (item != null)
                    {
                        try
                        {
                            if (item.IsDirectory == false)
                            {
                                var proc = new System.Diagnostics.Process();
                                proc.StartInfo.FileName = item.Path;
                                proc.StartInfo.UseShellExecute = true;
                                proc.Start();
                            }
                        }
                        catch { }
                    }
                },
                () =>
                {
                    return true;
                });
            SendCommand = new DelegateCommand(
                (object? parameter) =>
                {
                    if (parameter is not CommandItem) return;
                    var item = parameter as CommandItem;
                    if (item != null)
                    {
                        if(SendCommandEvent != null)
                        {
                            SendCommandEvent((UseCommandPrefix ? CommandPrefix : "") + item.Command + (UseCommandSuffix ? CommandSuffix : ""));
                        }
                    }
                },
                () =>
                {
                    return true;
                });

            mFileSystemWatcher = new FileSystemWatcher();
            mFileSystemWatcher.Path = Properties.Resources.CommandDirName;
            mFileSystemWatcher.NotifyFilter = NotifyFilters.LastAccess
                                            | NotifyFilters.LastWrite
                                            | NotifyFilters.FileName
                                            | NotifyFilters.DirectoryName;
            mFileSystemWatcher.Filter = "*"; 
            mFileSystemWatcher.Changed += new FileSystemEventHandler(FileSystemWatcher_Changed);
            mFileSystemWatcher.Created += new FileSystemEventHandler(FileSystemWatcher_Changed);
            mFileSystemWatcher.Deleted += new FileSystemEventHandler(FileSystemWatcher_Changed);
            mFileSystemWatcher.Renamed += new RenamedEventHandler(FileSystemWatcher_Renamed);
            mFileSystemWatcher.IncludeSubdirectories = true;
            mFileSystemWatcher.EnableRaisingEvents = true;
        }
        private void FileSystemWatcher_Changed(object source, FileSystemEventArgs e)
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Changed:
                    Console.WriteLine(e.FullPath +" is changed.");
                    LoadCommand(e.FullPath);
                    break;
                case WatcherChangeTypes.Created:
                    Console.WriteLine(e.FullPath + " is created.");
                    RefreshComamndFileTree();
                    break;
                case WatcherChangeTypes.Deleted:
                    Console.WriteLine(e.FullPath + " is deleted.");
                    RefreshComamndFileTree();
                    break;
            }
        }
        private void FileSystemWatcher_Renamed(object source, RenamedEventArgs e)
        {
            Console.WriteLine(e.FullPath + " is renamed.");
            RefreshComamndFileTree();
        }
        private void RefreshComamndFileTree()
        {
            CommandFileTreeRoot = FileTreeItem.GetDirectoryAndFiles(Properties.Resources.CommandDirName);
            RaisePropertyChanged("CommandFileTreeRoot");
        }
        private void LoadCommand(string path)
        {
            CommandList.Clear();
            try
            {
                using (var sr = new StreamReader(path, Encoding.UTF8))
                {
                    while (!sr.EndOfStream)
                    {
                        var text = sr.ReadLine()?.Split('\t');
                        if (text != null)
                        {
                            if (text.Length > 1)
                            {
                                CommandList.Add(new CommandItem(CommandList.Count, text[0], text[1]));
                            }
                            else
                            {
                                CommandList.Add(new CommandItem(CommandList.Count, text[0], ""));
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            RaisePropertyChanged("CommandList");
        }
    }
}
