/** @file       Editor/EventDrawer.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-14
**/

using UnityEngine;
using UnityEditor;


namespace Bore
{

  [CustomPropertyDrawer(typeof(IEvent),       useForChildren: true)] // shit, doesn't work..
  [CustomPropertyDrawer(typeof(DelayedEvent), useForChildren: true)] // ... gotta be explicit.
  internal class EventDrawer : UnityEditorInternal.UnityEventDrawer
  {
    private const string UNITYEVENT_LAST_PROPERTY = "m_PersistentCalls";
    private const string LABEL_SUFFIX_DISABLED = " (event disabled)";
    
    private const float STD_PAD           = InspectorDrawers.STD_PAD;
    private const float STD_LINE_HEIGHT   = InspectorDrawers.STD_LINE_HEIGHT;
    private const float STD_PAD_HALF      = STD_PAD / 2f;
    private const float UNEXPANDED_HEIGHT = STD_LINE_HEIGHT + STD_PAD;

    internal class DrawerState : PropertyDrawerState
    {
      public IEvent Event;
      public int    ChildCount;
      public float  ExtraHeight;
      public string EventLabel;

      protected override void UpdateDetails()
      {
        _ = m_RootProp.TryGetUnderlyingValue(out Event);

        ChildCount = 0;
        ExtraHeight = UNEXPANDED_HEIGHT;

        var iterator = m_RootProp.FindPropertyRelative(UNITYEVENT_LAST_PROPERTY);

        while (iterator.NextVisible(false) &&
               iterator.depth == m_RootProp.depth + 1 &&
               iterator.propertyPath.StartsWith(m_RootProp.propertyPath))
        {
          ExtraHeight += EditorGUI.GetPropertyHeight(iterator, iterator.isExpanded) + STD_PAD;
          ++ChildCount;
        }

        EventLabel = $"{Event.GetType().Name}: {m_RootProp.displayName}";
      }

      public void UpdateExtraHeight()
      {
        IsStale = false;

        if (ChildCount == 0)
          return;

        ExtraHeight = UNEXPANDED_HEIGHT;

        var iterator = m_RootProp.FindPropertyRelative(UNITYEVENT_LAST_PROPERTY);

        int i = 0;
        while (i++ < ChildCount && iterator.NextVisible(false))
        {
          ExtraHeight += EditorGUI.GetPropertyHeight(iterator, iterator.isExpanded) + STD_PAD;
        }
      }

      public SerializedProperty GetChildIterator()
      {
        if (ChildCount == 0)
          return null;
        return m_RootProp.FindPropertyRelative(UNITYEVENT_LAST_PROPERTY);
      }
    } // end internal class DrawerState


    public override void OnGUI(Rect total, SerializedProperty prop, GUIContent label)
    {
      PropertyDrawerState.Restore(prop, out DrawerState state);

      // enable/disable button
      float btn_begin = InspectorDrawers.FieldStartX + InspectorDrawers.FieldWidth * 0.45f;
      var pos = new Rect(btn_begin, total.y + STD_PAD_HALF, total.xMax - btn_begin, STD_LINE_HEIGHT);

      string btn_label;
      if (state.Event.IsEnabled)
        btn_label = "Disable Event";
      else
        btn_label = "Enable Event";

      if (GUI.Button(pos, btn_label))
      {
        Undo.RecordObject(prop.serializedObject.targetObject, btn_label);
        
        state.Event.IsEnabled = !state.Event.IsEnabled;

        return;
      }

      if (!state.Event.IsEnabled)
        label.text += LABEL_SUFFIX_DISABLED;

      // now do foldout header:
      pos.x = total.x;
      pos.xMax = btn_begin - STD_PAD * 2;

      if (FoldoutHeader.Open(pos, label, prop, out FoldoutHeader header, prop.depth))
      {
        pos.x     = header.Rect.x;
        pos.xMax  = total.xMax;
        pos.y    += pos.height + STD_PAD;

        EditorGUI.BeginDisabledGroup(!state.Event.IsEnabled);

        // get the property iterator for our extra members:
        var child_prop = state.GetChildIterator();
        if (child_prop != null)
        {
          int i = 0;

          // iterate:
          while (i++ < state.ChildCount && child_prop.NextVisible(false))
          {
            label.text = child_prop.displayName;
            pos.height = EditorGUI.GetPropertyHeight(child_prop, label, includeChildren: true);

            _ = EditorGUI.PropertyField(pos, child_prop, label, includeChildren: true);

            pos.y += pos.height + STD_PAD;
          }

          child_prop.Dispose();
        }

        // finally, draw the vanilla event interface:
        pos.xMin += InspectorDrawers.Indent;
        pos.yMax  = total.yMax;

        label.text = state.EventLabel;
        base.OnGUI(pos, prop, label);

        EditorGUI.EndDisabledGroup();
      }

      header.Rect.yMax = pos.yMax + STD_PAD;
      header.Dispose();
    }


    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
      if (!prop.isExpanded)
        return UNEXPANDED_HEIGHT;

      PropertyDrawerState.Restore(prop, out DrawerState state);

      if (state.IsStale)
        state.UpdateExtraHeight();

      return base.GetPropertyHeight(prop, label) + STD_PAD_HALF + state.ExtraHeight;
    }

  } // end class EventDrawer

}
