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
        public delegate void SendEventDelegate(byte[] data);
        public event SendEventDelegate? SendEvent = null;

        public TxTerminalManager()
        {
        }
        public void Send(byte[] data_src)
        {
            byte[] data;
            if (Token != TokenType.None)
            {
                byte[] token_data = Encoding.ASCII.GetBytes(TokenDict[Token]);
                data = new byte[data_src.Length + token_data.Length];
                Buffer.BlockCopy(data_src, 0, data, 0, data_src.Length);
                Buffer.BlockCopy(token_data, 0, data, data_src.Length, token_data.Length);
            }
            else
            {
                data = data_src;
            }
            Write(data);
            if (SendEvent != null)
            {
                SendEvent(data);
            }
        }
    }
}
