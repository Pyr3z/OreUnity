/*! @file       Editor/SerialSetDrawer.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-08-18
**/

using UnityEngine;
using UnityEditor;

namespace Ore.Editor
{
  [CustomPropertyDrawer(typeof(SerialSet<>), true)]
  internal class SerialSetDrawer : PropertyDrawer
  {

    public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
    {
      var child = prop.Copy();
      child.NextVisible(true);

      prop.isExpanded = child.isExpanded;

      OGUI.IndentLevel.Increase();

      if (!prop.isExpanded)
      {
        EditorGUI.PropertyField(total, child, label);
        OGUI.IndentLevel.Pop();
        return;
      }

      var rect = new Rect(total)
      {
        height = EditorGUI.GetPropertyHeight(child)
      };

      EditorGUI.PropertyField(rect, child, label);

      rect.y += rect.height - OGUI.STD_LINE_HEIGHT;
      rect.height = OGUI.STD_LINE_HEIGHT - 2f;
      rect.width = (rect.width - OGUI.STD_INDENT_0) / 2f - 2f;
      rect.x = OGUI.FieldStartX;

      if (GUI.Button(rect, "Trim Set"))
      {
        SerializedProperties.TryGetUnderlyingBoxedValue(prop, out object instance);

        var method = instance.GetType().GetMethod(nameof(StringSet.TrimSerialList));
        OAssert.NotNull(method, nameof(StringSet.TrimSerialList));

        method.Invoke(instance, System.Array.Empty<object>());
      }
      rect.x = total.x;
      rect.width = total.width;

      int depth = prop.depth;
      while (child.NextVisible(false) && child.depth != depth)
      {
        rect.y += rect.height + 2f;
        rect.height = EditorGUI.GetPropertyHeight(child);

        label.text = child.displayName;
        EditorGUI.PropertyField(rect, child, label);
      }

      OGUI.IndentLevel.Pop();
    }

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
      float height = OGUI.STD_LINE_ADVANCE;

      bool first = true;
      int depth = prop.depth;
      while (prop.NextVisible(first) && prop.depth != depth)
      {
        if (first && !prop.isExpanded)
          return height;
        height += EditorGUI.GetPropertyHeight(prop);
      }

      return height;
    }

  } // end class SerialSetDrawer
}