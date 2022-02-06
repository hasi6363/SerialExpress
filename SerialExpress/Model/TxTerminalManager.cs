using SerialExpress.Converter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SerialExpress.Model
{
    public class TxTerminalManager: TerminalManager
    {
        public CollectionViewSource ViewSource { get; }
        public delegate void SendEventDelegate(byte[] data);
        public event SendEventDelegate? SendEvent = null;

        public TxTerminalManager()
        {
            ViewSource = new CollectionViewSource();
            ViewSource.Source = DataList;
        }
        public void Send(byte[] data)
        {
            Write(data);
            if (SendEvent != null)
            {
                SendEvent(data);
            }
        }
    }
}
