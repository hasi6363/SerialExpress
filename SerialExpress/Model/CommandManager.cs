using Newtonsoft.Json;
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
    public class CommandItem: BindableBase
    {
        public delegate void SaveEventDelegate();
        public event SaveEventDelegate? SaveEvent = null;
        public delegate void SendCommandEventDelegate(CommandItem item);
        public event SendCommandEventDelegate? SendEvent = null;
        public delegate void AddCommandItemEventDelegate();
        public event AddCommandItemEventDelegate? AddEvent = null;
        public delegate void InsertCommandItemEventDelegate(CommandItem selected_item);
        public event InsertCommandItemEventDelegate? InsertEvent = null;
        public DelegateCommand EditCommand { get; }
        public DelegateCommand EditDescription { get; }
        public DelegateCommand AddCommandItem { get; }
        public DelegateCommand InsertCommandItem { get; }
        private int m_Index;
        public int Index
        {
            get { return m_Index; }
            set
            {
                m_Index = value;
                RaisePropertyChanged();
            }
        }
        private string m_Command;
        public string Command
        {
            get { return m_Command; }
            set
            {
                m_Command = value;
                RaisePropertyChanged();
                SaveEvent?.Invoke();
            }
        }
        private string m_Description;
        public string Description
        {
            get { return m_Description; }
            set
            {
                m_Description = value;
                RaisePropertyChanged();
                SaveEvent?.Invoke();
            }
        }
        private bool mCommandIsEditable = false;
        public bool CommandIsEditable
        {
            get { return mCommandIsEditable; }
            set
            {
                mCommandIsEditable = value;
                RaisePropertyChanged();
            }
        }
        private bool mDescriptionIsEditable = false;
        public bool DescriptionIsEditable
        {
            get { return mDescriptionIsEditable; }
            set
            {
                mDescriptionIsEditable = value;
                RaisePropertyChanged();
            }
        }
        public CommandItem(int index, string command, string description)
        {
            m_Index = index;
            m_Command = command;
            m_Description = description;
            EditCommand = new DelegateCommand(
                (object? parameter) =>
                {
                    CommandIsEditable = true;
                },
                () =>
                {
                    return true;
                });
            EditDescription = new DelegateCommand(
                (object? parameter) =>
                {
                    DescriptionIsEditable = true;
                },
                () =>
                {
                    return true;
                });
            AddCommandItem = new DelegateCommand(
                (object? parameter) =>
                {
                    AddEvent?.Invoke();
                },
                () =>
                {
                    return true;
                });
            InsertCommandItem = new DelegateCommand(
                (object? parameter) =>
                {
                    InsertEvent?.Invoke(this);
                },
                () =>
                {
                    return true;
                });
        }
        public override string ToString()
        {
            return Command;
        }
        public void Send()
        {
            SendEvent?.Invoke(this);
        }
    }
    public class CommandManager : BindableBase
    {
        [JsonIgnore]
        public ObservableCollection<FileTreeItem>? CommandFileTreeRoot { get; private set; } = null;
        [JsonIgnore]
        public ObservableCollection<CommandItem> CommandList { get;private set; }
        private FileSystemWatcher mFileSystemWatcher;
        private bool mUseCommandPrefix = false;
        private string mCommandPrefix = "";
        private bool mUseCommandSuffix = false;
        private string mCommandSuffix = "";
        public bool UseCommandPrefix
        {
            get { return mUseCommandPrefix; }
            set
            {
                mUseCommandPrefix = value;
                RaisePropertyChanged();
            }
        }
        public string CommandPrefix
        {
            get { return mCommandPrefix; }
            set
            {
                mCommandPrefix = value;
                RaisePropertyChanged();
            }
        }
        public bool UseCommandSuffix
        {
            get { return mUseCommandSuffix; }
            set
            {
                mUseCommandSuffix = value;
                RaisePropertyChanged();
            }
        }
        public string CommandSuffix
        {
            get { return mCommandSuffix; }
            set
            {
                mCommandSuffix = value;
                RaisePropertyChanged();
            }
        }
        [JsonIgnore]
        public FileTreeItem? SelectedFileTreeItem { get; set; } = null;
        private CommandItem? m_SelectedCommandListItem = null;
        [JsonIgnore]
        public CommandItem? SelectedCommandListItem
        {
            get { return m_SelectedCommandListItem; }
            set
            {
                m_SelectedCommandListItem = value;
                foreach(var cmd in CommandList)
                {
                    if (cmd.CommandIsEditable is true)
                    {
                        cmd.CommandIsEditable = false;
                    }
                    if (cmd.DescriptionIsEditable is true)
                    {
                        cmd.DescriptionIsEditable = false;
                    }
                }
                RaisePropertyChanged();
            }
        }

        public delegate void SendCommandEventDelegate(string command);
        public event SendCommandEventDelegate? SendCommandEvent = null;
        [JsonIgnore]
        public DelegateCommand SelectedFileTreeItemChanged { get; }
        [JsonIgnore]
        public DelegateCommand AddCommandItem { get; }
        [JsonIgnore]
        public DelegateCommand AddFile { get; }
        [JsonIgnore]
        public DelegateCommand EditFile { get; }
        [JsonIgnore]
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

            SelectedFileTreeItemChanged = new DelegateCommand(
                (object? parameter) =>
                {
                    if (parameter is not FileTreeItem) return;
                    SelectedFileTreeItem = parameter as FileTreeItem;
                    if(SelectedFileTreeItem != null)
                    {
                        LoadCommandList(SelectedFileTreeItem.Path);
                    }
                    RaisePropertyChanged(nameof(SelectedFileTreeItem));
                },
                () =>
                {
                    return true;
                });
            AddCommandItem = new DelegateCommand(
                (object? parameter) =>
                {
                    AddEmptyCommandItem();
                    RaisePropertyChanged(nameof(CommandList));
                },
                () =>
                {
                    return true;
                });
            AddFile = new DelegateCommand(
                (object? parameter) =>
                {
                    var title = DateTime.Now.ToString("yyyyMMss_HHmmss") + "_untitled.txt";
                    if (parameter is null)
                    {
                        if (SelectedFileTreeItem == null)
                        {
                            File.Create(Path.Combine(Properties.Resources.CommandDirName, title));
                        }
                        else
                        {
                            var dir = SelectedFileTreeItem.Info.Directory;
                            if (dir != null)
                            {
                                File.Create(Path.Combine(dir.FullName, title));
                            }
                        }
                    }
                    else
                    {
                        FileTreeItem? item = parameter as FileTreeItem;
                        if (item != null)
                        {
                            var dir = item.Info.Directory;
                            if (dir != null)
                            {
                                File.Create(Path.Combine(dir.FullName, title));
                            }
                        }
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
                        if (SelectedFileTreeItem == null)
                        {
                            proc.StartInfo.FileName = Properties.Resources.CommandDirName;
                        }
                        else
                        {
                            proc.StartInfo.FileName = SelectedFileTreeItem.Path;
                        }
                        proc.StartInfo.UseShellExecute = true;
                        proc.Start();
                    }
                    else
                    {
                        if (parameter is not FileTreeItem) return;
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
                        SendCommandFunc(item);
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
        private void SendCommandFunc(CommandItem item)
        {
            if (SendCommandEvent != null)
            {
                SendCommandEvent((UseCommandPrefix ? CommandPrefix : "") + item.Command + (UseCommandSuffix ? CommandSuffix : ""));
            }
        }
        private void FileSystemWatcher_Changed(object source, FileSystemEventArgs e)
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Changed:
                    Console.WriteLine(e.FullPath +" is changed.");
                    if (SelectedFileTreeItem != null && Path.GetFullPath(e.FullPath) == SelectedFileTreeItem.Path)
                    {
                        LoadCommandList(e.FullPath);
                    }
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
            RaisePropertyChanged(nameof(CommandFileTreeRoot));
        }
        private CommandItem CreateNewCommandItem(string command, string description)
        {
            var item = new CommandItem(CommandList.Count, command, description);
            item.SendEvent += SendCommandFunc;
            item.SaveEvent += SaveCommandList;
            item.AddEvent += AddEmptyCommandItem;
            item.InsertEvent += InsertCommandItem;
            return item;
        }
        private void AddEmptyCommandItem()
        {
            AddCommandItemFunc("", "");
        }
        private void AddCommandItemFunc(string command, string description)
        {
            CommandList.Add(CreateNewCommandItem(command, description));
        }
        private void LoadCommandList(string path)
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
                                AddCommandItemFunc(text[0], text[1]);
                            }
                            else
                            {
                                AddCommandItemFunc(text[0], "");
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            RaisePropertyChanged(nameof(CommandList));
        }
        private void SaveCommandList()
        {
            if(SelectedFileTreeItem != null)
            {
                using var sw = new StreamWriter(new FileStream(SelectedFileTreeItem.Path, FileMode.Open, FileAccess.Write, FileShare.Read));
                try
                {
                    foreach (var cmd in CommandList)
                    {
                        sw.Write($"{cmd.Command}\t{cmd.Description}\r\n");
                    }
                    sw.Flush();
                }
                catch (Exception e) 
                {
                    MessageBox.Show(e.Message, "Save Command Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void InsertCommandItem(CommandItem selected_item)
        {
            var new_item = CreateNewCommandItem("","");
            var index = CommandList.IndexOf(selected_item);
            CommandList.Insert(index, new_item);
            UpdateCommandIndex();
            SaveCommandList();
            RaisePropertyChanged(nameof(CommandList));
        }
        private void UpdateCommandIndex()
        {
            foreach(var cmd in CommandList)
            {
                cmd.Index = CommandList.IndexOf(cmd);
            }
        }
    }
}
