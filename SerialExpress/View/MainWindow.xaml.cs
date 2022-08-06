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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.ShowSerialPortOpenDialog(this);
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

        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }
        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
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
            if (DataContext is MainWindowViewModel vm)
            {
                vm.SaveAs();
            }
        }

        private void SaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }
        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.StoreConfigurations();
            }
            this.Close();
        }

        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(sender is MainWindow mw)
            {
                var sb = new StringBuilder(0x1000);
                ListView lv;
                if(mw.TerminalView.TxTerminalListView.IsKeyboardFocusWithin)
                {
                    lv = mw.TerminalView.TxTerminalListView;
                }
                else if(mw.TerminalView.RxTerminalListView.IsKeyboardFocusWithin)
                {
                    lv = mw.TerminalView.RxTerminalListView;
                }
                else if(mw.CommandView.CommandListView.IsKeyboardFocusWithin)
                {
                    lv = mw.CommandView.CommandListView;
                }
                else
                {
                    return;
                }

                var list = new List<object>();
                foreach (object item in lv.SelectedItems)
                {
                    list.Add(item);
                }
                //ist = list.OrderBy(c => c.Time).ToList();
                foreach (object item in list)
                {
                    sb.AppendLine(item.ToString());
                }

                if (sb.Length != 0)
                {
                    Clipboard.SetText(sb.ToString());
                }
            }
        }

        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.Save();
            }
        }
    }
}
