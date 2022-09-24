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
    public static void Make<T>(IList<T> list, System.Comparison<T> cmp)
    {
      OAssert.AllNotNull(list, cmp);

      // `node` is the index of the last non-leaf node.
      // We start there and iterate backwards because any leaf nodes can be skipped.
      for (int node = (list.Count - 1 - 1) / 2; node >= 0; --node)
      {
        HeapifyDown(list, node, cmp);
      }
    }

    public static void Push<T>(IList<T> list, T push, System.Comparison<T> cmp)
    {
      OAssert.AllNotNull(list, cmp);

      int child  = list.Count;
      int parent = (child - 1) / 2;

      list.Add(push);

      // heapify up
      while (child > 0 && cmp(list[child], list[parent]) > 0)
      {
        (list[child],list[parent]) = (list[parent],list[child]);
        child  = parent;
        parent = (parent - 1) / 2;
      }
    }

    public static T Pop<T>(IList<T> list, System.Comparison<T> cmp)
    {
      OAssert.AllNotNull(list, cmp);

      int last = list.Count - 1;
      if (last < 0)
        return default;

      var item = list[0];

      if (last > 1)
      {
        (list[0],list[last]) = (list[last],list[0]);
        list.RemoveAt(last);
        HeapifyDown(list, 0, cmp);
      }

      return item;
    }

    public static void HeapifyDown<T>(IList<T> list, int node, System.Comparison<T> cmp)
    {
      OAssert.AllNotNull(list, cmp);

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
    public static void HeapifyDownRecursive<T>(IList<T> list, int node, System.Comparison<T> cmp)
    {
      OAssert.AllNotNull(list, cmp);

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


