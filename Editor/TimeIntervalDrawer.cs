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

    // class field will be shared in arrays of TimeIntervals (fine to keep for now)
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
          d    = EditorGUI.DoubleField(pos, prop.longValue * TimeInterval.TICKS2MS);
          edit = (long)(d / TimeInterval.TICKS2MS + (d >= 0 ? 0.5 : -0.5));
          break;
        case 2:  // Seconds
          d    = EditorGUI.DoubleField(pos, prop.longValue * TimeInterval.TICKS2SEC);
          edit = (long)(d / TimeInterval.TICKS2SEC + (d >= 0 ? 0.5 : -0.5));
          break;
        case 3:  // Minutes
          d    = EditorGUI.DoubleField(pos, prop.longValue * TimeInterval.TICKS2MIN);
          edit = (long)(d / TimeInterval.TICKS2MIN + (d >= 0 ? 0.5 : -0.5));
          break;
        case 4: // Hours
          d    = EditorGUI.DoubleField(pos, prop.longValue * TimeInterval.TICKS2HR);
          edit = (long)(d / TimeInterval.TICKS2HR + (d >= 0 ? 0.5 : -0.5));
          break;
        case 5: // Days
          d    = EditorGUI.DoubleField(pos, prop.longValue * TimeInterval.TICKS2DAY);
          edit = (long)(d / TimeInterval.TICKS2DAY + (d >= 0 ? 0.5 : -0.5));
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
