/*! @file       Editor/CopyableFieldDrawer.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-01-24
**/

using UnityEngine;
using UnityEditor;


namespace Ore.Editor
{
  [CustomPropertyDrawer(typeof(CopyableFieldAttribute))]
  public class CopyableFieldDrawer : PropertyDrawer
  {

    private const float COPY_BTN_W = 3 * OGUI.STD_BTN_W;
    private const string FP_VALUE_FORMAT = Floats.RoundTripFormat;

    public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
    {
      var rect = new Rect(total)
      {
        width = total.width - COPY_BTN_W - 4f
      };

      EditorGUI.PropertyField(rect, prop, label);

      rect.x = rect.xMax + 2f;
      rect.xMax = total.xMax;

      label.text = "Copy";
      if (GUI.Button(rect, label, EditorStyles.miniButtonRight))
      {
        switch (prop.propertyType)
        {
        case SerializedPropertyType.String:
          GUIUtility.systemCopyBuffer = prop.stringValue;
          break;
        case SerializedPropertyType.Integer:
          GUIUtility.systemCopyBuffer = prop.intValue.ToInvariant();
          break;
        case SerializedPropertyType.Float:
          GUIUtility.systemCopyBuffer = prop.floatValue.ToInvariant();
          break;
        case SerializedPropertyType.Boolean:
          GUIUtility.systemCopyBuffer = prop.boolValue.ToInvariantLower();
          break;
        case SerializedPropertyType.Enum:
          GUIUtility.systemCopyBuffer = prop.enumNames[prop.enumValueIndex];
          break;
        case SerializedPropertyType.Color:
          GUIUtility.systemCopyBuffer = Colors.ToHex(prop.colorValue, prefix: "");
          break;
        case SerializedPropertyType.Vector2:
          GUIUtility.systemCopyBuffer = prop.vector2Value.ToString(FP_VALUE_FORMAT);
          break;
        case SerializedPropertyType.Vector3:
          GUIUtility.systemCopyBuffer = prop.vector3Value.ToString(FP_VALUE_FORMAT);
          break;
        case SerializedPropertyType.Vector4:
          GUIUtility.systemCopyBuffer = prop.vector4Value.ToString(FP_VALUE_FORMAT);
          break;
        case SerializedPropertyType.Quaternion:
          GUIUtility.systemCopyBuffer = prop.quaternionValue.eulerAngles.ToString(FP_VALUE_FORMAT);
          break;
        case SerializedPropertyType.Vector2Int:
          GUIUtility.systemCopyBuffer = prop.vector2IntValue.ToString();
          break;
        case SerializedPropertyType.Vector3Int:
          GUIUtility.systemCopyBuffer = prop.vector3IntValue.ToString();
          break;

        case SerializedPropertyType.LayerMask:
          int bits = prop.intValue;
          if (bits == 0)
          {
            GUIUtility.systemCopyBuffer = "Nothing";
          }
          else
          {
            // TODO this belongs in a separate runtime/editor util
            var bob = new System.Text.StringBuilder();

            while (bits != 0)
            {
              string layer = LayerMask.LayerToName(Bitwise.CTZ(bits));

              if (!layer.IsEmpty())
              {
                if (bob.Length > 0)
                  bob.Append(", ");

                bob.Append(layer);
              }

              bits = Bitwise.LSBye(bits);
            }

            GUIUtility.systemCopyBuffer = bob.ToString();
          }
          break;

        default:
          // TODO handle other value types? ObjectReference?
          break;
        }
      }
    }

  } // end class CopyableFieldDrawer
}