/** @file       Editor/EventDrawer.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-14
**/

using UnityEngine;
using UnityEditor;


namespace Bore
{

  [CustomPropertyDrawer(typeof(IEvent))]
  internal class EventDrawer : UnityEditorInternal.UnityEventDrawer
  {
    private const string  UNITYEVENT_LAST_PROPERTY = "m_PersistentCalls";
    
    private const float   STD_PAD           = 2f;
    private const float   STD_LINE_HEIGHT   = 18f;
    private const float   UNEXPANDED_HEIGHT = STD_LINE_HEIGHT + STD_PAD;

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


    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
      base.OnGUI(pos, prop, label);
    }


    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
      return base.GetPropertyHeight(prop, label);
    }

  } // end class EventDrawer

}
