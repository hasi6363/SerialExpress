using SerialExpress.Model;
using SerialExpress.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var ver = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            this.Title = ver.ProductName + " [" + ver.ProductVersion + "]";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as MainWindowViewModel;
            if (vm != null)
            {
                vm.ShowSerialPortOpenDialog();
            }
        }
        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            if (assembly != null)
            {
                string dllPath = assembly.Location;
                var appPath = System.IO.Path.GetFileNameWithoutExtension(dllPath) + ".exe";
                Process.Start(appPath);
            }
        }

        private void New_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = DataContext as MainWindowViewModel;
            if (vm != null)
            {
                vm.ShowSerialPortOpenDialog();
            }
        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = DataContext as MainWindowViewModel;
            if(vm!= null)
            {
                vm.Save();
            }
        }

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }
        private void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = DataContext as MainWindowViewModel;
            if (vm != null)
            {
                vm.SaveAs();
            }
        }

        private void SaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder(0x1000);
            var view = this.FindName("TerminalView") as UserControl;
            if (view != null)
            {
                var tx_lv = view.FindName("TxTerminalListView") as ListView;
                var rx_lv = view.FindName("RxTerminalListView") as ListView;
                var vm = DataContext as MainWindowViewModel;

                if (tx_lv != null && tx_lv.IsKeyboardFocusWithin)
                {
                    var list = new List<TerminalDataItem>();
                    foreach (TerminalDataItem item in tx_lv.SelectedItems)
                    {
                        list.Add(item);
                    }
                    list = list.OrderBy(c => c.Time).ToList();
                    foreach (TerminalDataItem item in list)
                    {
                        sb.AppendLine(item.ToString());
                    }
                }
                else if (rx_lv != null && rx_lv.IsKeyboardFocusWithin)
                {
                    var list = new List<TerminalDataItem>();
                    foreach (TerminalDataItem item in rx_lv.SelectedItems)
                    {
                        list.Add(item);
                    }
                    list = list.OrderBy(c => c.Time).ToList();
                    foreach (TerminalDataItem item in list)
                    {
                        sb.AppendLine(item.ToString());
                    }
                }
            }
            if (sb.Length != 0)
            {
                Clipboard.SetText(sb.ToString());
            }
        }

        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

    }
}
