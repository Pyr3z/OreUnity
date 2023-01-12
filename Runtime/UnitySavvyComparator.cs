/*! @file       Runtime/UnitySavvyComparator.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-12-07
**/

namespace Ore
{

  public sealed class UnitySavvyComparator : Comparator<object>
  {
    public override bool IsNone(in object obj)
    {
      return obj is UnityEngine.Object uobj ? !uobj : base.IsNone(in obj);
    }
  } // end sealed class UnitySavvyComparator

}