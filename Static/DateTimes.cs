/*! @file       Static/DateTimes.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-01-22
**/

using JetBrains.Annotations;

using UnityEngine;

using DateTime      = System.DateTime;
using DateTimeKind  = System.DateTimeKind;
using TimeSpan      = System.TimeSpan;


namespace Ore
{
  [PublicAPI]
  public static class DateTimes
  {

    public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static readonly DateTime SpreadsheetEpoch = new DateTime(1899, 12, 30, 0, 0, 0, DateTimeKind.Utc);


    public const long TicksPerMs     = 10000L;
    public const long TicksPerSecond = 10000000L;
    public const long TicksPerHour   = 36000000000L;
    public const long TicksPerDay    = 864000000000L;


    public static DateTime Today => DateTime.UtcNow.Date; // Beware of DateTime.Today; it returns local time!
    public static DateTime Tomorrow => DateTime.UtcNow.AddDays(1).Date;
    public static DateTime Yesterday => DateTime.UtcNow.AddDays(-1).Date;


    public static bool IsToday(long ticks)
    {
      return ticks / TicksPerDay == Today.Ticks / TicksPerDay;
    }


    public static string ToISO8601(this DateTime timepoint)
    {
      return timepoint.ToString("O", Strings.InvariantFormatter);
    }

    public static string ToRFC1123(this DateTime timepoint)
    {
      return timepoint.ToString("R", Strings.InvariantFormatter);
    }


    public static long ToUnixTicks(this DateTime timepoint)
    {
      if (timepoint == default)
        return 0;

      return (timepoint.ToUniversalTime() - Epoch).Ticks;
    }

    public static double ToUnixMillis(this DateTime timepoint)
    {
      if (timepoint == default)
        return 0;

      return (timepoint.ToUniversalTime() - Epoch).TotalMilliseconds;
    }

    public static double ToUnixSeconds(this DateTime timepoint)
    {
      if (timepoint == default)
        return 0;

      return (timepoint.ToUniversalTime() - Epoch).TotalSeconds;
    }

    public static double ToUnixDays(this DateTime timepoint)
    {
      if (timepoint == default)
        return 0;

      return (timepoint.ToUniversalTime() - Epoch).TotalDays;
    }

    public static double ToSpreadsheetDays(this DateTime timepoint)
    {
      if (timepoint == default || timepoint < SpreadsheetEpoch)
        return 0;

      return (timepoint.ToUniversalTime() - SpreadsheetEpoch).TotalDays;
    }


    public static double NowUnixMillis(bool local = false)
    {
      return ToUnixMillis(local ? DateTime.Now : DateTime.UtcNow);
    }

    public static double NowUnixSeconds(bool local = false)
    {
      return ToUnixSeconds(local ? DateTime.Now : DateTime.UtcNow);
    }

    public static double NowUnixDays(bool local = false)
    {
      return ToUnixDays(local ? DateTime.Now : DateTime.UtcNow);
    }

    public static double NowSpreadsheetDays(bool local = false)
    {
      return ToSpreadsheetDays(local ? DateTime.Now : DateTime.UtcNow);
    }


    public static void SetPlayerPref(this DateTime value, [NotNull] string key)
    {
      PlayerPrefs.SetString(key, value.ToBinary().ToHexString());
    }

    public static bool TryGetPlayerPref([NotNull] string key, out DateTime value)
    {
      string str = PlayerPrefs.GetString(key, string.Empty);

      if (!str.IsEmpty() && Parsing.TryParseHex(str, out long raw))
      {
        value = DateTime.FromBinary(raw);
        return true;
      }

      value = default;
      return false;
    }

    public static DateTime GetPlayerPref([NotNull] string key)
    {
      string str = PlayerPrefs.GetString(key, string.Empty);

      if (!str.IsEmpty() && Parsing.TryParseHex(str, out long raw))
      {
        return DateTime.FromBinary(raw);
      }

      return default;
    }

  } // end class DateTimes
}