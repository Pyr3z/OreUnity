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
    private static readonly string[] UNITS =
    {
      "Frames",
      "Ticks",
      "Milliseconds",
      "Seconds",
      "Minutes",
      "Hours",
      "Days",
      "Weeks",
    };

    private const int DEFAULT_UNITS = 2; // "Seconds"

    // TODO class field will be shared in arrays of TimeIntervals (probs fine for now)
    private int m_Units = -1;


    public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
    {
      var propTicks = prop.FindPropertyRelative(nameof(TimeInterval.Ticks));
      var propAsFrames = prop.FindPropertyRelative("m_AsFrames");

      if (propAsFrames.boolValue)
      {
        m_Units = 0; // "Frames"
      }
      else if (m_Units <= 0)
      {
        m_Units = DetectUnits(propTicks.longValue);
      }

      var pos = EditorGUI.PrefixLabel(total, label);

      OGUI.IndentLevel.Push(0);

      pos.width = (pos.width - 2f) / 2f;

      long edit = 0L;
      double d;

      EditorGUI.BeginChangeCheck();
      switch (m_Units)
      {
        case 0:  // Frames
        case 1:  // Ticks
          edit = EditorGUI.LongField(pos, propTicks.longValue);
          break;

        case 2:  // Milliseconds
          d    = EditorGUI.DoubleField(pos, propTicks.longValue * TimeInterval.TICKS2MS);
          edit = (long)(d / TimeInterval.TICKS2MS + (d >= 0 ? 0.5 : -0.5));
          break;

        case 3:  // Seconds
          d    = EditorGUI.DoubleField(pos, propTicks.longValue * TimeInterval.TICKS2SEC);
          edit = (long)(d / TimeInterval.TICKS2SEC + (d >= 0 ? 0.5 : -0.5));
          break;

        case 4:  // Minutes
          d    = EditorGUI.DoubleField(pos, propTicks.longValue * TimeInterval.TICKS2MIN);
          edit = (long)(d / TimeInterval.TICKS2MIN + (d >= 0 ? 0.5 : -0.5));
          break;

        case 5:  // Hours
          d    = EditorGUI.DoubleField(pos, propTicks.longValue * TimeInterval.TICKS2HR);
          edit = (long)(d / TimeInterval.TICKS2HR + (d >= 0 ? 0.5 : -0.5));
          break;

        case 6:  // Days
          d    = EditorGUI.DoubleField(pos, propTicks.longValue * TimeInterval.TICKS2DAY);
          edit = (long)(d / TimeInterval.TICKS2DAY + (d >= 0 ? 0.5 : -0.5));
          break;

        case 7:  // Weeks
          d    = EditorGUI.DoubleField(pos, propTicks.longValue * TimeInterval.TICKS2WK);
          edit = (long)(d / TimeInterval.TICKS2WK + (d >= 0 ? 0.5 : -0.5));
          break;
      }
      if (EditorGUI.EndChangeCheck())
      {
        propTicks.longValue = edit;
      }

      pos.x += pos.width + 2f;

      int prevUnit = m_Units;
      m_Units = EditorGUI.Popup(pos, m_Units, UNITS);
      if (prevUnit != m_Units)
      {
        if (prevUnit == 0) // was "Frames"
        {
          propTicks.longValue = (propTicks.longValue * TimeInterval.SmoothTicksLastFrame).Rounded();
          propAsFrames.boolValue = false;
        }
        else if (m_Units == 0) // has become "Frames"
        {
          propTicks.longValue = (propTicks.longValue / TimeInterval.SmoothTicksLastFrame).Rounded();
          propAsFrames.boolValue = true;
        }
      }

      OGUI.IndentLevel.Pop();
    }


    private int DetectUnits(long ticks)
    {
      if (ticks < 0)
        ticks *= -1;

      if (ticks < TimeInterval.OfMillis(1).Ticks)
      {
        return 1; // Ticks
      }
      if (ticks < TimeInterval.OfSeconds(0.5).Ticks)
      {
        return 2; // Milliseconds
      }
      if (ticks < TimeInterval.OfMinutes(5).Ticks)
      {
        return 3; // Seconds
      }
      if (ticks < TimeInterval.OfHours(4).Ticks)
      {
        return 4; // Minutes
      }
      if (ticks < TimeInterval.OfDays(3).Ticks)
      {
        return 5; // Hours
      }

      return 6;   // Days
    }

  }
}
