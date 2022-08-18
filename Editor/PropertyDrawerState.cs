/*! @file       Editor/PropertyDrawerState.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-15
 *
 *  @brief      A base class for tracking/caching states for
 *              non-trivial "Drawer" implementations.
**/

using System.Collections.Generic;

using UnityEditor;

using Debug = UnityEngine.Debug;


namespace Ore.Editor
{

  public abstract class PropertyDrawerState
  {

    #region INSTANCE

    public bool IsStale { get; protected set; }


    public void UpdateProperty(SerializedProperty root_property)
    {
      m_RootProp = root_property;
      OnUpdateProperty();
      RenewLifespan();
    }


    protected bool NeedsUpdate => m_RootProp.IsDisposed();

    protected abstract void OnUpdateProperty(); // <-- 1 required override

    protected virtual void OnUpdateHeight() { } // <-- 1 optional override

    protected bool CheckFails(out SerializedProperty root_property)
    {
      if (OAssert.Fails(!m_RootProp.IsDisposed()))
      {
        root_property = null; // output null = extra safety measure
        return true;          // failed
      }
      else
      {
        root_property = m_RootProp;
        return false;
      }
    }

    protected void RenewLifespan() => m_Lifespan = RESET_LIFESPAN_TICKS;

    // private fields (subclasses likely add more)
    private SerializedProperty m_RootProp;
    private int m_Lifespan;

    #endregion INSTANCE


    #region STATIC

    public static void Restore<TState>(SerializedProperty root_property, out TState state)
      where TState : PropertyDrawerState, new()
    {
      uint id = root_property.GetPropertyHash();

      if (s_StateMap.TryGetValue(id, out var basestate) && basestate != null)
      {
        state = (TState)basestate;
        if (state.NeedsUpdate || !SerializedProperty.EqualContents(state.m_RootProp, root_property))
        {
          state.UpdateProperty(root_property);
          state.IsStale = false;
        }
        else
        {
          state.RenewLifespan();
          state.IsStale = true;
        }
      }
      else
      {
        s_StateMap[id] = state = new TState();
        state.UpdateProperty(root_property);
      }

      if (s_InspectorTracker == null)
      {
        s_InspectorTracker = ActiveEditorTracker.sharedTracker;
        EditorApplication.update += TickStaleStates;
      }
    }


    private const int RESET_LIFESPAN_TICKS = 64; // in Editor ticks

    // TODO: replace lame Dictionary with Levi's badass HashMap
    private static readonly Dictionary<uint, PropertyDrawerState> s_StateMap = new Dictionary<uint, PropertyDrawerState>();

    private static ActiveEditorTracker s_InspectorTracker = null;

    private static void TickStaleStates()
    {
      Debug.Assert(s_InspectorTracker != null);

      // Note: this will be much faster + cleaner after Dictionary gets replaced with HashMap
      var expireds = new List<uint>(s_StateMap.Count);
      foreach (var kvp in s_StateMap)
      {
        if (TickedStateIsExpired(kvp.Value))
          expireds.Add(kvp.Key);
      }

      // perform removal
      foreach (var uid in expireds)
      {
        s_StateMap.Remove(uid);
      }

      if (s_StateMap.Count == 0)
        EditorApplication.update -= TickStaleStates;
    }

    private static bool TickedStateIsExpired(PropertyDrawerState state)
    {
      return --state.m_Lifespan <= 0 &&
             (state.NeedsUpdate || s_InspectorTracker.activeEditors.Length == 0);
    }

    #endregion STATIC

  } // end abstract class PropertyDrawerState

}
