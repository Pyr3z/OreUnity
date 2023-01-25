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
    private static string[] s_Units =
    {
      "Ticks",
      "Milliseconds",
      "Seconds",
      "Minutes",
      "Hours",
      "Days",
      "Frames",
    };

    private const int DEFAULT_UNITS = 2; // "Seconds"

    // TODO class field will be shared in arrays of TimeIntervals (probs fine for now)
    private int m_Units = DEFAULT_UNITS;


    public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
    {
      var propTicks = prop.FindPropertyRelative(nameof(TimeInterval.Ticks));
      var propAsFrames = prop.FindPropertyRelative("m_AsFrames");

      if (propAsFrames.boolValue)
      {
        m_Units = 6; // "Frames"
      }
      else if (m_Units == 6)
      {
        m_Units = DEFAULT_UNITS;
      }

      var pos = EditorGUI.PrefixLabel(total, label);

      OGUI.IndentLevel.Push(0);

      pos.width = (pos.width - 2f) / 2f;

      long edit = 0L;
      double d;

      EditorGUI.BeginChangeCheck();
      switch (m_Units)
      {
        case 0:  // Ticks
        case 6:  // Frames
          edit = EditorGUI.LongField(pos, propTicks.longValue);
          break;
        case 1:  // Milliseconds
          d    = EditorGUI.DoubleField(pos, propTicks.longValue * TimeInterval.TICKS2MS);
          edit = (long)(d / TimeInterval.TICKS2MS + (d >= 0 ? 0.5 : -0.5));
          break;
        case 2:  // Seconds
          d    = EditorGUI.DoubleField(pos, propTicks.longValue * TimeInterval.TICKS2SEC);
          edit = (long)(d / TimeInterval.TICKS2SEC + (d >= 0 ? 0.5 : -0.5));
          break;
        case 3:  // Minutes
          d    = EditorGUI.DoubleField(pos, propTicks.longValue * TimeInterval.TICKS2MIN);
          edit = (long)(d / TimeInterval.TICKS2MIN + (d >= 0 ? 0.5 : -0.5));
          break;
        case 4:  // Hours
          d    = EditorGUI.DoubleField(pos, propTicks.longValue * TimeInterval.TICKS2HR);
          edit = (long)(d / TimeInterval.TICKS2HR + (d >= 0 ? 0.5 : -0.5));
          break;
        case 5:  // Days
          d    = EditorGUI.DoubleField(pos, propTicks.longValue * TimeInterval.TICKS2DAY);
          edit = (long)(d / TimeInterval.TICKS2DAY + (d >= 0 ? 0.5 : -0.5));
          break;
      }
      if (EditorGUI.EndChangeCheck())
      {
        propTicks.longValue = edit;
      }

      pos.x += pos.width + 2f;

      int prevUnit = m_Units;
      m_Units = EditorGUI.Popup(pos, m_Units, s_Units);
      if (prevUnit != m_Units)
      {
        if (prevUnit == 6) // was "Frames"
        {
          propTicks.longValue = (propTicks.longValue * TimeInterval.SmoothTicksLastFrame).Rounded();
          propAsFrames.boolValue = false;
        }
        else if (m_Units == 6) // has become "Frames"
        {
          propTicks.longValue = (propTicks.longValue / TimeInterval.SmoothTicksLastFrame).Rounded();
          propAsFrames.boolValue = true;
        }
      }

      OGUI.IndentLevel.Pop();
    }

  }
}
