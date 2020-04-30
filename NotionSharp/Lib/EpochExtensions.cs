using System;

namespace NotionSharp.Lib
{
    public static class EpochExtensions
    {
        private static DateTimeOffset Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTimeOffset EpochToDateTimeOffset(this long milliseconds)
            => Epoch.AddMilliseconds(milliseconds);
 
        //var unixTime = ((Int32)Math.Floor(DateTime.UtcNow.Subtract(Epoch).TotalSeconds)).ToString();
    }
}