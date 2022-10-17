/*! @file       Editor/PrefColor.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-10-
**/

using UnityEngine;
using UnityEditor;
using JetBrains.Annotations;


namespace Ore.Editor
{
  public class PrefColor
  {
    public Color32 Value
    {
      get => Load();
      set
      {
        m_Color = value;
        EditorPrefs.SetString(m_Key, ColorUtility.ToHtmlStringRGBA(m_Color));
        m_Loaded = true;
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

    public PrefColor([NotNull] string key, [NotNull] string htmlhex)
      : this(key, DEFAULT_COLOR)
    {
      if (ColorUtility.TryParseHtmlString(htmlhex, out Color c))
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


    private Color32 Load()
    {
      if (!m_Loaded)
      {
        var str = EditorPrefs.GetString(m_Key);
        if (ColorUtility.TryParseHtmlString(str, out Color c))
        {
          m_Color = c;
        }
        else
        {
          m_Color = DEFAULT_COLOR;
        }

        m_Loaded = true;
      }

      return m_Color;
    }

  } // end class PrefColor
}