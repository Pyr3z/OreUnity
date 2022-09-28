/*! @file       Runtime/HashKeyComparator.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-28
**/

using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;


namespace Ore
{

  public class HashKeyComparator : IEqualityComparer
  {
    public static readonly HashKeyComparator Default = new HashKeyComparator();


    public bool Equals([CanBeNull] object a, [CanBeNull] object b)
    {
      // slightly different short-circuiting than object.Equals
      return a == b || ( a is {} && a.Equals(b) );
    }

    public int GetHashCode([CanBeNull] object obj)
    {
      if (obj is null)
        return 0;

      // ignore sign bit
      return obj.GetHashCode() & int.MaxValue;
    }
  } // end class KeyComparator

}