/*! @file       Editor/PrefColor.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-10-
**/

using UnityEngine;
using UnityEditor;
using JetBrains.Annotations;


namespace Ore.Editor
{
  [PublicAPI]
  public class PrefColor
  {
    public Color32 Value
    {
      get => Load();
      set
      {
        if (Colors.AreEqual(Load(), value))
          return;

        m_Color = value;
        EditorPrefs.SetString(m_Key, m_Color.ToHex());
      }
    }

    public static implicit operator Color32 ([CanBeNull] PrefColor pc)
    {
      if (pc is null)
        return DEFAULT_COLOR;

      return pc.Load();
    }

    public static implicit operator Color ([CanBeNull] PrefColor pc)
    {
      if (pc is null)
        return DEFAULT_COLOR;

      return pc.Load();
    }


    private static readonly Color32 DEFAULT_COLOR = new Color32(0xFF, 0x00, 0xFF, 0xFF);

    private readonly string m_Key;
    private Color32         m_Color;
    private bool            m_Loaded;


    public PrefColor([NotNull] string key)
      : this(key, DEFAULT_COLOR)
    {
    }

    public PrefColor([NotNull] string key, [NotNull] string htmlHex)
      : this(key, DEFAULT_COLOR)
    {
      if (Parsing.TryParseColor32(htmlHex, out Color32 c))
      {
        m_Color = c;
      }
    }

    public PrefColor([NotNull] string key, Color32 color)
    {
      m_Key    = key;
      m_Color  = color;
      m_Loaded = false;
    }

    private PrefColor()
    {
      // deleted default constructor
    }


    public void DrawLayout(string label)
    {
      if (label is null)
      {
        DrawLayout();
        return;
      }

      OGUI.ScratchContent.text = label;
      DrawLayout(OGUI.ScratchContent);
      OGUI.ScratchContent.text = string.Empty;
    }

    public void DrawLayout(GUIContent label = null)
    {
      EditorGUI.BeginChangeCheck();
      Color32 color = EditorGUILayout.ColorField(label ?? GUIContent.none, Value);
      if (EditorGUI.EndChangeCheck())
      {
        Value = color;
      }
    }


    private Color32 Load()
    {
      if (!m_Loaded)
      {
        var hex = EditorPrefs.GetString(m_Key);
        if (Parsing.TryParseColor32(hex, out Color32 c))
        {
          m_Color = c;
        }

        m_Loaded = true;
      }

      return m_Color;
    }

  } // end class PrefColor
}