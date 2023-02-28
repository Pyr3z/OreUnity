/*! @file       Attributes/RequiredAttribute.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-02-28
**/

using JetBrains.Annotations;


namespace Ore
{
  [PublicAPI]
  [System.AttributeUsage(System.AttributeTargets.Field)]
  public class RequiredAttribute : UnityEngine.PropertyAttribute
  {

    public RequiredAttribute()
    {
      // TODO
    }

  } // end class RequiredAttribute
}