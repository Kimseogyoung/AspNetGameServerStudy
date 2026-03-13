using System.Xml.Linq;

namespace WebStudyServer.Helper
{
    public class ServerTime
    {
        public DateTime UtcNow
        {
            get
            {
                var dateTime = DateTime.UtcNow;

                if (_addHours != 0)
                {
                    dateTime = dateTime.AddHours(_addHours);
                }

                if (_addMinutes != 0)
                {
                    dateTime = dateTime.AddMinutes(_addMinutes);
                }

                if (_addSeconds != 0)
                {
                    dateTime = dateTime.AddSeconds(_addSeconds);
                }

                return dateTime;
            }
        }

        public void SetTime(DateTime time)
        {
            _ = UtcNow;
            var timeSpan = time - DateTime.UtcNow;
            _addHours = (int)timeSpan.TotalHours;
            _addMinutes = timeSpan.Minutes;
            _addSeconds = timeSpan.Seconds;
        }

        public void SetHour(int hours)
        {
            _addHours = hours;
        }

        public void SetMinutes(int minutes)
        {
            _addMinutes = minutes;
        }

        public void SetSeconds(int seconds)
        {
            _addSeconds = seconds;
        }

        public static ServerTime Instance
        {
            get
            {
                _instance ??= new ServerTime();
                return _instance;
            }
        }

        private static ServerTime? _instance;
        private int _addHours;
        private int _addMinutes;
        private int _addSeconds;
    }
}
