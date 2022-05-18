/*  @file       Bore/Runtime/AttributeDrawers/ReadOnlyDrawer.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2021-12-20
 */

using UnityEngine;

using UnityEditor;


namespace Bore.Editor
{

  [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
  public sealed class ReadOnlyDrawer : DecoratorDrawer
  {
    public override void OnGUI(Rect pos)
    {
      GUI.enabled = false;
    }

    public override float GetHeight()
    {
      return 0f;
    }
  }

}
