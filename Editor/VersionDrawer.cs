/*! @file       Editor/VersionDrawer.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-12
**/

// ReSharper disable CognitiveComplexity

using System.Collections.Generic;

using UnityEngine;
using UnityEditor;


namespace Ore.Editor
{
  [CustomPropertyDrawer(typeof(SerialVersion))]
  public class VersionDrawer : PropertyDrawer
  {
    private readonly SerialVersion m_ScratchVer = new SerialVersion();
    private readonly List<(string str, int idx)> m_Parts = new List<(string str, int idx)>();

    private const int MAX_SPLIT_COMPONENTS = 5;

    public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
    {
      var str_prop = prop.FindPropertyRelative("m_String");

      string str = str_prop.stringValue;

      if (!GUI.enabled)
      {
        GUI.enabled = true;
        total = EditorGUI.PrefixLabel(total, label);
        EditorGUI.SelectableLabel(total, str);
        GUI.enabled = false;
        return;
      }

      m_ScratchVer.Deserialize(str);

      int ilen = m_ScratchVer.Length;

      if (ilen < 2 || ilen > MAX_SPLIT_COMPONENTS)
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

      int plen = m_ScratchVer.SplitParts(m_Parts);
      for (int p = 0; p < plen; ++p)
      {
        var (part, idx) = m_Parts[p];
        if (idx < 0)
          continue;

        EditorGUI.BeginChangeCheck();
        part = EditorGUI.DelayedTextField(pos, part);
        if (EditorGUI.EndChangeCheck() && !changed)
        {
          if (Parsing.TryParseInt32(part, out _ ))
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
            strb.Append(SerialVersion.SEPARATOR);
        }

        m_ScratchVer.Deserialize(strb.ToString());

        if (m_ScratchVer.ToString() != str)
        {
          str_prop.stringValue = m_ScratchVer;
        }
      }
      else if (prop.isExpanded)
      {
        OGUI.IndentLevel.Increase(fixLabelWidth: false);

        pos = new Rect(total)
        {
          y = total.yMax - OGUI.STD_LINE_HEIGHT - OGUI.STD_LINE_ADVANCE,
          height = OGUI.STD_LINE_HEIGHT
        };

        label.text = "Raw string:";

        EditorGUI.BeginChangeCheck();
        str = EditorGUI.DelayedTextField(pos, label, m_ScratchVer);
        if (EditorGUI.EndChangeCheck())
        {
          str_prop.stringValue = str;
        }

        pos.y += OGUI.STD_LINE_ADVANCE;
        label.text = "Hash:";

        pos = EditorGUI.PrefixLabel(pos, label);

        OGUI.IndentLevel.Pop(fixLabelWidth: false);

        EditorGUI.SelectableLabel(pos, m_ScratchVer.GetHashCode().ToHexString("0x"));
      }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
      if (GUI.enabled && property.isExpanded)
        return OGUI.STD_LINE_HEIGHT + OGUI.STD_LINE_ADVANCE * 2;
      return OGUI.STD_LINE_HEIGHT;
    }
  } // end class VersionDrawer
}