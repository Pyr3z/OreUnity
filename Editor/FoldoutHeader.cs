/** @file       Editor/FoldoutHeader.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-20
**/

using UnityEngine;
using UnityEditor;


namespace Bore
{

  public class FoldoutHeader : GUI.Scope
  {
    const bool FIX_LABEL_WIDTH = false;

    public static bool Open(Rect total, GUIContent content, SerializedProperty prop, out FoldoutHeader header, int indent = -1)
    {
      return prop.isExpanded = header = new FoldoutHeader(total, content, prop.isExpanded, prop.IsArrayElement(), indent);
    }

    public static implicit operator bool (FoldoutHeader fh)
    {
      return fh != null && fh.IsOpen;
    }


    public Rect Rect;
    public readonly bool  IsOpen, IsVanilla, IsListElement;
    public readonly int   Indent;


    private FoldoutHeader(Rect pos, GUIContent content, bool is_open, bool is_list_el, int indent)
    {
      if (is_list_el)
      {
        pos.xMin += 5f;
        OGUI.LabelWidth.Push(EditorGUIUtility.labelWidth - 8f);
        indent -= 2;
      }

      if (indent > 0)
      {
        OGUI.PushIndentLevel(indent, FIX_LABEL_WIDTH);
      }

      Indent        = indent;
      Rect          = pos;
      IsVanilla     = true;
      IsListElement = is_list_el;
      IsOpen        = EditorGUI.BeginFoldoutHeaderGroup(pos, is_open, content);
    }


    protected override void CloseScope()
    {
      if (IsVanilla)
        EditorGUI.EndFoldoutHeaderGroup();
      else
        GUILayout.EndVertical();

      if (Indent > 0)
        OGUI.PopIndentLevel(FIX_LABEL_WIDTH);

      if (IsListElement)
        OGUI.LabelWidth.Pop();
    }

  } // end class FoldoutHeader

}
