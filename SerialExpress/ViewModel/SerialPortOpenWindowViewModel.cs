using SerialExpress.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialExpress.ViewModel
{
    public class SerialPortOpenWindowViewModel:BindableBase
    {
        public SerialPortManager SerialPortManager { get; }
        public TerminalManager TxTerminalManager { get; }
        public TerminalManager RxTerminalManager { get; }
        public SerialPortOpenWindowViewModel(SerialPortManager serial_port_manager, TerminalManager tx_term_manager, TerminalManager rx_term_manager)
        {
            SerialPortManager = serial_port_manager;
            TxTerminalManager = tx_term_manager;
            RxTerminalManager = rx_term_manager;
        }
    }
}
