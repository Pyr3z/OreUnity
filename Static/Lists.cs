/*! @file       Static/Lists.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-20
**/

using JetBrains.Annotations;

using System.Collections.Generic;

using MethodImplAttribute = System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions   = System.Runtime.CompilerServices.MethodImplOptions;


namespace Ore
{
  /// <summary>
  /// Utilities and generic extensions for `IList<T>` containers.
  /// </summary>
  [PublicAPI]
  public static class Lists
  {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEmpty<T>([CanBeNull] this ICollection<T> list)
    {
      return list == null || list.Count == 0;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PushFront<T>([NotNull] this IList<T> list, T item)
    {
      // OAssert.NotNull(list, nameof(list));
      list.Insert(0, item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PushBack<T>([NotNull] this IList<T> list, T item)
    {
      // OAssert.NotNull(list, nameof(list));
      list.Insert(list.Count, item);
    }


    public static T PopFront<T>([NotNull] this IList<T> list)
    {
      OAssert.NotEmpty(list, nameof(list));

      var result = list[0];
      list.RemoveAt(0);
      return result;
    }

    public static T PopBack<T>([NotNull] this IList<T> list)
    {
      OAssert.NotEmpty(list, nameof(list));

      var result = list[list.Count - 1];
      list.RemoveAt(list.Count - 1);
      return result;
    }

    [NotNull]
    public static T[] PopBack<T>([NotNull] this IList<T> list, int count)
    {
      OAssert.NotNull(list, nameof(list));
      OAssert.True(count > 0, "count > 0");

      var result = new T[count];

      int i = 0, j = list.Count;
      while (count --> 0 && j > 0)
      {
        result[i++] = list[--j];
        list.RemoveAt(j);
      }

      return result;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Front<T>([NotNull] this IReadOnlyList<T> list, T fallback = default)
    {
      return list.Count == 0 ? fallback : list[0];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Back<T>([NotNull] this IReadOnlyList<T> list, T fallback = default)
    {
      return list.Count == 0 ? fallback : list[list.Count - 1];
    }


    public static int BinarySearch<T>(
      [NotNull]   this IReadOnlyList<T> list,
      [CanBeNull] T value)
    {
      return BinarySearch(list, value, Comparator<T>.Default);
    }

    public static int BinarySearch<T>(
      [NotNull]   this IReadOnlyList<T> list,
      [CanBeNull] T value,
      [NotNull] IComparer<T> comparer)
    {
      int lhs = 0;
      int rhs = list.Count - 1;

      while (lhs <= rhs)
      {
        int idx = (lhs + rhs) >> 1;
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
      }

      return ~lhs;
    }


    public static void Shuffle<T>([NotNull] this IList<T> list)
    {
      int len = list.Count;
      for (int i = 0; i < len; ++i)
      {
        int swap = Integers.RandomIndex(len);
        (list[i],list[swap]) = (list[swap],list[i]);
      }
    }

    [NotNull]
    public static IList<T> Shuffled<T>([NotNull] this IList<T> list)
    {
      var copy = new List<T>(list);
      Shuffle(copy);
      return copy;
    }

  } // end static class Lists

}
