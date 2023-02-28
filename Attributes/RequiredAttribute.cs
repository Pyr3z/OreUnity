/*! @file       Attributes/RequiredAttribute.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-02-28
**/

using JetBrains.Annotations;


namespace Ore
{
  [PublicAPI]
  [System.AttributeUsage(System.AttributeTargets.Field)]
  [System.Diagnostics.Conditional("UNITY_EDITOR")]
  public class RequiredAttribute : UnityEngine.PropertyAttribute
  {

    public string CustomMessage;

    public RequiredAttribute()
    {
    }

    public RequiredAttribute([CanBeNull] string message)
    {
      CustomMessage = message;
    }

  } // end class RequiredAttribute
}