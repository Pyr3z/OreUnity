/*! @file       Static/Algorithms.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-02-30
 *
 *  @brief      Collection of generic data structure algorithms.
**/

using System.Collections.Generic;


namespace Ore
{

  public static class Algorithms
  {

    /// <summary>
    /// DOES NOT CHECK FOR NULL OR VALID INDICES
    /// </summary>
    public static void Swap<T>(this IList<T> list, int idx1, int idx2)
    {
      var temp = list[idx1];
      list[idx1] = list[idx2];
      list[idx2] = temp;
    }


    public static void MakeHeap<T>(this IList<T> list, System.Comparison<T> cmp)
    {
      OAssert.AllNotNull(list, cmp);

      // `node` is the index of the last non-leaf node.
      // We start there and iterate backwards because any leaf nodes can be skipped.
      for (int node = (list.Count - 1 - 1) / 2; node >= 0; --node)
      {
        HeapifyDown(list, node, cmp);
      }
    }

    public static void PushHeap<T>(this IList<T> list, T push, System.Comparison<T> cmp)
    {
      OAssert.AllNotNull(list, cmp);

      int child  = list.Count;
      int parent = (child - 1) / 2;

      list.Add(push);

      // heapify up
      while (child > 0 && cmp(list[child], list[parent]) > 0)
      {
        list.Swap(child, parent);

        child  = parent;
        parent = (parent - 1) / 2;
      }
    }

    public static T PopHeap<T>(this IList<T> list, System.Comparison<T> cmp)
    {
      OAssert.AllNotNull(list, cmp);

      int last = list.Count - 1;
      if (last < 0)
        return default;

      var item = list[0];

      if (last > 1)
      {
        list.Swap(0, last);
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

        list.Swap(node, max);

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

      list.Swap(node, max);

      if (max <= (list.Count - 1 - 1) / 2)
        HeapifyDownRecursive(list, max, cmp); // <--
    }

  } // end static class Algorithms

}


