/** @file   Static/Strings.cs
 *  @author levianperez\@gmail.com
 *  @author levi\@leviperez.dev
 *  @date   2020-06-06
 *
 *  @brief
 *    Utilities for C# string manipulation & querying.
**/

using System.Collections.Generic;

using StringBuilder = System.Text.StringBuilder;
using Encoding      = System.Text.Encoding;
using Convert       = System.Convert;


namespace Ore
{
  /// <summary>
  /// Utilities for C# string manipulation & querying.
  /// </summary>
  public static class Strings
  {
    public static Encoding DefaultEncoding { get; set; } = Encoding.Unicode;


    public static readonly char[] WHITESPACES = { ' ', '\t', '\n', '\r', '\v' };



    public static bool IsEmpty(this string str)
    {
      return str == null || str.Length == 0;
    }


    public static byte[] ToBytes(this string str, Encoding encoding = null)
    {
      if (IsEmpty(str))
        return new byte[0];

      return (encoding ?? DefaultEncoding).GetBytes(str);
    }


    public static string FromBytes(byte[] bytes, Encoding encoding = null)
    {
      if (bytes == null || bytes.Length == 0)
        return string.Empty;

      return (encoding ?? DefaultEncoding).GetString(bytes);
    }


    public static string ToBase64(this string str, Encoding encoding = null)
    {
      return Convert.ToBase64String(str.ToBytes(encoding));
    }

    public static string ParseBase64(this string str, Encoding encoding = null)
    {
      return FromBytes(Convert.FromBase64String(str), encoding);
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

      char c = str[i];
      bool in_word = false;
      var bob = new StringBuilder(ilen + 8);

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
          in_word = true;
        else if (in_word && (char.IsUpper(c) || c == '_'))
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


    public static int CountAny(string str, params char[] chars)
    {
      var charset = new HashSet<char>(chars);

      int count = 0;

      foreach (char c in str)
      {
        if (charset.Contains(c))
          ++count;
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
          ++count;
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

  } // end static class Strings

}
