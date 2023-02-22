/*! @file       Editor/DeviceDeciderRowDrawer.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-01
**/

using UnityEngine;
using UnityEditor;

namespace Ore.Editor
{

  [CustomPropertyDrawer(typeof(SerialDeviceDecider.Row))]
  public class DeviceDeciderRowDrawer : PropertyDrawer
  {
    private bool ValidateDimension(System.Enum dim)
    {
      return (DeviceDimension)dim != DeviceDimension.Continuous;
    }

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
      var prop_dim = prop.FindPropertyRelative(nameof(SerialDeviceDecider.Row.Dimension));
      var prop_key = prop.FindPropertyRelative(nameof(SerialDeviceDecider.Row.Key));
      var prop_wgt = prop.FindPropertyRelative(nameof(SerialDeviceDecider.Row.Weight));

      pos.width = (pos.width - 4f) / 3f;

      if (!DeviceDimensions.TryParse(prop_dim.stringValue, out DeviceDimension dim))
      {
        dim = DeviceDimension.None;
      }

      EditorGUI.BeginChangeCheck();
      dim = (DeviceDimension)EditorGUI.EnumPopup(pos, GUIContent.none, dim, ValidateDimension);
      if (EditorGUI.EndChangeCheck())
      {
        prop_dim.stringValue = dim.ToString();
      }

      pos.x += 2f + pos.width;

      EditorGUI.DelayedTextField(pos, prop_key, GUIContent.none);

      if (prop_key.stringValue.IsEmpty())
      {
        prop_key.stringValue = "-";
      }

      pos.x += 2f + pos.width;

      if (!float.TryParse(prop_wgt.stringValue, out float wgt))
        wgt = -1f;

      EditorGUI.BeginChangeCheck();
      wgt = EditorGUI.DelayedFloatField(pos, GUIContent.none, wgt);
      if (EditorGUI.EndChangeCheck())
      {
        prop_wgt.stringValue = wgt.ToInvariant();
      }
    }
  } // end class DeviceDeciderRowDrawer

}
