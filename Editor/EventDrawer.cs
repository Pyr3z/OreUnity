/** @file       Editor/EventDrawer.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-14
**/

using UnityEngine;
using UnityEditor;


namespace Bore
{

  [CustomPropertyDrawer(typeof(IEvent))]
  internal class EventDrawer : UnityEditorInternal.UnityEventDrawer
  {
    private const float UNEXPANDED_HEIGHT = 20f;


    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
      return base.GetPropertyHeight(prop, label);
    }

  } // end class EventDrawer

}
