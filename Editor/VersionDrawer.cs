/*! @file       Editor/VersionDrawer.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-12
**/

using System.Collections.Generic;

using UnityEngine;

using UnityEditor;


namespace Ore.Editor
{
  [CustomPropertyDrawer(typeof(VersionID))]
  public class VersionDrawer : PropertyDrawer
  {
    private VersionID m_ScratchVer = new VersionID(null);
    private List<(string str, int idx)> m_Parts = new List<(string str, int idx)>();

    public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
    {
      var str_prop = prop.FindPropertyRelative("m_String");

      string str = str_prop.stringValue;
      m_ScratchVer.Deserialize(str);

      int plen = m_ScratchVer.SplitParts(m_Parts);
      int ilen = m_ScratchVer.Length;

      if (ilen < 2 || ilen > 4)
      {
        EditorGUI.DelayedTextField(total, str_prop, label);
        return;
      }

      bool changed = false;

      var pos = new Rect(total)
      {
        height = OGUI.STD_LINE_HEIGHT
      };

      pos = EditorGUI.PrefixLabel(pos, label);
      pos.width = (pos.width - 2f * (ilen - 1)) / ilen;

      var tog = new Rect(pos)
      {
        width = OGUI.STD_TOGGLE_W * 2f,
        x = pos.x - 2f - OGUI.STD_TOGGLE_W * 2f
      };
      if (GUI.Button(tog, "(..)"))
      {
        prop.isExpanded = !prop.isExpanded;
        changed = true;
      }

      for (int p = 0; p < plen; ++p)
      {
        var (part, idx) = m_Parts[p];
        if (idx < 0)
          continue;

        EditorGUI.BeginChangeCheck();
        part = EditorGUI.DelayedTextField(pos, part);
        if (EditorGUI.EndChangeCheck() && !changed)
        {
          if (Parsing.TryParseInt32(part, out _ ) || idx == ilen - 1)
          {
            m_Parts[p] = (part, idx);
            changed = true;
          }
        }

        pos.x += pos.width + 2f;
      }

      if (changed)
      {
        var strb = new System.Text.StringBuilder();
        foreach (var (part, idx) in m_Parts)
        {
          strb.Append(part);
          if (idx >= 0 && idx < ilen - 1)
            strb.Append(VersionID.SEPARATOR);
        }

        m_ScratchVer.Deserialize(strb.ToString());

        if (m_ScratchVer.ToString() != str)
        {
          str_prop.stringValue = m_ScratchVer;
        }
      }
      else if (prop.isExpanded)
      {
        OGUI.IndentLevel.Increase();

        pos = new Rect(total)
        {
          yMin = total.yMax - OGUI.STD_LINE_HEIGHT
        };

        label.text = "(raw version string)";
        pos = EditorGUI.PrefixLabel(pos, label);
        EditorGUI.SelectableLabel(pos, m_ScratchVer);

        OGUI.IndentLevel.Pop();
      }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
      if (property.isExpanded)
        return OGUI.STD_LINE_HEIGHT + OGUI.STD_LINE_ADVANCE;
      return OGUI.STD_LINE_HEIGHT;
    }
  } // end class VersionDrawer
}