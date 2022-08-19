/*! @file       Editor/OGUI.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-20
**/

using System.Collections.Generic;

using UnityEngine;
using UnityEditor;


namespace Ore.Editor
{

  public static class OGUI
  {

    public static float Indent => EditorGUI.indentLevel * STD_INDENT;

    // TODO this is defunct in reorderable lists!
    public static float LabelStartX => STD_INDENT_0 + EditorGUI.indentLevel * STD_INDENT;
    public static float LabelEndX => FieldStartX - STD_PAD;
    public static float LabelWidthRaw => LabelWidth.Stack.Front(fallback: EditorGUIUtility.labelWidth);
    public static float LabelWidthHalf => EditorGUIUtility.labelWidth * 0.45f;

    public static float FieldStartX => FieldStartXRaw - EditorGUI.indentLevel * STD_INDENT;
    public static float FieldStartXRaw => FieldEndX * 0.45f - STD_INDENT_0;
    public static float FieldEndX => EditorGUIUtility.currentViewWidth - STD_PAD_RIGHT;
    public static float FieldWidth => Mathf.Max(FieldEndX - FieldStartXRaw, EditorGUIUtility.fieldWidth);

    public static float ViewWidth => EditorGUIUtility.currentViewWidth;
    public static float ContentWidth => FieldEndX - LabelStartX;



    public const float STD_LINE_HEIGHT = 18f; // EditorGUIUtility.singleLineHeight
    public const float STD_LINE_HALF = STD_LINE_HEIGHT / 2f;

    public const float STD_INDENT_0 = 18f;
    public const float STD_INDENT = 15f;

    public const float STD_PAD = 2f; // == EditorGUIUtility.standardVerticalSpacing
    public const float STD_PAD_HALF = STD_PAD / 2f;
    public const float STD_PAD_RIGHT = STD_PAD * 2f;

    public const float STD_LINE_ADVANCE = STD_LINE_HEIGHT + STD_PAD;

    public const float STD_TOGGLE_W = STD_LINE_HEIGHT - 1f;
    public const float STD_TOGGLE_H = STD_LINE_HEIGHT + 1f;

    public const float STD_BTN_W = 14f;
    public const float STD_BTN_H = 12f;

    public const float MIN_TOGGLE_W = STD_TOGGLE_W - 2f;
    public const float MIN_TOGGLE_H = STD_TOGGLE_H - 2f;



    public static class LabelWidth
    {
      internal static List<float> Stack = new List<float>(4);

      public static float Peek()
      {
        return Stack.Back(fallback: EditorGUIUtility.labelWidth);
      }

      public static void Push(float width)
      {
        Stack.PushBack(EditorGUIUtility.labelWidth);
        EditorGUIUtility.labelWidth = width;
      }

      public static void Pop()
      {
        if (Stack.IsEmpty())
          EditorGUIUtility.labelWidth = -1f; // makes it use the default (150f)
        else
          EditorGUIUtility.labelWidth = Stack.PopBack();
      }

      public static void Reset()
      {
        if (Stack.IsEmpty())
          EditorGUIUtility.labelWidth = -1f; // makes it use the default (150f)
        else
        {
          EditorGUIUtility.labelWidth = Stack[0];
          Stack.Clear();
        }
      }
    } // end static class LabelWidth


    public static class LabelAlign
    {
      public static TextAnchor DEFAULT => Styles.Defaults.Label.alignment;

      internal static List<TextAnchor> Stack = new List<TextAnchor>(4);

      public static TextAnchor Peek()
      {
        return Stack.Back(fallback: DEFAULT);
      }

      public static void Push(TextAnchor align)
      {
        Stack.PushBack(EditorStyles.label.alignment);
        EditorStyles.label.alignment = align;
      }

      public static void Pop()
      {
        if (Stack.IsEmpty())
          EditorStyles.label.alignment = Stack.PopBack();
        else
          EditorStyles.label.alignment = DEFAULT;
      }

      public static void Reset()
      {
        if (Stack.IsEmpty())
          EditorStyles.label.alignment = DEFAULT;
        else
        {
          EditorStyles.label.alignment = Stack[0];
          Stack.Clear();
        }
      }

    } // end static class LabelAlign


    public static class IndentLevel
    {
      internal static List<int> Stack = new List<int>(4);

      public static void Push(int lvl, bool fix_label_width = true)
      {
        if (lvl < 0)
          lvl = 0;

        Stack.PushBack(EditorGUI.indentLevel);

        EditorGUI.indentLevel = lvl;

        if (fix_label_width)
          LabelWidth.Push(LabelWidthRaw - STD_INDENT * lvl);
      }

      public static void Pop(bool fix_label_width = true)
      {
        if (Stack.IsEmpty())
          EditorGUI.indentLevel = Stack.PopBack();
        else
        {
          EditorGUI.indentLevel = 0;
        }

        if (fix_label_width)
          LabelWidth.Pop();
      }

      public static void Reset()
      {
        Stack.Clear();
        EditorGUI.indentLevel = 0;
      }

      public static void Increase(bool fix_label_width = true, int delta = 1)
      {
        Push(EditorGUI.indentLevel + delta, fix_label_width);
      }

    } // end static class IndentLevel

  } // end static class OGUI

}
