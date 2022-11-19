/*! @file       Editor/FoldoutHeader.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-20
**/

using UnityEngine;
using UnityEditor;


namespace Ore.Editor
{

  public class FoldoutHeader : GUI.Scope
  {
    const bool FIX_LABEL_WIDTH = false;

    public static bool Open(Rect               total,
                            GUIContent         content,
                            SerializedProperty prop,
                        out FoldoutHeader      header,
                            bool               isDisabled = false,
                            int                indent     = -1)
    {
      return prop.isExpanded = header =
        new FoldoutHeader(total, content, indent, prop.isExpanded, prop.IsArrayElement(), isDisabled);
    }

    public static FoldoutHeader Open(Rect               total,
                                     GUIContent         content,
                                     SerializedProperty prop,
                                     bool               isDisabled = false,
                                     int                indent     = -1)
    {
      var header = new FoldoutHeader(total, content, indent, prop.isExpanded, prop.IsArrayElement(), isDisabled);

      prop.isExpanded = header.IsOpen;

      return header;
    }


    public static implicit operator bool(FoldoutHeader fh)
    {
      return fh != null && fh.IsOpen;
    }


    public Rect Rect;
    public readonly int Indent;
    public readonly bool IsOpen, IsVanilla, IsListElement, IsDisabled;


    private FoldoutHeader(Rect pos, GUIContent content, int indent, bool isOpen, bool isListElm, bool isDisabled)
    {
      if ((IsListElement = isListElm) == true)
      {
        pos.xMin += 5f;
        OGUI.LabelWidth.Push(EditorGUIUtility.labelWidth - 8f);
        indent -= 2;
      }

      Rect = pos;

      if (indent > 0)
      {
        OGUI.IndentLevel.Push(indent, FIX_LABEL_WIDTH);
        pos.xMin += (indent-1) * OGUI.STD_INDENT;
      }

      Indent = indent;
      IsVanilla = true;

      IsOpen = EditorGUI.BeginFoldoutHeaderGroup(pos, isOpen, content);

      EditorGUI.BeginDisabledGroup(IsDisabled = isDisabled);
    }


    protected override void CloseScope()
    {
      if (IsVanilla)
        EditorGUI.EndFoldoutHeaderGroup();
      else
        GUILayout.EndVertical();

      if (Indent > 0)
        OGUI.IndentLevel.Pop(FIX_LABEL_WIDTH);

      if (IsListElement)
        OGUI.LabelWidth.Pop();

      EditorGUI.EndDisabledGroup();
    }

  } // end class FoldoutHeader

}
