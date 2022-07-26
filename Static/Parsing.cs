/** @file   Static/Parsing.cs
 *  @author levianperez\@gmail.com
 *  @author levi\@leviperez.dev
 *  @date   2022-06-01
**/

using UnityEngine;

using TimeSpan = System.TimeSpan;
using Predicate = System.Func<string, bool>;


namespace Ore
{
  /// <summary>
  /// Utilities for parsing strings into other data types.
  /// </summary>
  public static class Parsing
  {
    public static readonly Color32 DefaultColor32 = Color.magenta;


    public static bool FindLine(string rawtext, Predicate where, out string line)
    {
      line = null;

      if (where != null)
      {
        foreach (var l in rawtext.Split(new char[] { '\n' }, System.StringSplitOptions.None))
        {
          if (where(l))
          {
            line = l;
            return true;
          }
        }
      }

      return false;
    }


    public static bool TryParseNextIndex(string str, out int idx)
    {
      idx = -1;

      int idx_start = str.IndexOf('[') + 1;
      int idx_len = str.IndexOf(']', idx_start) - idx_start;

      return idx_len > 0 &&
              int.TryParse(str.Substring(idx_start, idx_len), out idx) &&
              idx >= 0;
    }


    public static bool TryParseTimezoneOffset(string str, out TimeSpan span)
    {
      // EXPECTED FORMAT REGEX: @"([+-])?([0-9]{2})(?:[:]?([0-9]{2}))?"

      if (TryParseTimezoneOffset(str, out float hours))
      {
        span = TimeSpan.FromHours(hours);
        return true;
      }

      span = default;
      return false;
    }

    public static bool TryParseTimezoneOffset(string str, out float hours)
    {
      // EXPECTED FORMAT REGEX: @"([+-])?([0-9]{2})(?:[:]?([0-9]{2}))?"

      hours = 0f;

      int i = 0, ilen = str.Length;
      bool negative = false;
      char c = '\0';

      while (i < ilen)
      {
        c = str[i];

        if (char.IsDigit(c))
          break;
        else if (negative)
          return false;

        if (c == '-')
          negative = true;

        ++i;
      }

      if (!char.IsDigit(c))
        return false;

      // parse hour
      int inc = PartialParsePositive(str, i, 2, ceil: 23, out int part);
      if (inc == 0)
        return false;

      hours = part;
      i += inc;

      // parse minutes (optional)
      if (i < ilen)
      {
        if (str[i] == ':')
          ++i;

        inc = PartialParsePositive(str, i, 2, ceil: 59, out part);

        if (inc > 0 && part > 0)
          hours += part / 60f;
      }

      if (negative)
        hours *= -1;

      return true;
    }


    public static bool TryParseColor32(string hex, out Color32 color)
    {
      color = DefaultColor32;

      if (hex == null || hex.Length < 2)
        return false;

      int i = 0;

      if (hex[0] == '#')
        ++i;

      if (hex[i] == '0' && (hex[i + 1] == 'x' || hex[i + 1] == 'X'))
      {
        if (i + 3 < hex.Length)
          i += 2;
        else
          return false;
      }

      char hexhi, hexlo;
      byte write = 0x00;

      bool special_syntax = hex[0] == '#' && hex.Length - i < 5;

      int j = 0;
      while (j < 4 && i < hex.Length - 1)
      {
        if (special_syntax)
          // special HTML syntax: makes 
          hexhi = hexlo = hex[i++];
        else
        {
          hexhi = hex[i++];
          hexlo = hex[i++];
        }

        if (TryParseHexByte(hexhi, hexlo, ref write))
          color[j++] = write;
        else
        {
          break;
        }
      }

      while (j < 3) // fill in the remainder
        color[j++] = 0x00;
      if (j == 3)
        color[j] = 0xFF;

      return true;
    }


    public static bool TryParseHexByte(char ascii_hi, char ascii_lo, ref byte write)
    {
      if (!TryParseHexNybble(ascii_hi, ref write))
        return false;

      write <<= 4;
      return TryParseHexNybble(ascii_lo, ref write);
    }


    #region PRIVATE SECTION

    private static int PartialParsePositive(string str, int start, int len, int ceil, out int val)
    {
      const uint OVERFLOW_BYTE = 0xF0000000u;

      val = 0;
      int i = start, ilen = (start + len).AtMost(str.Length);

      while (i < ilen)
      {
        char c = str[i];
        if (c < '0' || c > '9')
          break;

        if ((val & OVERFLOW_BYTE) != 0)
          return 0;

        val = 10 * val + (c - '0'); // elevate previous digits, add new 1s place

        if (val < 0 || val > ceil) // overflow
          return 0;

        ++i;
      }

      return (i - start).AtLeast(0); // returns number of characters parsed
    }

    private static bool TryParseHexNybble(char ascii, ref byte write)
    {
      const byte CLEAR_LO = 0xF0;

      if (ascii < '0')
        return false;

      if (ascii <= '9')
      {
        write = (byte)(write & CLEAR_LO | ascii - '0');
        return true;
      }

      if (ascii < 'A')
        return false;

      if (ascii <= 'F')
      {
        write = (byte)(write & CLEAR_LO | ascii - 'A' + 10);
        return true;
      }

      if (ascii < 'a')
        return false;

      if (ascii <= 'f')
      {
        write = (byte)(write & CLEAR_LO | ascii - 'a' + 10);
        return true;
      }

      return false;
    }

    #endregion PRIVATE SECTION

  } // end static class Parsing

}
