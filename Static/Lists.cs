/*! @file       Static/Lists.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-20
**/

using System.Collections;
using System.Collections.Generic;


namespace Ore
{
  /// <summary>
  /// Utilities and generic extensions for `IList<T>` containers.
  /// </summary>
  public static class Lists
  {

    public static bool IsEmpty<T>(this ICollection<T> list)
    {
      return list == null || list.Count == 0;
    }


    public static void PushFront<T>(this IList<T> list, T item)
    {
      OAssert.NotNull(list, nameof(list));
      list.Insert(0, item);
    }

    public static void PushBack<T>(this IList<T> list, T item)
    {
      OAssert.NotNull(list, nameof(list));
      list.Insert(list.Count, item);
    }


    public static T PopFront<T>(this IList<T> list)
    {
      OAssert.NotEmpty(list, nameof(list));

      var result = list[0];
      list.RemoveAt(0);
      return result;
    }

    public static T PopBack<T>(this IList<T> list)
    {
      OAssert.NotEmpty(list, nameof(list));

      var result = list[list.Count - 1];
      list.RemoveAt(list.Count - 1);
      return result;
    }

    public static T[] PopBack<T>(this IList<T> list, int count)
    {
      OAssert.NotNull(list, nameof(list));

      var result = new T[count];

      int i = 0, j = list.Count;
      while (count-- > 0 && j > 0)
      {
        result[i++] = list[--j];
        list.RemoveAt(j);
      }

      return result;
    }


    public static T Front<T>(this IReadOnlyList<T> list, T fallback = default)
    {
      if (list == null || list.Count == 0)
        return fallback;

      return list[0];
    }

    public static T Back<T>(this IReadOnlyList<T> list, T fallback = default)
    {
      if (list == null || list.Count == 0)
        return fallback;

      return list[list.Count - 1];
    }

  } // end static class Lists

}
