/** @file       Editor/Styles.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-29
 *  
 *  @brief      CHRIST for the love of god, don't let Levi
 *              go crazy in this file. KEEP IT SIMPLE STUPID!
**/

using UnityEngine;
using UnityEditor;


namespace Bore
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
        EditorStyles.label.richText = true;

        if (s_DelayDelayTried)
        {
          EditorApplication.update -= DelayInitialize;
        }
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


    public static class Dark
    {

      public static readonly Color32 Error              = Colors.FromHex("#CA2622FF");

      public static readonly Color32 Comment            = Colors.FromHex("#57A64AFF");

      public static readonly Color32 ReferenceTypeName  = Colors.FromHex("#4EC9B1FF");

      public static readonly Color32 ValueTypeName      = Colors.FromHex("#86C691FF");

    } // end static class Dark

  } // end static class Styles

}