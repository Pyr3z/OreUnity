/*! @file       Editor/TimeIntervalDrawer.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-11-08
**/

using UnityEngine;
using UnityEditor;

using Math = System.Math;


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
    };

    private const double TICKS2MS  = 1e-4;
    private const double TICKS2SEC = TICKS2MS  / 1000;
    private const double TICKS2MIN = TICKS2SEC / 60;
    private const double TICKS2HR  = TICKS2MIN / 60;
    private const double TICKS2DAY = TICKS2HR  / 24;

    private int m_Units = 2;


    public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
    {
      prop = prop.FindPropertyRelative(nameof(TimeInterval.Ticks));

      var pos = EditorGUI.PrefixLabel(total, label);

      OGUI.IndentLevel.Push(0);

      pos.width = (pos.width - 2f) / 2f;

      long edit;
      double d;

      EditorGUI.BeginChangeCheck();
      switch (m_Units)
      {
        default: // Ticks
          edit = EditorGUI.LongField(pos, prop.longValue);
          break;
        case 1:  // Milliseconds
          d    = EditorGUI.DoubleField(pos, prop.longValue * TICKS2MS);
          edit = (long)(d / TICKS2MS + (d >= 0 ? 0.5 : -0.5));
          break;
        case 2:  // Seconds
          d    = EditorGUI.DoubleField(pos, prop.longValue * TICKS2SEC);
          edit = (long)(d / TICKS2SEC + (d >= 0 ? 0.5 : -0.5));
          break;
        case 3:  // Minutes
          d    = EditorGUI.DoubleField(pos, prop.longValue * TICKS2MIN);
          edit = (long)(d / TICKS2MIN + (d >= 0 ? 0.5 : -0.5));
          break;
        case 4: // Hours
          d    = EditorGUI.DoubleField(pos, prop.longValue * TICKS2HR);
          edit = (long)(d / TICKS2HR + (d >= 0 ? 0.5 : -0.5));
          break;
        case 5: // Days
          d    = EditorGUI.DoubleField(pos, prop.longValue * TICKS2DAY);
          edit = (long)(d / TICKS2DAY + (d >= 0 ? 0.5 : -0.5));
          break;
      }
      if (EditorGUI.EndChangeCheck())
      {
        prop.longValue = edit;
      }

      pos.x += pos.width + 2f;
      m_Units = EditorGUI.Popup(pos, m_Units, s_Units);

      OGUI.IndentLevel.Pop();
    }

  }
}
