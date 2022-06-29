/** @file       Editor/Drawers.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-20
**/

using System.Collections.Generic;

using UnityEngine;
using UnityEditor;


namespace Bore
{

  public static class Drawers
  {

    public static float Indent          => EditorGUI.indentLevel * STD_INDENT;

    // TODO this is defunct in reorderable lists!
    public static float LabelStartX     => STD_INDENT_0 + EditorGUI.indentLevel * STD_INDENT;
    public static float LabelEndX       => FieldStartX - STD_PAD;
    public static float LabelWidth      => EditorGUIUtility.labelWidth;
    public static float LabelWidthRaw   => s_LabelWidthStack.Front(fallback: EditorGUIUtility.labelWidth);
    public static float LabelWidthHalf  => LabelWidth * 0.45f;

    public static float FieldStartX     => FieldStartXRaw;
    public static float FieldStartXRaw  => FieldEndX * 0.45f - STD_INDENT_0;
    public static float FieldEndX       => EditorGUIUtility.currentViewWidth - STD_PAD_RIGHT;
    public static float FieldWidth      => Mathf.Max(FieldEndX - FieldStartXRaw, EditorGUIUtility.fieldWidth);

    public static float ViewWidth       => EditorGUIUtility.currentViewWidth;
    public static float ContentWidth    => FieldEndX - LabelStartX;



    public const float STD_LINE_HEIGHT  = 18f; // EditorGUIUtility.singleLineHeight
    public const float STD_LINE_HALF    = STD_LINE_HEIGHT / 2f;

    public const float STD_INDENT_0     = 18f;
    public const float STD_INDENT       = 15f;

    public const float STD_PAD          = 2f; // == EditorGUIUtility.standardVerticalSpacing
    public const float STD_PAD_HALF     = STD_PAD / 2f;
    public const float STD_PAD_RIGHT    = STD_PAD * 2f;

    public const float STD_LINE_ADVANCE = STD_LINE_HEIGHT + STD_PAD;

    public const float STD_TOGGLE_W     = STD_LINE_HEIGHT - 1f;
    public const float STD_TOGGLE_H     = STD_LINE_HEIGHT + 1f;

    public const float STD_BTN_W        = 14f;
    public const float STD_BTN_H        = 12f;

    public const float MIN_TOGGLE_W     = STD_TOGGLE_W - 2f;
    public const float MIN_TOGGLE_H     = STD_TOGGLE_H - 2f;



    private static List<float> s_LabelWidthStack = new List<float>();
    public static void PushLabelWidth(float width)
    {
      s_LabelWidthStack.PushBack(EditorGUIUtility.labelWidth);
      EditorGUIUtility.labelWidth = width;
    }
    public static void PopLabelWidth()
    {
      if (s_LabelWidthStack.Count > 0)
      {
        EditorGUIUtility.labelWidth = s_LabelWidthStack.PopBack();
      }
    }
    public static void ResetLabelWidth()
    {
      if (s_LabelWidthStack.Count > 0)
      {
        EditorGUIUtility.labelWidth = s_LabelWidthStack[0];
        s_LabelWidthStack.Clear();
      }
    }


    private static List<TextAnchor> s_LabelAlignmentStack = new List<TextAnchor>();
    public static void PushLabelAlign(TextAnchor align)
    {
      s_LabelAlignmentStack.PushBack(EditorStyles.label.alignment);
      EditorStyles.label.alignment = /* Styles.Label.alignment = */ align;
    }
    public static void PopLabelAlign()
    {
      if (s_LabelAlignmentStack.Count > 0)
      {
        EditorStyles.label.alignment = /* Styles.Label.alignment = */ s_LabelAlignmentStack.PopBack();
      }
      else
      {
        EditorStyles.label.alignment = /* Styles.Label.alignment = */ TextAnchor.MiddleLeft;
      }
    }


    private static List<int> s_IndentLvlStack = new List<int>();
    public static void PushIndentLevel(int lvl, bool fix_label_width = true)
    {
      if (lvl < 0)
        lvl = 0;

      s_IndentLvlStack.PushBack(EditorGUI.indentLevel);

      EditorGUI.indentLevel = lvl;

      if (fix_label_width)
        PushLabelWidth(LabelWidthRaw - STD_INDENT * lvl);
    }
    public static void PopIndentLevel(bool fix_label_width = true)
    {
      if (s_IndentLvlStack.Count > 0)
      {
        EditorGUI.indentLevel = s_IndentLvlStack.PopBack();

        if (fix_label_width)
          PopLabelWidth();
      }
    }
    public static void ResetIndentLevel()
    {
      s_IndentLvlStack.Clear();
      EditorGUI.indentLevel = 0;
    }
    public static void PushNextIndentLevel(bool fix_label_width = true, int delta = 1)
    {
      PushIndentLevel(EditorGUI.indentLevel + delta, fix_label_width);
    }

  } // end static class Drawers

}
