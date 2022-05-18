/** @file   Runtime/StaticTypes/Strings.cs
    @author levianperez\@gmail.com
    @author levi\@leviperez.dev
    @date   2020-06-06

    @brief
      Utilities for C# string manipulation & generic parsing.
**/

using StringBuilder = System.Text.StringBuilder;


namespace Bore
{
  public static class Strings
  {
    public static readonly char[] WHITESPACES = { ' ', '\t', '\n', '\r', '\v' };


    public static bool IsEmpty(this string str)
    {
      return str == null || str.Length == 0;
    }

    public static int Count(this string str, params char[] chars)
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

    public static int CountDigits(this string str)
    {
      int count = 0;

      foreach (char c in str)
      {
        if (char.IsDigit(c))
          ++count;
      }

      return count;
    }

    public static int CountContiguousDigits(this string str)
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

    public static string MakeGUID()
    {
    #if UNITY_EDITOR
      return UnityEditor.GUID.Generate().ToString();
    #else
      return System.Guid.NewGuid().ToString("N");
    #endif
    }

    public static string ExpandCamelCase(this string str)
    {
      if (str == null || str.Length <= 1)
        return str;

      int i = 0, ilen = str.Length;

      if (str[1] == '_')
      {
        // handles the forms "m_Variable", "s_StaticStuff" ...
        if (str.Length == 2)
          return str;
        else
          i = 2;
      }
      else if (char.IsLower(str[0]) && char.IsUpper(str[1]))
      {
        // handles "mVariable", "aConstant"...
        i = 1;
      }

      char c       = str[i];
      bool in_word = false;
      var  bob     = new StringBuilder(ilen + 8);

      if (char.IsLower(c)) // adjusts for lower camel case
      {
        in_word = true;
        bob.Append(char.ToUpper(c));
        ++i;
      }
      
      while (i < ilen)
      {
        c = str[i];

        if (char.IsLower(c) || char.IsDigit(c))
        {
          in_word = true;
        }
        else if (in_word && ( char.IsUpper(c) || c == '_' ))
        {
          bob.Append(' ');
          in_word = false;
        }

        if (char.IsLetterOrDigit(c))
          bob.Append(c);

        ++i;
      }

      return bob.ToString();
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

  } // end static class Strings

}
