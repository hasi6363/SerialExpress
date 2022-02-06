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
        public CollectionViewSource ViewSource { get; }
        public RxTerminalManager()
        {
            ViewSource = new CollectionViewSource();
            ViewSource.Source = DataList;
        }
    }
}
