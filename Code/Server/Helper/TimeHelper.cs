namespace WebStudyServer.Helper
{
    public class TimeHelper
    {
        public static long DateTimeToTimeStamp(DateTime value) => ((DateTimeOffset)value).ToUnixTimeMilliseconds();

        public static DateTime TimeStampToDateTime(long value) => TimestampBaseTime.AddMilliseconds(value).ToUniversalTime();

        public static bool IsValidDateTime(DateTime nowTime, DateTime startTime, DateTime endTime)
        {
            if (startTime > nowTime || endTime < nowTime)
            {
                return false;
            }
            return true;
        }

        private static readonly DateTime TimestampBaseTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    }
}
