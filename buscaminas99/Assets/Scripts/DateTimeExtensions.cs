using System;

public static class DateTimeExtensions {

    private static DateTime UnixEpochDateTime = new DateTime(1970, 1, 1);
    private static long UnixEpochTicks = UnixEpochDateTime.Ticks;
    private static long UnixEpochSeconds = UnixEpochTicks / TimeSpan.TicksPerSecond; // 62,135,596,800
    public static long ToUnixTimeSeconds(this DateTime dateTime)
    {
        // Truncate sub-second precision before offsetting by the Unix Epoch to avoid
        // the last digit being off by one for dates that result in negative Unix times.
        //
        // For example, consider the DateTimeOffset 12/31/1969 12:59:59.001 +0
        //   ticks            = 621355967990010000
        //   ticksFromEpoch   = ticks - DateTime.UnixEpochTicks          = -9990000
        //   secondsFromEpoch = ticksFromEpoch / TimeSpan.TicksPerSecond = 0
        //
        // Notice that secondsFromEpoch is rounded *up* by the truncation induced by integer division,
        // whereas we actually always want to round *down* when converting to Unix time. This happens
        // automatically for positive Unix time values. Now the example becomes:
        //   seconds          = ticks / TimeSpan.TicksPerSecond = 62135596799
        //   secondsFromEpoch = seconds - UnixEpochSeconds      = -1
        //
        // In other words, we want to consistently round toward the time 1/1/0001 00:00:00,
        // rather than toward the Unix Epoch (1/1/1970 00:00:00).
        var utcDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        long seconds = utcDateTime.Ticks / TimeSpan.TicksPerSecond;
        return seconds - UnixEpochSeconds;
    }
}
