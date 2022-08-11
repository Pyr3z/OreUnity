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


    public static string ColorText(string text, Color32 color)
    {
      // TODO RichText class
      return $"<color=#{color.ToHex()}>{text}</color>";
    }


    public static class Defaults
    {
      public static GUIStyle Label = null;

      public static float LabelWidth = 150f;
    }

    public static class Dark
    {

      public static readonly Color32 Error = Colors.FromHex("#CA2622FF");

      public static readonly Color32 Comment = Colors.FromHex("#57A64AFF");

      public static readonly Color32 ReferenceTypeName = Colors.FromHex("#4EC9B1FF");

      public static readonly Color32 ValueTypeName = Colors.FromHex("#86C691FF");

    } // end static class Dark

  } // end static class Styles

}