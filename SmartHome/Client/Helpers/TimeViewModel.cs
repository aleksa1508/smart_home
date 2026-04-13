using System;
using System.ComponentModel;
using System.Windows.Threading;

namespace Client.Helpers
{
    public class TimeViewModel : INotifyPropertyChanged
    {
        private string currentTime;
        private string currentDate;
        private DispatcherTimer timer;
        public string CurrentTime
        {
            get { return currentTime; }
            set
            {
                currentTime = value;
                OnPropertyChanged(nameof(CurrentTime));
            }
        }
        public string CurrentDate
        {
            get { return currentDate; }
            set
            {
                currentDate = value;
                OnPropertyChanged(nameof(CurrentDate));
            }
        }

        public TimeViewModel()
        {
            CurrentTime = DateTime.Now.ToString("HH:mm");
            CurrentDate = DateTime.Now.ToString("dd MMMM yyyy");
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => { CurrentTime = DateTime.Now.ToShortTimeString(); CurrentDate = DateTime.Now.ToString("dd MMMM yyyy"); };
            timer.Start();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
