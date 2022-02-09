﻿using SerialExpress.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

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
        public ObservableCollection<PortNameType> PortNameList { get; }
        public ObservableCollection<int> BaudRateList { get; }
        public ObservableCollection<int> DataBitsList { get; }
        public ObservableCollection<Parity> ParityList { get; }
        public ObservableCollection<StopBits> StopBitsList { get; }
        public string PortName
        {
            get { return SerialPort.PortName; }
            set
            {
                if (value == null)
                {
                    RaisePropertyChanged();
                    return;
                }
                SerialPort.PortName = value;
                RaisePropertyChanged();
            }
        }
        public int BaudRate
        {
            get { return SerialPort.BaudRate; }
            set
            {
                SerialPort.BaudRate = value;
                RaisePropertyChanged();
            }
        }
        public int DataBits
        {
            get { return SerialPort.DataBits; }
            set
            {
                SerialPort.DataBits = value;
                RaisePropertyChanged();
            }
        }
        public Parity Parity
        {
            get { return SerialPort.Parity; }
            set
            {
                SerialPort.Parity = value;
                RaisePropertyChanged();
            }
        }
        public StopBits StopBits
        {
            get { return SerialPort.StopBits; }
            set
            {
                SerialPort.StopBits = value;
                RaisePropertyChanged();
            }
        }
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
        public bool IsOpened
        {
            get { return SerialPort.IsOpen; }
        }
        public bool IsClosed
        {
            get { return !SerialPort.IsOpen; }
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
            PortNameList = new ObservableCollection<PortNameType>();
            BaudRateList = new ObservableCollection<int>()
                {
                    110, 300, 600, 1200, 2400, 4800,
                    9600, 14400, 19200, 38400, 57600,
                    115200, 230400, 460800, 921600,
                    1000000, 2000000, 3000000
                };
            RaisePropertyChanged("BaudRateList");
            DataBitsList = new ObservableCollection<int>()
                {
                    7, 8
                };
            RaisePropertyChanged("DataBitsList");
            ParityList = new ObservableCollection<Parity>();
            foreach (Parity e in Enum.GetValues(typeof(Parity)))
            {
                ParityList.Add(e);
            }
            RaisePropertyChanged("ParityList");
            StopBitsList = new ObservableCollection<StopBits>();
            foreach (StopBits e in Enum.GetValues(typeof(StopBits)))
            {
                StopBitsList.Add(e);
            }
            RaisePropertyChanged("StopBitsList");

            SerialPort.BaudRate = 9600;
            SerialPort.DataBits = 8;
            SerialPort.Parity = Parity.None;
            SerialPort.StopBits = StopBits.One;
            SerialPort.ReadBufferSize = 0x4000;
            SerialPort.WriteBufferSize = 0x10;
            SerialPort.WriteTimeout = 1000;
            SerialPort.DtrEnable = true;
            OpenCommand = new DelegateCommand(
                (object? parameter) =>
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
                    catch 
                    {
                        SerialPort.Close();
                        NextAction = NextActionEnum.Open;
                    }

                    if (PortStatusChangedCallback != null)
                    {
                        PortStatusChangedCallback(SerialPort);
                    }
                    RaisePropertyChanged("IsOpened");
                    RaisePropertyChanged("IsClosed");
                },
                () =>
                {
                    return true;
                });
            UpdatePortNameListCommand = new DelegateCommand(
                (object? parameter) =>
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
        public override string ToString()
        {
            return $"PortName:\"{SelectedPortName}\" BaudRate:{BaudRate} DataBits:{DataBits} Parity:{Parity} StopBits:{StopBits}";
        }
    }
}