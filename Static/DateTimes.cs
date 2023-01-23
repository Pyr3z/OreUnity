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


    public static long ToUnixMillis(this DateTime timepoint)
    {
      if (timepoint == default || timepoint < Epoch)
        return 0L;

      return (timepoint.ToUniversalTime() - Epoch).TotalMilliseconds.Rounded();
    }

    public static int ToUnixSeconds(this DateTime timepoint)
    {
      if (timepoint == default || timepoint < Epoch)
        return 0;

      return (int)(timepoint.ToUniversalTime() - Epoch).TotalSeconds.Rounded();
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