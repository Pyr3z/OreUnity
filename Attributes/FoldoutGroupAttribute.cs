/*! @file       Attributes/FoldoutGroupAttribute.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-02-28
**/

using JetBrains.Annotations;


namespace Ore
{
  [PublicAPI]
  [System.AttributeUsage(System.AttributeTargets.Field)]
  public class FoldoutGroupAttribute : UnityEngine.PropertyAttribute
  {

    public readonly string GroupId;

    public readonly bool StartExpanded;

    public FoldoutGroupAttribute(string groupId, bool expanded = false)
    {
      GroupId       = groupId;
      StartExpanded = expanded;
    }

  } // end class FoldoutGroupAttribute
}