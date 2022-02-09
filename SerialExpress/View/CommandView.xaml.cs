using SerialExpress.Model;
using SerialExpress.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// CommandView.xaml の相互作用ロジック
    /// </summary>
    public partial class CommandView : UserControl
    {
        public CommandView()
        {
            InitializeComponent();
        }

        private void CommandListView_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key==Key.Enter)
            {
                var vm = DataContext as MainWindowViewModel;
                var lv = sender as ListView;
                if(vm != null && lv != null)
                {
                    CommandItem? item = lv.SelectedItem as CommandItem;
                    if (item != null)
                    {
                        vm.SendCommand.Execute(item.Command);
                    }
                }
            }
        }
    }
}
