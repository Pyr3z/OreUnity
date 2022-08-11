/*! @file       Editor/ReadOnlyDrawer.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2021-12-20
**/

using UnityEngine;

using UnityEditor;


namespace Ore.Editor
{

  [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
  internal class ReadOnlyDrawer : DecoratorDrawer // TODO this is an ultra basic implementation, some properties don't work.
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
