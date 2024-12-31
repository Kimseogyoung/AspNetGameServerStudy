namespace WebStudyServer.Helper
{
    public class TimeHelper
    {
        public static long DateTimeToTimeStamp(DateTime value) => ((DateTimeOffset)value).ToUnixTimeMilliseconds();

        public static DateTime TimeStampToDateTime(long value) => s_timestampBaseTime.AddMilliseconds(value).ToUniversalTime();

        private static readonly DateTime s_timestampBaseTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    }
}
