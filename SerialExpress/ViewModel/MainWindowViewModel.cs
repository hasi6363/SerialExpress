using SerialExpress.Converter;
using SerialExpress.Model;
using SerialExpress.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace SerialExpress.ViewModel
{
    public class MainWindowViewModel : BindableBase
    { 
        public ConfigurationManager ConfigurationManager { get; }
        public RxTerminalManager RxTerminalManager { get; }
        public TxTerminalManager TxTerminalManager { get; }
        public SerialPortManager SerialPortManager { get; }
        public CommandManager CommandManager { get; }
        public CommandHistory History { get; }

        public DelegateCommand SendCommand { get; }
        public DelegateCommand GetPrevCommand { get; }
        public DelegateCommand GetNextCommand { get; }
        public DelegateCommand ClearTerminal { get; }
        public string InputText
        {
            get { return History.Current; }
            set
            {
                History.Current = value;
                RaisePropertyChanged();
            }
        }
        public string ReceivedTempData
        {
            get { return BytesToStringConverter.Convert(RxTerminalManager.TempData); }
        }
        public string StatusBarText
        {
            get
            {
                if (SerialPortManager.IsOpened)
                {
                    return $"{SerialPortManager} TxToken:{TxTerminalManager.Token} RxToken:{RxTerminalManager.Token}";
                }else
                {
                    return "";
                }
            }
        }
        public bool UseCommandPrefix
        {
            get
            {
                return CommandManager.UseCommandPrefix;
            }
            set
            {
                CommandManager.UseCommandPrefix = value;
                RaisePropertyChanged();
            }
        }
        public string CommandPrefix
        {
            get
            {
                return CommandManager.CommandPrefix;
            }
            set
            {
                CommandManager.CommandPrefix = value;
                RaisePropertyChanged();
            }
        }
        public bool UseCommandSuffix
        {
            get
            {
                return CommandManager.UseCommandSuffix;
            }
            set
            {
                CommandManager.UseCommandSuffix = value;
                RaisePropertyChanged();
            }
        }
        public string CommandSuffix
        {
            get
            {
                return CommandManager.CommandSuffix;
            }
            set
            {
                CommandManager.CommandSuffix = value;
                RaisePropertyChanged();
            }
        }

        private DispatcherTimer m_DataReceivedTimer;

        public MainWindowViewModel()
        {
            ConfigurationManager = new ConfigurationManager();
            RxTerminalManager = new RxTerminalManager();
            TxTerminalManager = new TxTerminalManager();
            SerialPortManager = new SerialPortManager();
            CommandManager = new CommandManager();
            History = new CommandHistory();

            SerialPortManager.PortStatusChangedCallback += SerialPort_PortStatusChanged;
            SerialPortManager.PortStatusChangedCallback += RxTerminalManager.PortStatusChanged;
            SerialPortManager.PortStatusChangedCallback += TxTerminalManager.PortStatusChanged;
            SerialPortManager.SerialPort.DataReceived += SerialPort_DataReceived;
            TxTerminalManager.SendEvent += SerialPortManager.Send;
            CommandManager.SendCommandEvent += SendCommandEvent;

            m_DataReceivedTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            m_DataReceivedTimer.Tick += DataReceivedTimer_Tick;

            SendCommand = new DelegateCommand(
                (object? parameter) =>
                {
                    if (!(parameter is string)) throw new ArgumentException();
                    string? command = parameter as string;
                    if(command is null) throw new ArgumentException();
                    InputText = command;
                    byte[] data_src = StringToBytesConverter.Convert(command);
                    TxTerminalManager.Send(data_src);
                    InputText = History.Next();
                },
                () =>
                {
                    return true;
                });
            GetPrevCommand = new DelegateCommand(
                (object? parameter) =>
                {
                    InputText = History.Prev();
                },
                () =>
                {
                    return true;
                });
            GetNextCommand = new DelegateCommand(
                (object? parameter) =>
                {
                    InputText = History.Next();
                },
                () =>
                {
                    return true;
                });
            ClearTerminal = new DelegateCommand(
                (object? parameter) =>
                {
                    TxTerminalManager.Clear();
                    RxTerminalManager.Clear();
                },
                () =>
                {
                    return true;
                });
        }

        private void DataReceivedTimer_Tick(object? sender, EventArgs e)
        {
            if (SerialPortManager.SerialPort.IsOpen)
            {
                byte[] data = new byte[SerialPortManager.SerialPort.BytesToRead];
                SerialPortManager.SerialPort.Read(data, 0, data.Length);
                RxTerminalManager.Write(data);
                RaisePropertyChanged("ReceivedTempData");
                m_DataReceivedTimer.IsEnabled = false;
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!m_DataReceivedTimer.IsEnabled)
            {
                m_DataReceivedTimer.Start();
            }
        }

        private void SerialPort_PortStatusChanged(SerialPort serial_port)
        {
            if (serial_port.IsOpen)
            {
                DateTime WakeupTime = DateTime.Now;
                string port_name = serial_port.PortName + "_";
                string log_dir = Properties.Resources.LogDirName;
                if (!Directory.Exists(log_dir))
                {
                    Directory.CreateDirectory(log_dir);
                }
                TxTerminalManager.BinFileStream = new FileStream(log_dir + Path.DirectorySeparatorChar + WakeupTime.ToString("yyyyMMdd-HHmmss_") + port_name + Properties.Resources.TxDataBinFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                TxTerminalManager.TextFileStream = new FileStream(log_dir + Path.DirectorySeparatorChar + WakeupTime.ToString("yyyyMMdd-HHmmss_") + port_name + Properties.Resources.TxDataTextFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                RxTerminalManager.BinFileStream = new FileStream(log_dir + Path.DirectorySeparatorChar + WakeupTime.ToString("yyyyMMdd-HHmmss_") + port_name + Properties.Resources.RxDataBinFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                RxTerminalManager.TextFileStream = new FileStream(log_dir + Path.DirectorySeparatorChar + WakeupTime.ToString("yyyyMMdd-HHmmss_") + port_name + Properties.Resources.RxDataTextFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            }
            RaisePropertyChanged("StatusBarText");
        }
        public void ShowSerialPortOpenDialog()
        {
            LoadConfigurations();
            var spw = new SerialPortOpenWindow(SerialPortManager, TxTerminalManager, RxTerminalManager);
            if (spw.ShowDialog() == true)
            {
                StoreConfigurations();
            }
        }
        private void LoadConfigurations()
        {
            var config = ConfigurationManager.Load();

            if (config != null)
            {
                try
                {
                    SerialPortManager.BaudRate = int.Parse(config.GetSection("SerialPort").GetSection("BaudRate").Value);
                    SerialPortManager.DataBits = int.Parse(config.GetSection("SerialPort").GetSection("DataBits").Value);
                    SerialPortManager.Parity = Enum.Parse<Parity>(config.GetSection("SerialPort").GetSection("Parity").Value);
                    SerialPortManager.StopBits = Enum.Parse<StopBits>(config.GetSection("SerialPort").GetSection("StopBits").Value);
                    TxTerminalManager.Token = Enum.Parse<TerminalManager.TokenType>(config.GetSection("TxTerminal").GetSection("Token").Value);
                    RxTerminalManager.Token = Enum.Parse<TerminalManager.TokenType>(config.GetSection("RxTerminal").GetSection("Token").Value);
                    CommandManager.UseCommandPrefix = bool.Parse(config.GetSection("Command").GetSection("UseCommandPrefix").Value);
                    CommandManager.CommandPrefix = config.GetSection("Command").GetSection("CommandPrefix").Value;
                    CommandManager.UseCommandSuffix = bool.Parse(config.GetSection("Command").GetSection("UseCommandSuffix").Value);
                    CommandManager.CommandSuffix = config.GetSection("Command").GetSection("CommandSuffix").Value;
                }
                catch
                {
                    StoreConfigurations();
                }
            }
        }
        public void StoreConfigurations()
        {
            var dict = new Dictionary<string, object>
            {
                ["SerialPort"] = SerialPortManager,
                ["TxTerminal"] = TxTerminalManager,
                ["RxTerminal"] = RxTerminalManager,
                ["Command"] = CommandManager
            };
            ConfigurationManager.Save(dict);
        }
        public void Save()
        {
            StoreConfigurations();
        }
        public void SaveAs()
        {

        }
        public void SendCommandEvent(string command)
        {
            if (command != null)
            {
                SendCommand.Execute(command);
            }
        }
    }
}
