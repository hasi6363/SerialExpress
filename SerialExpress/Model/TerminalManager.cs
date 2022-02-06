using SerialExpress.Converter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialExpress.Model
{
    public class TerminalDataItem
    {
        public DateTime Time { get; }
        public byte[] Data { get; }
        public string Text { get; }
        public TerminalDataItem(byte[] data)
        {
            Time = DateTime.Now;
            Data = data;
            Text = BytesToStringConverter.Convert(data);
        }
        public override string ToString()
        {
            return "[" + Time.ToString("HH:mm:ss.fff") + "]\t" + Text;
        }
    }
    public class TerminalManager
    {
        /// <summary>
        /// When it receive data, the received data is wirtten to file and temp data.
        /// If there is new line char in temp data, add this data to data list.
        /// </summary>
        public Stream? BinFileStream;
        public Stream? TextFileStream;
        public Stream TempStream;
        public ObservableCollection<TerminalDataItem> DataList;
        public string Token { get; set; } = "\r\n";
        public int TokenIndex { get; set; } = 0;
        public TerminalManager()
        {
            DataList = new ObservableCollection<TerminalDataItem>();
            TempStream = new MemoryStream();
        }
        public void Write(byte[] data)
        {
            if (BinFileStream is not null)
            {
                BinFileStream.Write(data, 0, data.Length);
            }
            SeparateAndAdd(data, Token);
        }

        private void SeparateAndAdd(byte[] data, string token)
        {
            for (int i = 0; i < data.Length; i++)
            {
                TempStream.WriteByte(data[i]);
                if (data[i] == token[TokenIndex])
                {
                    TokenIndex++;
                }
                else
                {
                    TokenIndex = 0;
                }
                if (TokenIndex == token.Length)
                {
                    TokenIndex = 0;
                    byte[] item_data = new byte[TempStream.Length];
                    TempStream.Position = 0;
                    TempStream.Read(item_data, 0, item_data.Length);
                    AppendToDataList(item_data);
                    TempStream.SetLength(0);
                }
            }
        }
        private void AppendToDataList(byte[] data)
        {
            var item = new TerminalDataItem(data);
            DataList.Add(item);
            if (TextFileStream is not null)
            {
                byte[] item_text = Encoding.UTF8.GetBytes(item.ToString() + Environment.NewLine);
                TextFileStream.Write(item_text, 0, item_text.Length);
            }
        }
        public void OpenPortCallback(System.IO.Ports.SerialPort serial_port)
        {
            string open_port_message = serial_port.PortName + (serial_port.IsOpen ? " is opened" : " is closed");
            byte[] data = Encoding.UTF8.GetBytes(open_port_message);
            AppendToDataList(data);

            if(!serial_port.IsOpen)
            { 
                if (TextFileStream != null)
                {
                    TextFileStream.Close();
                }
                if(BinFileStream != null)
                {
                    BinFileStream.Close();
                }
            }
        }
    }
}
