/*! @file       Static/Lists.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-20
**/

using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;


namespace Ore
{
  /// <summary>
  /// Utilities and generic extensions for `IList<T>` containers.
  /// </summary>
  [PublicAPI]
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

    public static int BinarySearch<T>(
      [NotNull]   this IReadOnlyList<T> list,
      [CanBeNull] T value,
      [CanBeNull] IComparer<T> comparer = null)
    {
      comparer ??= Comparer<T>.Default;

      int lhs = 0;
      int rhs = list.Count - 1;
      int idx = rhs / 2;

      while (lhs <= rhs)
      {
        int cmp = comparer.Compare(list[idx], value);

        if (cmp < 0)
        {
          lhs = idx + 1;
        }
        else if (cmp > 0)
        {
          rhs = idx - 1;
        }
        else
        {
          return idx;
        }

        idx = (lhs + rhs) / 2;
      }

      return ~idx;
    }

  } // end static class Lists

}
