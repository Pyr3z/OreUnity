/*! @file       Editor/TimeIntervalDrawer.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-11-08
**/

using UnityEngine;
using UnityEditor;


namespace Ore.Editor
{
  [CustomPropertyDrawer(typeof(TimeInterval))]
  internal class TimeIntervalDrawer : PropertyDrawer
  {

    // class field will be shared in arrays of TimeIntervals (probs fine for now)
    TimeInterval.Units? m_Units;


    public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
    {
      var propTicks = prop.FindPropertyRelative(nameof(TimeInterval.Ticks));
      var propAsFrames = prop.FindPropertyRelative("m_AsFrames");

      if (propAsFrames.boolValue)
      {
        m_Units = 0; // "Frames"
      }
      else if (!m_Units.HasValue || m_Units == TimeInterval.Units.Frames)
      {
        m_Units = TimeInterval.DetectUnits(propTicks.longValue);
      }

      var pos = EditorGUI.PrefixLabel(total, label);

      OGUI.IndentLevel.Push(0);

      pos.width = (pos.width - 2f) / 2f;

      long edit = 0L;
      double d;

      EditorGUI.BeginChangeCheck();
      switch (m_Units)
      {
        case TimeInterval.Units.Frames:
        case TimeInterval.Units.Ticks:
          edit = EditorGUI.LongField(pos, propTicks.longValue);
          break;

        case TimeInterval.Units.Milliseconds:
          d    = EditorGUI.DoubleField(pos, propTicks.longValue * TimeInterval.TICKS2MS);
          edit = (long)(d / TimeInterval.TICKS2MS + (d >= 0 ? 0.5 : -0.5));
          break;

        case TimeInterval.Units.Seconds:
          d    = EditorGUI.DoubleField(pos, propTicks.longValue * TimeInterval.TICKS2SEC);
          edit = (long)(d / TimeInterval.TICKS2SEC + (d >= 0 ? 0.5 : -0.5));
          break;

        case TimeInterval.Units.Minutes:
          d    = EditorGUI.DoubleField(pos, propTicks.longValue * TimeInterval.TICKS2MIN);
          edit = (long)(d / TimeInterval.TICKS2MIN + (d >= 0 ? 0.5 : -0.5));
          break;

        case TimeInterval.Units.Hours:
          d    = EditorGUI.DoubleField(pos, propTicks.longValue * TimeInterval.TICKS2HR);
          edit = (long)(d / TimeInterval.TICKS2HR + (d >= 0 ? 0.5 : -0.5));
          break;

        case TimeInterval.Units.Days:
          d    = EditorGUI.DoubleField(pos, propTicks.longValue * TimeInterval.TICKS2DAY);
          edit = (long)(d / TimeInterval.TICKS2DAY + (d >= 0 ? 0.5 : -0.5));
          break;

        case TimeInterval.Units.Weeks:
          d    = EditorGUI.DoubleField(pos, propTicks.longValue * TimeInterval.TICKS2WK);
          edit = (long)(d / TimeInterval.TICKS2WK + (d >= 0 ? 0.5 : -0.5));
          break;
      }
      if (EditorGUI.EndChangeCheck())
      {
        propTicks.longValue = edit;
      }

      pos.x += pos.width + 2f;

      var prevUnit = m_Units;
      m_Units = (TimeInterval.Units)EditorGUI.EnumPopup(pos, m_Units);
      if (prevUnit != m_Units)
      {
        if (prevUnit == TimeInterval.Units.Frames)
        {
          propTicks.longValue = (propTicks.longValue * TimeInterval.SmoothTicksLastFrame).Rounded();
          propAsFrames.boolValue = false;
        }
        else if (m_Units == TimeInterval.Units.Frames)
        {
          propTicks.longValue = (propTicks.longValue / TimeInterval.SmoothTicksLastFrame).Rounded();
          propAsFrames.boolValue = true;
        }
      }

      OGUI.IndentLevel.Pop();
    }

  }
}
