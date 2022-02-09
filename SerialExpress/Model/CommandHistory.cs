using System;
using System.Collections.Generic;
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
                if (Buffer.Count == 0 || Buffer[Buffer.Count - 1] != "")
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
    }
}
