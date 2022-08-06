using SerialExpress.Model;
using SerialExpress.ViewModel;
using System;
using System.Collections.Generic;
using System.IO.Ports;
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
using System.Windows.Shapes;

namespace SerialExpress.View
{
    /// <summary>
    /// SerialPortOpenWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SerialPortOpenWindow : Window
    {
        public static RoutedCommand OpenClosePortCommand { get; } = new RoutedCommand();
        public SerialPortOpenWindow(SerialPortManager serial_port_manager, TxTerminalManager tx_term_manager, RxTerminalManager rx_term_manager)
        {
            InitializeComponent();
            this.DataContext = new SerialPortOpenWindowViewModel(serial_port_manager, tx_term_manager, rx_term_manager);
            
            CommandBindings.Add(new CommandBinding(OpenClosePortCommand,
                (s, e) =>
                {
                    if (DataContext is SerialPortOpenWindowViewModel vm)
                    {
                        vm.SerialPortManager.OpenClosePortCommand.Execute(vm.SerialPortManager);
                        if (vm.SerialPortManager.IsOpened)
                        {
                            this.DialogResult = true;
                            this.Close();
                            return;
                        }
                    }
                },
                (s, e) =>
                {
                    e.CanExecute = true;
                }));
        }

        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
    }
}
