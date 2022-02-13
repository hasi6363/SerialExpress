using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SerialExpress.Model
{
    public class RxTerminalManager: TerminalManager
    {
        public RxTerminalManager()
        {
        }
        [JsonIgnore]
        public byte[] TempData
        {
            get
            {
                return TempStream.ToArray();
            }
        }
    }
}
