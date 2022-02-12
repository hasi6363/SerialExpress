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
using System.Windows.Threading;

namespace SerialExpress.View
{
    /// <summary>
    /// CommandView.xaml の相互作用ロジック
    /// </summary>
    public partial class CommandView : UserControl
    {
        private static readonly int ClickTimerThreshold = 400;
        private static readonly int LongDoubleClickThreshold = 250;
        private MouseClickManager MouseClickManager;
        public CommandView()
        {
            InitializeComponent();
            MouseClickManager = new MouseClickManager(ClickTimerThreshold, this.Dispatcher, MouseClickedEvent)
            {
                MaxCount = 2
            };
        }

        private void MouseClickedEvent(object? sender, int click_count, TimeSpan last_elapsed_time)
        {
            if(sender != null)
            {
                switch(click_count)
                {
                    case 1:
                        {
                            if (sender is ListViewItem lvi)
                            {
                                var item = (CommandItem)lvi.Content;
                                if (item != null)
                                {
                                    item.CommandIsEditable = false;
                                    item.DescriptionIsEditable = false;
                                }
                            }
                            break;
                        }
                    case 2:
                        {
                            if (sender is ListViewItem lvi)
                            {
                                if (last_elapsed_time.TotalMilliseconds < LongDoubleClickThreshold)
                                {
                                    var item = (CommandItem)lvi.Content;
                                    if (item != null && !item.CommandIsEditable && !item.DescriptionIsEditable)
                                    {
                                        item.Send();
                                    }
                                }
                            }
                            if (sender is ContentControl cc && cc.DataContext is CommandItem ci)
                            {
                                if (last_elapsed_time.TotalMilliseconds > LongDoubleClickThreshold)
                                {
                                    if ((string)cc.Tag == "Command")
                                    {
                                        ci.CommandIsEditable = true;
                                    }
                                    else if ((string)cc.Tag == "Description")
                                    {
                                        ci.DescriptionIsEditable = true;
                                    }
                                }
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Send command by Enter key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        private void MouseClickEvent_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                MouseClickManager.UpdateClickCount(sender, e);
            }
        }

        /// <summary>
        /// Cancel select item by Clicking outside the area
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommandListView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender != null && sender is ListView)
            {
                var lv = (ListView)sender;
                lv.SelectedItem = null;
            }
        }
    }
}
