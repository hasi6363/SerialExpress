using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace SerialExpress.View
{
    /// <summary>
    /// Manage MouseClick event
    /// How to use:
    /// 1. Create new MouseClickManager instance.
    /// 2. Register event to MouseClickedEvent for handling click event
    /// 3. When click event is happened, call UpdateCount function
    ///    then crate MouseClickObject if sender is different and start timer
    /// 4. if timer is finished, MouseClickedEvent is called with sender object and click count.
    /// </summary>
    public class MouseClickManager
    {
        public class MouseClickObject
        {
            private readonly DispatcherTimer Timer;
            private readonly Stopwatch mStopWatch;
            public TimeSpan LastElapsedTime { get; private set; }
            public uint MaxCount { get; set; } = 0;
            private int ClickCount;
            public object SenderObject { get; }
            public object? Parameter { get; set; }
            private readonly MouseClickedEventDelegate mTimerTickEvent;
            public MouseClickObject(object sender, TimeSpan interval, MouseClickedEventDelegate event_handler, Dispatcher dispatcher)
            {
                SenderObject = sender;
                mTimerTickEvent += event_handler;
                Timer = new DispatcherTimer(interval, DispatcherPriority.Normal, Timer_Tick, dispatcher)
                {
                    IsEnabled = false
                };
                mStopWatch = new Stopwatch();
            }
            public void UpdateClickCount()
            {
                if (Timer.IsEnabled == false)
                {
                    Timer.Start();
                    mStopWatch.Restart();
                }
                else
                {
                    LastElapsedTime = mStopWatch.Elapsed;
                }
                ClickCount++;
                if(MaxCount == ClickCount)
                {
                    Timer_Tick(null, new EventArgs());
                }
            }
            private void Timer_Tick(object? sender, EventArgs e)
            {
                Timer.Stop();
                mTimerTickEvent?.Invoke(SenderObject, Parameter, ClickCount, LastElapsedTime);
                mStopWatch.Stop();
                ClickCount = 0;
            }
        }
        public List<MouseClickObject> MouseClickObjectList { get; }
        public TimeSpan Interval { get; }
        public Dispatcher Dispatcher { get; }
        private uint mMaxCount;
        public uint MaxCount
        {
            get
            {
                return mMaxCount;
            }
            set
            {
                mMaxCount = value;
                foreach (var obj in MouseClickObjectList)
                {
                    obj.MaxCount = mMaxCount;
                }
            }
        }
        public delegate void MouseClickedEventDelegate(object? sender, object? param, int click_count, TimeSpan last_elapsed_time);
        public event MouseClickedEventDelegate MouseClickedEvent;

        public MouseClickManager(int millisecond, Dispatcher dispatcher, MouseClickedEventDelegate mouse_clicked_event)
        {
            MouseClickObjectList = new List<MouseClickObject>();
            Interval = TimeSpan.FromMilliseconds(millisecond);
            Dispatcher = dispatcher;
            MouseClickedEvent = mouse_clicked_event;
        }
        public void UpdateClickCount(object sender, MouseButtonEventArgs e, object? parameter)
        {
            var obj = MouseClickObjectList.Find((MouseClickObject obj) => obj.SenderObject == sender);
            if (obj == null)
            {
                obj = new MouseClickObject(sender, Interval, MouseClickedEvent, Dispatcher)
                {
                    MaxCount = mMaxCount
                };
                MouseClickObjectList.Add(obj);
                obj.Parameter = parameter;
                obj.UpdateClickCount();
            }
            else
            {
                obj.Parameter = parameter;
                obj.UpdateClickCount();
            }
        }
    }
}
