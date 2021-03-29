// ============================================================================
//               CrazzzyAuction by CrazzzyPeter (c) 2021-2021 
// ============================================================================
//
// Contacts:
//   https://github.com/crazzzypeter
//   https://www.youtube.com/c/crazzzypeter
//   https://www.twitch.tv/crazzzypeter
//   instagram: @crazzzypeter
//   telegram: @crazzzypeter
//
// ============================================================================
// License: GNU GPL 2.0
// ============================================================================
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace CrazzzyAuction
{
    public class AuctionTimerModel : INotifyPropertyChanged
    {
        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private DispatcherTimer Timer;

        private long TicksEnd;

        private long SavedValue;

        private bool _IsRunning;

        private const int MaxValue = 60 * 60 * 1000;

        public bool IsRunning
        {
            get
            {
                return _IsRunning;
            }
        }

        public AuctionTimerModel()
        {
            Timer = new DispatcherTimer();
            Timer.Tick += Timer_Tick;
            Timer.Interval = new TimeSpan(100);
            _IsRunning = false;
        }

        private void Timer_Tick(object sender, object e)
        {
            NotifyPropertyChanged("Time");
        }

        public void Start()
        {
            if (_IsRunning)
                return;
            Timer.Start();
            _IsRunning = true;
            TicksEnd = Environment.TickCount + SavedValue;
            NotifyPropertyChanged("Time");
        }

        public void Stop()
        {
            if (!_IsRunning)
                return;
            Timer.Stop();
            _IsRunning = false;
            SavedValue = TicksEnd - Environment.TickCount;
            NotifyPropertyChanged("Time");
        }

        public TimeSpan Time
        {
            get
            {
                if (IsRunning)
                {
                    if (TicksEnd - Environment.TickCount < 0)
                    {
                        Stop();
                        SavedValue = 0;
                    }
                    return new TimeSpan((TicksEnd - Environment.TickCount) * 10000);
                }
                else
                {
                    return new TimeSpan(SavedValue * 10000);
                }
            }
            set
            {
                if (IsRunning)
                {
                    TicksEnd = Environment.TickCount + value.Ticks / 10000;

                    if (TicksEnd - Environment.TickCount < 0)
                        TicksEnd = Environment.TickCount;
                    if (TicksEnd - Environment.TickCount >= MaxValue)
                        TicksEnd = Environment.TickCount + MaxValue - 1;
                }
                else
                {
                    SavedValue = value.Ticks / 10000;
                    if (SavedValue < 0)
                        SavedValue = 0;
                    if (SavedValue >= MaxValue)
                        SavedValue = MaxValue - 1;
                }

                NotifyPropertyChanged();
            }
        }


        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
