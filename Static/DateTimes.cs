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


    public static void SetPlayerPref(this DateTime value, [NotNull] string key)
    {
      PlayerPrefs.SetString(key, value.ToBinary().ToHexString());
    }

    public static DateTime GetPlayerPref([NotNull] string key)
    {
      string str = PlayerPrefs.GetString(key, string.Empty);

      if (Parsing.TryParseHex(str, out long raw))
      {
        return DateTime.FromBinary(raw);
      }

      return DateTime.MinValue;
    }

  } // end class DateTimes
}