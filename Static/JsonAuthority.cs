/*! @file       Static/JsonAuthority.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-01-22
**/

using JetBrains.Annotations;

using System.Collections.Generic;

using Encoding = System.Text.Encoding;


namespace Ore
{
  [PublicAPI]
  public static class JsonAuthority
  {

    public static readonly Encoding Encoding     = Encoding.UTF8;
    public static readonly Encoding WideEncoding = Encoding.Unicode; // LE UTF16


    // TODO


    internal static IDictionary<string,object> DefaultMapMaker(int capacity)
    {
      return new HashMap<string,object>(capacity);
    }
    internal static IList<object> DefaultListMaker(int capacity)
    {
      return new object[capacity];
    }

  } // end class JsonAuthority
}