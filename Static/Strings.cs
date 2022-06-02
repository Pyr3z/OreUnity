/** @file   Static/Strings.cs
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

  } // end static class Strings

}
