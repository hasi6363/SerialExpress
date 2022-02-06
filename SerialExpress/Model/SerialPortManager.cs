using SerialExpress.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SerialExpress.Model
{
    public class SerialPortManager:BindableBase
    {
        public enum NextActionEnum
        {
            Open,
            Close
        }
        public struct PortNameType
        {
            public string ComName { get; }
            public string DeviceName { get; }
            public PortNameType(string com_name, string device_name)
            {
                ComName = com_name;
                DeviceName = device_name;
            }
            public override string ToString()
            {
                return DeviceName;
            }
        }

        public SerialPort SerialPort { get; }
        public delegate void PortStatusChangedCallbackDelegete(SerialPort serial_port);
        public event PortStatusChangedCallbackDelegete? PortStatusChangedCallback = null;
        public ObservableCollection<PortNameType> PortNameList { get; } = new ObservableCollection<PortNameType>();
        private PortNameType mSelectedPortName;

        public PortNameType SelectedPortName
        {
            get { return mSelectedPortName; }
            set
            {
                mSelectedPortName = value;
                RaisePropertyChanged();
            }
        }

        private NextActionEnum mNextAction = NextActionEnum.Open;
        public NextActionEnum NextAction
        {
            get { return mNextAction; }
            set
            {
                mNextAction = value;
                RaisePropertyChanged();
                RaisePropertyChanged("IsOpen");
            }
        }

        public DelegateCommand OpenCommand { get; }
        public DelegateCommand UpdatePortNameListCommand { get; }
        public SerialPortManager()
        {
            SerialPort = new SerialPort();
            SerialPort.BaudRate = 9600;
            SerialPort.DataBits = 8;
            SerialPort.Parity = Parity.None;
            SerialPort.StopBits = StopBits.One;
            SerialPort.ReadBufferSize = 0x4000;
            SerialPort.WriteBufferSize = 0x10;
            SerialPort.WriteTimeout = 1000;
            SerialPort.DtrEnable = true;
            OpenCommand = new DelegateCommand(
                (object parameter) =>
                {
                    try
                    {
                        SerialPort.PortName = SelectedPortName.ComName;
                        if (!SerialPort.IsOpen)
                        {
                            SerialPort.Open();
                            NextAction = NextActionEnum.Close;
                        }
                        else
                        {
                            SerialPort.Close();
                            NextAction = NextActionEnum.Open;
                        }
                    }
                    catch (Exception ex)
                    {
                        SerialPort.Close();
                        NextAction = NextActionEnum.Open;
                    }

                    if (PortStatusChangedCallback != null)
                    {
                        PortStatusChangedCallback(SerialPort);
                    }
                },
                () =>
                {
                    return true;
                });
            UpdatePortNameListCommand = new DelegateCommand(
                (object parameter) =>
                {
                    PortNameType[] port_list = GetConnectedDeviceNames();
                    PortNameList.Clear();
                    foreach (PortNameType s in port_list)
                    {
                        PortNameList.Add(s);
                    }
                    RaisePropertyChanged("PortNameList");
                },
                () =>
                {
                    return true;
                });
        }
        public static PortNameType[] GetConnectedDeviceNames()
        {
            ManagementClass pnp_entry = new ManagementClass("Win32_PnPEntity");
            ManagementObjectCollection obj_list = pnp_entry.GetInstances();

            var dev_name_list = new List<PortNameType>();
            var check = new Regex("(COM[1-9][0-9]?[0-9]?)");

            foreach (ManagementObject obj in obj_list)
            {
                var name_property = obj.GetPropertyValue("Name");
                if (name_property != null)
                {
                    string? dev_name = name_property.ToString();
                    if (dev_name != null && check.IsMatch(dev_name))
                    {
                        string com_name = check.Match(dev_name).Value;
                        PortNameType port_name = new PortNameType(com_name, dev_name);
                        dev_name_list.Add(port_name);
                    }
                }
            }
            return dev_name_list.ToArray();
        }
        public void Send(byte[] data)
        {
            SerialPort.Write(data, 0, data.Length);
        }
    }
}
