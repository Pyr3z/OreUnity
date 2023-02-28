/*! @file       Attributes/EnableIfAttribute.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-02-28
**/

using JetBrains.Annotations;


namespace Ore
{
  [PublicAPI]
  [System.AttributeUsage(System.AttributeTargets.Field)]
  [System.Diagnostics.Conditional("UNITY_EDITOR")]
  public class EnableIfAttribute : UnityEngine.PropertyAttribute
  {
    public readonly string ValueGetter;

    public readonly object CompareValue;


    public EnableIfAttribute(string valueGetter)
    {
      ValueGetter  = valueGetter;
      CompareValue = null;
    }

    public EnableIfAttribute(string valueGetter, object equalTo)
    {
      ValueGetter  = valueGetter;
      CompareValue = equalTo;
    }

  } // end class EnableIfAttribute
}