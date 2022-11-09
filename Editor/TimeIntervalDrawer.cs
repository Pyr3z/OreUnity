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
      "Minutes"
    };

    private int m_Units = 2;


    public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
    {
      prop = prop.FindPropertyRelative(nameof(TimeInterval.Ticks));

      var pos = EditorGUI.PrefixLabel(total, label);

      OGUI.IndentLevel.Push(0);

      pos.width = (pos.width - 2f) / 2f;

      long edit;
      EditorGUI.BeginChangeCheck();
      switch (m_Units)
      {
        default: // Ticks
          edit = EditorGUI.LongField(pos, prop.longValue);
          break;
        case 1:  // Milliseconds
          edit = (long)(EditorGUI.DoubleField(pos, prop.longValue * 1e-4) * 1e4);
          break;
        case 2:  // Seconds
          edit = (long)(EditorGUI.DoubleField(pos, prop.longValue * 1e-7) * 1e7);
          break;
        case 3:  // Minutes
          edit = (long)(EditorGUI.DoubleField(pos, prop.longValue * 1.6666666666666667e-9) * 1.6666666666666667e9);
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
