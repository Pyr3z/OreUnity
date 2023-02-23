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
    private DeviceDecider m_ScratchDad = new DeviceDecider();


    public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
    {
      var prop_rows = prop.FindPropertyRelative(nameof(SerialDeviceDecider.Rows));

      prop_rows.isExpanded = true;

      var pos = new Rect(total)
      {
        height = total.height - m_ScratchDad.ContinuousCount * OGUI.STD_LINE_ADVANCE
      };

      EditorGUI.BeginChangeCheck();
      EditorGUI.PropertyField(pos, prop_rows, label);
      if (EditorGUI.EndChangeCheck())
      {
        UpdateScratchDaddy(prop_rows);
      }

      pos.y = pos.yMax + 2f;
      pos.height = OGUI.STD_LINE_HEIGHT;

      foreach (var factor in m_ScratchDad.GetContinuousFactors())
      {
        label.text = factor.Dimension.ToString().ExpandCamelCase();
        _ = EditorGUI.CurveField(pos, label, factor.Curve);
        pos.y += OGUI.STD_LINE_ADVANCE;
      }
    }

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
      var prop_rows = prop.FindPropertyRelative(nameof(SerialDeviceDecider.Rows));

      if (m_ScratchDad.FactorCount == 0)
      {
        UpdateScratchDaddy(prop_rows);
      }

      return EditorGUI.GetPropertyHeight(prop_rows) +
             m_ScratchDad.ContinuousCount * OGUI.STD_LINE_ADVANCE;
    }


    private void UpdateScratchDaddy(SerializedProperty prop_rows)
    {
      m_ScratchDad.Clear();

      for (int i = 0, ilen = prop_rows.arraySize; i < ilen; ++i)
      {
        var row = prop_rows.GetArrayElementAtIndex(i);
        _ = row.NextVisible(enterChildren: true);

        string dimension = row.stringValue;
        _ = row.NextVisible(enterChildren: false);

        string key = row.stringValue;
        _ = row.NextVisible(enterChildren: false);

        string weight = row.stringValue;

        if (!m_ScratchDad.TryParseRow(dimension, key, weight))
        {
          Orator.ReachedOnce(prop_rows.serializedObject.targetObject);
        }
      }
    }

  } // end class DeviceDeciderDrawer

}