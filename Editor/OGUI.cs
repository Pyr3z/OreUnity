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
    public static readonly GUIContent ScratchContent = new GUIContent();


    public static float Indent => EditorGUI.indentLevel * STD_INDENT;

    // TODO this is defunct in reorderable lists!
    public static float LabelStartX => STD_INDENT_0 + EditorGUI.indentLevel * STD_INDENT;
    public static float LabelEndX => FieldStartX - STD_PAD;
    public static float LabelWidthBase => LabelWidth.Stack.Front(fallback: EditorGUIUtility.labelWidth);
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


    public static void SliderPlus(string label, ref float current, float min, ref float max)
    {
      SliderPlus(
        EditorGUILayout.GetControlRect(hasLabel: false, STD_LINE_HEIGHT),
        label,
        ref current,
        min,
        ref max
      );
    }

    public static void SliderPlus(string label, ref float current, float min, float max)
    {
      SliderPlus(
        EditorGUILayout.GetControlRect(hasLabel: false, STD_LINE_HEIGHT),
        label,
        ref current,
        min,
        max
      );
    }

    public static void SliderPlus(string label, ref int current, int min, ref int max)
    {
      SliderPlus(
        EditorGUILayout.GetControlRect(hasLabel: false, STD_LINE_HEIGHT),
        label,
        ref current,
        min,
        ref max
      );
    }

    public static void SliderPlus(Rect pos, string label, ref float current, float min, float max)
    {
      pos.width -= 54f;
      current = EditorGUI.Slider(pos, label, current, min, max);
    }

    public static void SliderPlus(Rect pos, string label, ref float current, float min, ref float max)
    {
      const float kPlusFieldW   = 64f;
      const float kMarginAdjust = 10f;

      if (!label.IsEmpty())
      {
        ScratchContent.text = label;
        pos = EditorGUI.PrefixLabel(pos, OGUI.ScratchContent);
      }

      pos.xMin -= Indent;
      pos.width -= kPlusFieldW - kMarginAdjust;
      current = EditorGUI.Slider(pos, current, min, max);

      pos.x += pos.width - kMarginAdjust;
      pos.width = kPlusFieldW;
      max = EditorGUI.DelayedFloatField(pos, max);
    }

    public static void SliderPlus(Rect pos, string label, ref int current, int min, ref int max)
    {
      const float kPlusFieldW   = 64f;
      const float kMarginAdjust = 10f;

      if (!label.IsEmpty())
      {
        ScratchContent.text = label;
        pos                 = EditorGUI.PrefixLabel(pos, OGUI.ScratchContent);
      }

      pos.xMin  -= Indent;
      pos.width -= kPlusFieldW - kMarginAdjust;
      current   =  EditorGUI.IntSlider(pos, current, min, max);

      pos.x     += pos.width - kMarginAdjust;
      pos.width =  kPlusFieldW;
      max       =  EditorGUI.DelayedIntField(pos, max);
    }


    public static class Draw
    {

      public static void Separator(float yOffset = STD_LINE_HALF)
      {
        Separator(Colors.Boring, yOffset);
      }

      public static void Separator(Color32 lineColor, float yOffset = STD_LINE_HALF)
      {
        var pos = GUILayoutUtility.GetRect(ContentWidth, yOffset * 2f);

        float y = pos.y + yOffset;

        if (EditorGUI.indentLevel > 0)
        {
          Line(
            p0:    new Vector2(pos.xMin, y),
            p1:    new Vector2(pos.xMax, y),
            color: lineColor
          );
        }
        else
        {
          Line(
            p0:    new Vector2(pos.xMin - STD_INDENT_0 + STD_PAD, y),
            p1:    new Vector2(pos.xMax, y),
            color: lineColor
          );
        }
      }

      public static void FillBar(Rect pos, Color32 fill, float t, Color32 textColor = default)
      {
        if (Event.current.type != EventType.Repaint)
          return;

        Rect(pos, outline: Colors.Dark);

        var fillRect = new Rect(pos.x, pos.y, pos.width * t, pos.height);

        Rect(fillRect, outline: Colors.Dark, fill.Alpha(0.5f));

        if (textColor.IsClear())
          return;

        string text = $"{(int)(t * 100f + 0.5f):0}%";
        text = Styles.ColorText(text, textColor);
        text = Styles.BigText(text);

        LabelAlign.Push(TextAnchor.MiddleCenter);
        GUI.Label(pos, text);
        LabelAlign.Pop();
      }

      public static void Rect(Rect pos, Color32 outline = default, Color32 fill = default, bool always = false)
      {
        if (!always && Event.current.type != EventType.Repaint)
          return;

        Handles.BeginGUI();

        using (new Handles.DrawingScope(Color.white))
        {
          if (outline.IsDefault())
            Handles.DrawSolidRectangleWithOutline(pos, fill, Colors.Bright);
          else
            Handles.DrawSolidRectangleWithOutline(pos, fill, outline);
        }

        Handles.EndGUI();
      }

      public static void Line(Vector2 p0, Vector2 p1, Color32 color, bool always = false)
      {
        if (!always && (Event.current.type != EventType.Repaint || color.IsClear()))
          return;

        Handles.BeginGUI();

        using (new Handles.DrawingScope(color))
        {
          Handles.DrawLine(p0, p1);
        }

        Handles.EndGUI();
      }

    } // end static class Draw


    public static class LabelWidth
    {
      internal static readonly List<float> Stack = new List<float>(4);

      public static float Peek()
      {
        return Stack.Back(fallback: EditorGUIUtility.labelWidth);
      }

      public static void Push(float width)
      {
        Stack.PushBack(EditorGUIUtility.labelWidth);
        EditorGUIUtility.labelWidth = width;
      }

      public static void PushDelta(float delta)
      {
        Push(EditorGUIUtility.labelWidth + delta);
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


      private static readonly List<TextAnchor> s_AlignStack = new List<TextAnchor>(4);


      public static TextAnchor Peek()
      {
        return s_AlignStack.Back(fallback: DEFAULT);
      }

      public static void Push(TextAnchor align)
      {
        s_AlignStack.PushBack(EditorStyles.label.alignment);
        EditorStyles.label.alignment = align;
      }

      public static void Pop()
      {
        if (s_AlignStack.IsEmpty())
          EditorStyles.label.alignment = DEFAULT;
        else
          EditorStyles.label.alignment = s_AlignStack.PopBack();
      }

      public static void Reset()
      {
        if (s_AlignStack.IsEmpty())
          EditorStyles.label.alignment = DEFAULT;
        else
        {
          EditorStyles.label.alignment = s_AlignStack[0];
          s_AlignStack.Clear();
        }
      }

    } // end static class LabelAlign


    public static class IndentLevel
    {
      private const bool FIX_LABEL_WIDTH_DEFAULT = false;

      private static readonly List<int> s_IndentStack = new List<int>(4);


      public static void Push(int lvl, bool fixLabelWidth = FIX_LABEL_WIDTH_DEFAULT)
      {
        if (lvl < 0)
          lvl = 0;

        s_IndentStack.PushBack(EditorGUI.indentLevel);

        EditorGUI.indentLevel = lvl;

        if (fixLabelWidth)
          LabelWidth.Push(LabelWidthBase - STD_INDENT * lvl);
      }

      public static void PushDelta(int delta, bool fixLabelWidth = FIX_LABEL_WIDTH_DEFAULT)
      {
        Push(EditorGUI.indentLevel + delta, fixLabelWidth);
      }

      public static void Increase(bool fixLabelWidth = FIX_LABEL_WIDTH_DEFAULT)
      {
        Push(EditorGUI.indentLevel + 1, fixLabelWidth);
      }

      public static void Pop(bool fixLabelWidth = FIX_LABEL_WIDTH_DEFAULT)
      {
        if (s_IndentStack.IsEmpty())
          EditorGUI.indentLevel = s_IndentStack.PopBack();
        else
        {
          EditorGUI.indentLevel = 0;
        }

        if (fixLabelWidth)
          LabelWidth.Pop();
      }

      public static void Reset()
      {
        s_IndentStack.Clear();
        EditorGUI.indentLevel = 0;
      }

    } // end static class IndentLevel

  } // end static class OGUI

}
