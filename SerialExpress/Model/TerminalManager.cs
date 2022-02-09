using SerialExpress.Converter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SerialExpress.Model
{
    public class TerminalDataItem
    {
        public DateTime Time { get; }
        public byte[] Data { get; }
        public string Text { get; }
        public bool IsMessage { get; }
        public TerminalDataItem(byte[] data)
        {
            Time = DateTime.Now;
            Data = data;
            Text = BytesToStringConverter.Convert(data);
            IsMessage = false;
        }
        public TerminalDataItem(string message)
        {
            Time = DateTime.Now;
            Text = message;
            Data = new byte[0];
            IsMessage = true;
        }
        public override string ToString()
        {
            return "[" + Time.ToString("HH:mm:ss.fff") + "]\t" + Text;
        }
    }
    public class TerminalManager:BindableBase
    {
        /// <summary>
        /// When it receive data, the received data is wirtten to file and temp data.
        /// If there is new line char in temp data, add this data to data list.
        /// </summary>
        public enum TokenType
        {
            None,
            CR,
            LF,
            CRLF
        }
        public Dictionary<TokenType, string> TokenDict { get; }
        public FileStream? BinFileStream { get; set; }
        public FileStream? TextFileStream { get; set; }
        public MemoryStream TempStream { get; }
        public ObservableCollection<TerminalDataItem> DataList { get; }
        public TokenType Token { get; set; } = TokenType.CRLF;
        public ObservableCollection<TokenType> TokenTypeList { get; }
        public int TokenIndex { get; set; } = 0;
        public CollectionViewSource ViewSource { get; }

        public TerminalManager()
        {
            DataList = new ObservableCollection<TerminalDataItem>();
            TokenTypeList = new ObservableCollection<TokenType>();
            TempStream = new MemoryStream();

            ViewSource = new CollectionViewSource();
            ViewSource.Source = DataList;

            TokenDict = new Dictionary<TokenType, string>()
            {
                { TokenType.None, "" },
                { TokenType.CR, "\r" },
                { TokenType.LF, "\n" },
                { TokenType.CRLF, "\r\n" }
            };
            foreach (TokenType e in Enum.GetValues(typeof(TokenType)))
            {
                TokenTypeList.Add(e);
            }
            RaisePropertyChanged("TokenTypeList");
        }
        public void Write(byte[] data)
        {
            string token = TokenDict[Token];
            for (int i = 0; i < data.Length; i++)
            {
                TempStream.WriteByte(data[i]);
                if (token.Length != 0)
                {
                    if (data[i] == token[TokenIndex])
                    {
                        TokenIndex++;
                    }
                    else
                    {
                        TokenIndex = 0;
                    }
                }
                else
                {
                    TokenIndex = 0;
                }
                if (TokenIndex == token.Length)
                {
                    TokenIndex = 0;
                    AppendToDataList(new TerminalDataItem(TempStream.ToArray()));
                    TempStream.SetLength(0);
                }
            }
        }

        private void AppendToDataList(TerminalDataItem item)
        {
            DataList.Add(item);
            if (BinFileStream is not null)
            {
                var bw = new BinaryWriter(BinFileStream);
                bw.Write(item.Data);
                bw.Flush();
            }
            if (TextFileStream is not null)
            {
                var sw = new StreamWriter(TextFileStream);
                sw.WriteLine(item.ToString());
                sw.Flush();
            }
            RaisePropertyChanged("DataList");
        }
        public void PortStatusChanged(System.IO.Ports.SerialPort serial_port)
        {
            string open_port_message = serial_port.PortName + (serial_port.IsOpen ? " is opened" : " is closed");
            AppendToDataList(new TerminalDataItem(open_port_message));

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
        public void Clear()
        {
            DataList.Clear();
            RaisePropertyChanged("DataList");
        }
    }
}
