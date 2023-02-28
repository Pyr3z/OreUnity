/*! @file       Attributes/OnValueChangedAttribute.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-02-28
**/

using JetBrains.Annotations;


namespace Ore
{
  [PublicAPI]
  [System.AttributeUsage(System.AttributeTargets.Field)]
  [System.Diagnostics.Conditional("UNITY_EDITOR")]
  public class OnValueChangedAttribute : UnityEngine.PropertyAttribute
  {

    public readonly string MethodName;

    public OnValueChangedAttribute(string methodName)
    {
      MethodName = methodName;
    }

  } // end class OnValueChangedAttribute
}