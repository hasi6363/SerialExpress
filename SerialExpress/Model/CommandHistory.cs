using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialExpress.Model
{
    public class CommandHistory
    {
        private List<string> Buffer { get; }
        public string Current
        {
            get
            {
                UpdateBufferAndIndex();
                return Buffer[mIndex];
            }
            set
            {
                UpdateBufferAndIndex();
                Buffer[mIndex] = value;
            }
        }
        private int mIndex = 0;
        public CommandHistory()
        {
            Buffer = new List<string>();
        }
        public void Clear()
        {
            Buffer.Clear();
        }
        public string Next()
        {
            if (mIndex < Buffer.Count)
            {
                mIndex++;
            }
            return Current;
        }
        public string Prev()
        {
            if (mIndex > 0)
            {
                mIndex--;
            }
            return Current;
        }
        private void UpdateBufferAndIndex()
        {
            // if current position is out of range, add new buffer and it is current.
            if (mIndex == Buffer.Count)
            {
                if (Buffer.Count == 0 || Buffer[^1] != "")
                {
                    Buffer.Add("");
                }
                else
                {
                    // if top of buffer is empty, not to add new buffer.
                    mIndex = Buffer.Count - 1;
                }
            }
            else if (mIndex > Buffer.Count)
            {
                throw new ArgumentException();
            }
        }
        public void Save()
        {
            using var sw = new StreamWriter(Properties.Resources.CommandHistoryFileName, false, Encoding.UTF8);
            foreach(var b in Buffer)
            {
                sw.WriteLine(b);
            }
            sw.Flush();
        }
        public void Load()
        {
            Clear();
            using var sr = new StreamReader(new FileStream(Properties.Resources.CommandHistoryFileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read), Encoding.UTF8);
            if (sr != null)
            {
                string? s;
                do
                {
                    s = sr.ReadLine();
                    if (s != null)
                    {
                        Buffer.Add((string)s);
                    }
                } while (s == null);
            }
        }
    }
}
