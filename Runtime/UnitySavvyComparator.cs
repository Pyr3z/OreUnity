/*! @file       Runtime/ObjectSavvyComparator.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-12-07
**/

namespace Ore
{

  public sealed class ObjectSavvyComparator : Comparator<object>
  {
    public override bool IsNone(in object obj)
    {
      if (obj is UnityEngine.Object uobj)
        return !uobj;
      return base.IsNone(in obj);
    }
  } // end sealed class ObjectSavvyComparator

}