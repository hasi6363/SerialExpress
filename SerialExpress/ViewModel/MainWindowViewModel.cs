using SerialExpress.Converter;
using SerialExpress.Model;
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
        public RxTerminalManager RxTerminalManager { get; }
        public TxTerminalManager TxTerminalManager { get; }
        public SerialPortManager SerialPortManager { get; }
        public CommandHistory History { get; }

        public DelegateCommand SendCommand { get; }
        public DelegateCommand GetPrevCommand { get; }
        public DelegateCommand GetNextCommand { get; }
        public string InputText
        {
            get { return History.Current; }
            set
            {
                History.Current = value;
                RaisePropertyChanged();
            }
        }
        public bool SerialPortIsOpen
        {
            get { return SerialPortManager.SerialPort.IsOpen; }
        }
        private DispatcherTimer m_DataReceivedTimer;

        public MainWindowViewModel()
        {
            RxTerminalManager = new RxTerminalManager();
            TxTerminalManager = new TxTerminalManager();
            SerialPortManager = new SerialPortManager();
            History = new CommandHistory();

            SerialPortManager.PortStatusChangedCallback += SerialPort_PortStatusChanged;
            SerialPortManager.PortStatusChangedCallback += RxTerminalManager.OpenPortCallback;
            SerialPortManager.PortStatusChangedCallback += TxTerminalManager.OpenPortCallback;
            SerialPortManager.SerialPort.DataReceived += SerialPort_DataReceived;
            TxTerminalManager.SendEvent += SerialPortManager.Send;

            m_DataReceivedTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            m_DataReceivedTimer.Tick += DataReceivedTimer_Tick;
            RxTerminalManager.Write(new byte[] { 0x30, 0x31, 0x32, 0x33, 0x0d, 0x0a, 0x34, 0x35, 0x0d, 0x0a });

            SendCommand = new DelegateCommand(
                (object parameter) =>
                {
                    if (!(parameter is string)) throw new ArgumentException();
                    string? command = parameter as string;
                    if(command is null) throw new ArgumentException();
                    byte[] data_src = StringToBytesConverter.Convert(command);
                    byte[]? new_line = null;
                    byte[]? data = null;
                    new_line = new byte[] { 0x0d, 0x0a };
                    if (new_line != null)
                    {
                        data = new byte[data_src.Length + new_line.Length];
                        Buffer.BlockCopy(data_src, 0, data, 0, data_src.Length);
                        Buffer.BlockCopy(new_line, 0, data, data_src.Length, new_line.Length);
                    }
                    else
                    {
                        data = data_src;
                    }
                    TxTerminalManager.Send(data);
                    InputText = History.Next();
                },
                () =>
                {
                    return true;
                });
            GetPrevCommand = new DelegateCommand(
                (object parameter) =>
                {
                    InputText = History.Prev();
                },
                () =>
                {
                    return true;
                });
            GetNextCommand = new DelegateCommand(
                (object parameter) =>
                {
                    InputText = History.Next();
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
                RxTerminalManager.BinFileStream = new FileStream(log_dir + Path.DirectorySeparatorChar + WakeupTime.ToString("yyyyMMdd-HHmmss_") + port_name + Properties.Resources.TxDataBinFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                RxTerminalManager.TextFileStream = new FileStream(log_dir + Path.DirectorySeparatorChar + WakeupTime.ToString("yyyyMMdd-HHmmss_") + port_name + Properties.Resources.TxDataTextFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                TxTerminalManager.BinFileStream = new FileStream(log_dir + Path.DirectorySeparatorChar + WakeupTime.ToString("yyyyMMdd-HHmmss_") + port_name + Properties.Resources.RxDataBinFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                TxTerminalManager.TextFileStream = new FileStream(log_dir + Path.DirectorySeparatorChar + WakeupTime.ToString("yyyyMMdd-HHmmss_") + port_name + Properties.Resources.RxDataTextFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            }
            RaisePropertyChanged("SerialPortIsOpen");
        }
    }
}
