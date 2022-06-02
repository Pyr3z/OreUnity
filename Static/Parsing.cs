/** @file   Static/Parsing.cs
    @author levianperez\@gmail.com
    @author levi\@leviperez.dev
    @date   2022-06-01
**/

using StringBuilder = System.Text.StringBuilder;


namespace Bore
{

  public static class Parsing
  {

    public static int Count(string str, params char[] chars)
    {
      int count = 0;

      foreach (char check in chars)
      {
        foreach (char c in str)
        {
          // fuh
          if (string.CompareOrdinal(c.ToString(), check.ToString()) == 0)
            ++count;
        }
      }

      return count;
    }

    public static int CountDigits(string str)
    {
      int count = 0;

      foreach (char c in str)
      {
        if (char.IsDigit(c))
          ++count;
      }

      return count;
    }

    public static int CountContiguousDigits(string str)
    {
      int count = 0, max = 0;

      foreach (char c in str)
      {
        if (char.IsDigit(c))
        {
          ++count;
        }
        else if (c == '.') { } // no-op
        else if (count > 0)
        {
          if (max < count)
            max = count;
          count = 0;
        }
      }

      return max < count ? count : max;
    }


    public static bool TryParseTimezoneOffset(string str, out System.TimeSpan span)
    {
      // EXPECTED FORMAT REGEX: @"([+-])?([0-9]{2})(?:[:]?([0-9]{2}))?"

      if (TryParseTimezoneOffset(str, out float hours))
      {
        span = System.TimeSpan.FromHours(hours);
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
          hours += (part / 60f);
      }

      if (negative)
        hours *= -1;

      return true;
    }

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

        val = (10 * val) + (c - '0'); // elevate previous digits, add new 1s place

        if (val < 0 || val > ceil) // overflow
          return 0;

        ++i;
      }

      return (i - start).AtLeast(0); // returns number of characters parsed
    }

  } // end static class Parsing

}
