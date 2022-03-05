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
        private static readonly int ClickTimerThreshold = 500;
        private MouseClickManager MouseClickManager;
        private CommandItem? FirstClickItem = null;
        private bool CancelSend = false;
        public CommandView()
        {
            InitializeComponent();
            MouseClickManager = new MouseClickManager(ClickTimerThreshold, this.Dispatcher, MouseClickedEvent)
            {
                MaxCount = 2
            };
        }
        private void SendItemCommand(CommandItem ci)
        {
            if (CancelSend == false)
            {
                ci.Send();
            }
            else
            {
                CancelSend = false;
            }
        }
        private void MouseClickedEvent(object? sender, object? parameter, int click_count, TimeSpan last_elapsed_time)
        {
            if(sender != null)
            {
                switch(click_count)
                {
                    case 1:
                        {
                            if (sender is TextBlock tb && tb.DataContext is CommandItem ci)
                            {
                                if (FirstClickItem == ci)
                                {
                                    if (tb.Name is "CommandTextBlock")
                                    {
                                        ci.CommandIsEditable = true;
                                    }
                                    else if (tb.Name is "DescriptionTextBlock")
                                    {
                                        ci.DescriptionIsEditable = true;
                                    }
                                }
                            }
                            break;
                        }
                    case 2:
                        {
                            if (sender is ListViewItem lvi)
                            {
                                var item = (CommandItem)lvi.Content;
                                if (item != null && !item.CommandIsEditable && !item.DescriptionIsEditable)
                                {
                                    SendItemCommand(item);
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
                        SendItemCommand(item);
                    }
                }
            }
        }

        private void ListViewItem_MouseClickEvent(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                object? param = null;
                if(sender is ListViewItem lvi)
                {
                    if(lvi.IsFocused == true)
                    {
                        FirstClickItem = lvi.DataContext as CommandItem;
                    }
                    else
                    {
                        FirstClickItem = null;
                    }
                }
                MouseClickManager.UpdateClickCount(sender, e, param);
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

        private void TextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(sender is TextBox tb)
            {
                tb.Focus();
            }
        }

        private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox tb && tb.DataContext is CommandItem ci)
            {
                if (tb.Name == "CommandTextBox")
                {
                    ci.CommandIsEditable = false;
                }
                else if (tb.Name is "DescriptionTextBox")
                {
                    ci.DescriptionIsEditable = false;
                }
            }
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (sender is TextBox tb && tb.DataContext is CommandItem ci)
            {
                if (e.Key == Key.Enter || e.Key == Key.Escape)
                {
                    if (tb.Name == "CommandTextBox")
                    {
                        ci.CommandIsEditable = false;
                    }
                    else if (tb.Name is "DescriptionTextBox")
                    {
                        ci.DescriptionIsEditable = false;
                    }
                    if (e.Key == Key.Enter)
                    {
                        CancelSend = true;
                    }
                }
            }
        }
    }
}
