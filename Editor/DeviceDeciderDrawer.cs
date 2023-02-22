/*! @file       Editor/DeviceDeciderDrawer.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-01
**/

using UnityEngine;
using UnityEditor;


namespace Ore.Editor
{

  [CustomPropertyDrawer(typeof(SerialDeviceDecider))]
  public class DeviceDeciderDrawer : PropertyDrawer
  {
    private DeviceDecider m_EditorDecider = new DeviceDecider();


    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
      var prop_rows = prop.FindPropertyRelative(nameof(SerialDeviceDecider.Rows));

      prop_rows.isExpanded = true;

      EditorGUI.PropertyField(pos, prop_rows, label);

      // TODO draw continuous curves
    }

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
      var prop_rows = prop.FindPropertyRelative(nameof(SerialDeviceDecider.Rows));

      return EditorGUI.GetPropertyHeight(prop_rows);
    }

  } // end class DeviceDeciderDrawer

}