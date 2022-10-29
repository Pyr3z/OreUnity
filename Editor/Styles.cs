/*! @file       Editor/Styles.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-29
 *
 *  @brief      CHRIST for the love of god, don't let Levi
 *              go crazy in this file. KEEP IT SIMPLE STUPID!
**/

using UnityEngine;
using UnityEditor;


namespace Ore.Editor
{

  [InitializeOnLoad]
  public static class Styles
  {
    public static class Defaults
    {

      public static GUIStyle Label         = null;

      public static int      LabelFontSize = 10;

      public static float    LabelWidth    = 150f;

      public static float    FieldWidth    = 50f;

    } // end static class Defaults


    public static string ColorText(string text, Color32 color)
    {
      return $"<color=#{color.ToHex()}>{text}</color>";
    }

    public static string BigText(string text, int size = 14)
    {
      return $"<size={size}>{text}</size>";
    }

    public static string BoldText(string text)
    {
      return $"<b>{text}</b>";
    }



    static Styles()
    {
      EditorApplication.delayCall += DelayInitialize;
    }

    private static bool s_DelayDelayTried = false;
    private static void DelayInitialize()
    {
      try
      {
        OAssert.NotNull(EditorStyles.label);

        Defaults.Label = new GUIStyle(EditorStyles.label);

        EditorStyles.label.richText = true;

        // get compiled default label width
        float restore_lw = EditorGUIUtility.labelWidth;
        bool restore_hm = EditorGUIUtility.hierarchyMode;
        EditorGUIUtility.labelWidth = -1f;
        EditorGUIUtility.hierarchyMode = false;

        Defaults.LabelWidth = EditorGUIUtility.labelWidth;
        Defaults.LabelFontSize = Defaults.Label.fontSize;

        EditorGUIUtility.labelWidth = restore_lw;
        EditorGUIUtility.hierarchyMode = restore_hm;

        if (s_DelayDelayTried)
          EditorApplication.update -= DelayInitialize;
      }
      catch (System.NullReferenceException ex)
      {
        if (!s_DelayDelayTried)
        {
          EditorApplication.update += DelayInitialize;
          s_DelayDelayTried = true;
        }
        else
        {
          throw ex;
        }
      }
    }

  } // end static class Styles

}