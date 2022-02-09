using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SerialExpress.View
{
    /// <summary>
    /// TerminalUserControl.xaml の相互作用ロジック
    /// </summary>
    public partial class TerminalView : UserControl
    {
        public TerminalView()
        {
            InitializeComponent();
        }
        private void TerminalListView_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            var lv = sender as ListView;
            if (lv != null && lv.Items.Count != 0)
            {
                if (lv.SelectedIndex >= lv.Items.Count - 3)
                {
                    lv.SelectedIndex = lv.Items.Count - 1;
                    lv.ScrollIntoView(lv.Items[lv.Items.Count - 1]);
                }
            }
        }
    }
}
