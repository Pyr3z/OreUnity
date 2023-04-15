/*! @file       Editor/DeviceDeciderDrawer.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-01
**/

using UnityEngine;
using UnityEditor;


namespace Ore.Editor
{

  [CustomPropertyDrawer(typeof(DeviceDeciderData))]
  public class DeviceDeciderDrawer : PropertyDrawer
  {
    readonly DeviceDecider m_ScratchDad = new DeviceDecider();


    public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
    {
      var propRows = prop.FindPropertyRelative(nameof(DeviceDeciderData.Rows));

      var pos = new Rect(total)
      {
        height = (total.height - 42f - m_ScratchDad.ContinuousCount * 20f).AtLeast(18f)
      };

      EditorGUI.BeginChangeCheck();
      EditorGUI.PropertyField(pos, propRows, label);
      if (EditorGUI.EndChangeCheck())
      {
        UpdateScratchDaddy(propRows);
      }

      if (!propRows.isExpanded)
      {
        return;
      }

      var propConfigs = prop.FindPropertyRelative(nameof(DeviceDeciderData.EaseCurves));

      pos.y = pos.yMax;
      pos.height = 18f;

      EditorGUI.BeginChangeCheck();
      label.text = propConfigs.displayName;
      propConfigs.boolValue = EditorGUI.Toggle(pos, label, propConfigs.boolValue);
      pos.y += 20f;

      propConfigs = prop.FindPropertyRelative(nameof(DeviceDeciderData.SmoothCurves));

      label.text = propConfigs.displayName;
      propConfigs.floatValue = EditorGUI.DelayedFloatField(pos, label, propConfigs.floatValue);
      pos.y += 20f;
      if (EditorGUI.EndChangeCheck())
      {
        UpdateScratchDaddy(propRows);
      }

      foreach (var factor in m_ScratchDad.GetContinuousFactors())
      {
        label.text = factor.Dimension.ToString().ExpandCamelCase();
        _ = EditorGUI.CurveField(pos, label, factor.Curve);
        pos.y += 20f;
      }

      prop.serializedObject.ApplyModifiedProperties();
    }

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
      var propRows = prop.FindPropertyRelative(nameof(DeviceDeciderData.Rows));

      if (!propRows.isExpanded)
      {
        return 20f;
      }

      if (m_ScratchDad.FactorCount == 0)
      {
        UpdateScratchDaddy(propRows);
      }

      return EditorGUI.GetPropertyHeight(propRows) +
             40f +
             m_ScratchDad.ContinuousCount * 20f;
    }


    void UpdateScratchDaddy(SerializedProperty propRows)
    {
      m_ScratchDad.ClearFactors();

      for (int i = 0, ilen = propRows.arraySize; i < ilen; ++i)
      {
        var row = propRows.GetArrayElementAtIndex(i);
        _ = row.NextVisible(enterChildren: true);

        string dimension = row.stringValue;
        _ = row.NextVisible(enterChildren: false);

        string key = row.stringValue;
        _ = row.NextVisible(enterChildren: false);

        string weight = row.stringValue;

        if (!m_ScratchDad.ParseRow(dimension, key, weight))
        {
          Orator.ReachedOnce(propRows.serializedObject.targetObject);
        }
      }

      var it = propRows.Copy();

      it.NextVisible(enterChildren: false);
      bool easeCurves = it.boolValue;
      it.NextVisible(enterChildren: false);
      float smoothCurves = it.floatValue;

      foreach (var factor in m_ScratchDad.GetContinuousFactors())
      {
        if (easeCurves)
          factor.EaseCurve();
        if (!smoothCurves.ApproximatelyZero())
          factor.SmoothCurve(smoothCurves);
      }
    }

  } // end class DeviceDeciderDrawer

}