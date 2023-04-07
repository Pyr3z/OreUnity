/*! @file       Editor/AssetInspector.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-04-06
**/

using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

using Type = System.Type;


namespace Ore.Editor
{

  [CustomEditor(typeof(Asset), editorForChildClasses: true)]
  [CanEditMultipleObjects]
  public class AssetInspector : UnityEditor.Editor
  {
    struct InspectionCache
    {
      public int    IID;
      public Asset  Asset;
      public Type   Type;
      public string Path;

      public void Update(Asset asset)
      {
        if (!asset)
        {
          IID   = 0;
          Asset = null;
          Type  = null;
          Path  = string.Empty;
          return;
        }

        int iid = asset.GetInstanceID();
        if (IID == iid)
          return;

        IID   = iid;
        Asset = asset;
        Type  = asset.GetType();
        Path  = AssetDatabase.GetAssetPath(asset);
      }
    }

    InspectionCache m_Cache;


    protected override void OnHeaderGUI()
    {
      base.OnHeaderGUI(); // TODO

      m_Cache.Update(target as Asset);
    }

    public override void OnInspectorGUI()
    {
      bool ok = target == m_Cache.Asset;
      OAssert.True(ok, "target == s_Cache.Asset");

      var sobj = serializedObject;
      sobj.Update();

      var currProp = sobj.GetIterator();

      ok = currProp.NextVisible(true) && currProp.propertyPath == "m_Script";
      OAssert.True(ok, "1st propertyPath == \"m_Script\"");

      // now, draw:
      using (new LocalizationGroup(m_Cache.Type))
      {
        _ = IterateProperties(currProp);
      }
    }

    bool IterateProperties(SerializedProperty propIter, string terminalPropName = null)
    {
      try
      {
        int  baseIndent = EditorGUI.indentLevel;
        bool drill      = false;

        while (propIter.NextVisible(drill))
        {
          EditorGUI.indentLevel = baseIndent + propIter.depth;

          if (propIter.propertyType != SerializedPropertyType.String && propIter.isArray)
          {
            #if UNITY_2020_1_OR_NEWER

            if (propIter.IsNonReorderable())
            {
              drill = EditorGUILayout.PropertyField(propIter, includeChildren: false);

              if (drill)
              {
                var arrayIter = propIter.Copy();
                propIter.NextVisible(enterChildren: true);
                DoArrayHeader(propIter, arrayIter);
                drill = false;
              }
              else
              {
                // still draw when closed
                DoArrayHeader(propIter.FindPropertyRelative("Array.size"), propIter);
              }
            }
            else // handle reorderable list
            {
              drill = EditorGUILayout.PropertyField(propIter, includeChildren: true);

              if (propIter.IsReadOnly())
              {
                FreezeReorderableList(propIter, freeze: true);
              }
            }

            #else // if Unity < 2020

            drill = EditorGUILayout.PropertyField(propIter, includeChildren: false, 
                                                  GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
            if (drill)
            {
              var propArray = propIter.Copy();
              propIter.NextVisible(enterChildren: true);
              DoArrayHeader(propIter, propArray);
              drill = false;
            }
            else
            {
              // still draw when closed
              DoArrayHeader(propIter.FindPropertyRelative("Array.size"), propIter);
            }

            #endif
          }
          else
          {
            drill = EditorGUILayout.PropertyField(propIter, includeChildren: false);
          }

          if (propIter.name == terminalPropName)
            return false;
        } // end while loop

        return true;
      }
      catch (System.Exception ex)
      {
        Orator.NFE(ex);
        return false;
      }
      finally
      {
        OGUI.LabelWidth.Reset();
        OGUI.IndentLevel.Reset();
      }
    }

    static void DoArrayHeader(SerializedProperty propSize, SerializedProperty propElements)
    {
      var pos = GUILayoutUtility.GetLastRect();
      pos.yMin = pos.yMax - OGUI.STD_LINE_HEIGHT;


      pos.x     = OGUI.FieldStartX;
      pos.width = OGUI.FieldWidth * 0.45f;

      if (propElements.isExpanded)
      {
        if (GUI.Button(pos, "Add"))
        {
          propElements.InsertArrayElementAtIndex(propElements.arraySize);
        }
      }

      pos.x    += pos.width + OGUI.STD_PAD;
      pos.xMax =  OGUI.FieldEndX;

      var label = OGUI.ScratchContent;

      label.text = "Count: ";
      
      OGUI.IndentLevel.Push(0, fixLabelWidth: false);
      OGUI.LabelWidth.Push(EditorStyles.label.CalcSize(label).x + OGUI.STD_PAD_RIGHT);

      _ = EditorGUI.PropertyField(pos, propSize, label, includeChildren: false);

      OGUI.LabelWidth.Pop();
      OGUI.IndentLevel.Pop();
    }

    static void FreezeReorderableList(SerializedProperty propList, bool freeze)
    {
      // TODO
    }

  } // end class AssetInspector

}