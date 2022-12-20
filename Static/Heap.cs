/*! @file       Static/Heap.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-02-30
**/

using System.Collections.Generic;
using JetBrains.Annotations;


namespace Ore
{
  [PublicAPI]
  public static class Heap
  {
    public static void Make<T>([NotNull] IList<T> list, [NotNull] System.Comparison<T> cmp)
    {
      // `node` is the index of the last non-leaf node.
      // We start there and iterate backwards because any leaf nodes can be skipped.
      for (int node = (list.Count - 2) / 2; node >= 0; --node)
      {
        HeapifyDown(list, node, cmp);
      }
    }

    public static void Push<T>([NotNull] List<T> list, T push, [NotNull] System.Comparison<T> cmp)
    {
      int child  = list.Count;
      int parent = (child - 1) / 2;

      list.Add(push); // isn't supported when IList<T> is an array

      // heapify up
      while (child > 0 && cmp(list[child], list[parent]) > 0)
      {
        (list[child],list[parent]) = (list[parent],list[child]);
        child  = parent;
        parent = (parent - 1) / 2;
      }
    }

    public static void Push<T>([NotNull] T[] arr, int n, T push, [NotNull] System.Comparison<T> cmp)
    {
      #if UNITY_ASSERTIONS
        OAssert.True(n < arr.Length, "n < arr.Length");
      #endif

      arr[n] = push;

      int parent = (n - 1) / 2;

      // heapify up
      while (n > 0 && cmp(arr[n], arr[parent]) > 0)
      {
        (arr[n],arr[parent]) = (arr[parent],arr[n]);
        n                    = parent;
        parent               = (parent - 1) / 2;
      }
    }

    public static T Pop<T>([NotNull] List<T> list, [NotNull] System.Comparison<T> cmp)
    {
      int last = list.Count - 1;
      if (last < 0)
        return default;

      var item = list[0];

      if (last > 0)
      {
        (list[0],list[last]) = (list[last],list[0]);
        list.RemoveAt(last); // isn't supported when IList<T> is an array
        HeapifyDown(list, 0, cmp);
      }
      else
      {
        list.RemoveAt(0);
      }

      return item;
    }

    public static T Pop<T>([NotNull] T[] arr, int n, [NotNull] System.Comparison<T> cmp)
    {
      #if UNITY_ASSERTIONS
        OAssert.True(n < arr.Length, "n < arr.Length");
      #endif

      int last = n - 1;
      if (last < 0)
        return default;

      var item = arr[0];

      if (last > 0)
      {
        (arr[0],arr[last]) = (arr[last],arr[0]);
        arr[last] = default;
        HeapifyDown(arr, 0, cmp);
      }
      else
      {
        arr[0] = default;
      }

      return item;
    }

    public static void HeapifyDown<T>([NotNull] IList<T> list, int node, [NotNull] System.Comparison<T> cmp)
    {
      // This is way faster than the recursive version!

      int count = list.Count;
      int last  = (count - 1 - 1) / 2;
      int max   = node;

      while (node <= last)
      {
        int lhs = 2 * node + 1;
        int rhs = 2 * node + 2;

        if (lhs < count && cmp(list[lhs], list[max]) > 0)
          max = lhs;

        if (rhs < count && cmp(list[rhs], list[max]) > 0)
          max = rhs;

        if (max == node)
          return;

        (list[node],list[max]) = (list[max],list[node]);

        node = max;
      }
    }

    [System.Obsolete("This version is way slower than the non-recursive version! (UnityUpgradable) -> HeapifyDown<T>(*)")]
    public static void HeapifyDownRecursive<T>([NotNull] IList<T> list, int node, [NotNull] System.Comparison<T> cmp)
    {
      // eww, recursion!

      int max = node;
      int lhs = 2 * node + 1;
      int rhs = 2 * node + 2;

      if (lhs < list.Count && cmp(list[lhs], list[max]) > 0)
        max = lhs;

      if (rhs < list.Count && cmp(list[rhs], list[max]) > 0)
        max = rhs;

      if (max == node)
        return;

      (list[node],list[max]) = (list[max],list[node]);

      if (max <= (list.Count - 1 - 1) / 2)
        HeapifyDownRecursive(list, max, cmp); // <--
    }

  } // end static class Heap

}


