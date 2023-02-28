/*! @file       Attributes/HideIfAttribute.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-02-28
**/

using JetBrains.Annotations;


namespace Ore
{
  [PublicAPI]
  [System.AttributeUsage(System.AttributeTargets.Field)]
  public class HideIfAttribute : UnityEngine.PropertyAttribute
  {
    public readonly string ValueGetter;

    public readonly object CompareValue;


    public HideIfAttribute(string valueGetter)
    {
      ValueGetter  = valueGetter;
      CompareValue = null;
    }

    public HideIfAttribute(string valueGetter, object equalTo)
    {
      ValueGetter  = valueGetter;
      CompareValue = equalTo;
    }

  } // end class HideIfAttribute
}