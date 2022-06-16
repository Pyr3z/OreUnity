/** @file       Editor/PropertyDrawerState.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-15
**/

using System.Collections.Generic;

using UnityEditor;

using Debug = UnityEngine.Debug;


namespace Bore
{

  public abstract class PropertyDrawerState
  {

  #region INSTANCE

    public bool NeedsUpdate => m_RootProp.IsDisposed();
    public bool IsStale { get; protected set; }


    public void UpdateProperty(SerializedProperty root_property)
    {
      m_RootProp = root_property;
      UpdateDetails();
      RenewLifespan();
    }

    public void RenewLifespan() => m_Lifespan = RESET_LIFESPAN_TICKS;


    protected abstract void UpdateDetails();


    protected SerializedProperty m_RootProp;

    private int m_Lifespan;

  #endregion INSTANCE


  #region STATIC

    private const int RESET_LIFESPAN_TICKS = 100; // in Editor ticks

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
        {

        }
      }
    }

    private static bool TickedStateIsExpired(PropertyDrawerState state)
    {
      return --state.m_Lifespan <= 0 &&
             ( state.NeedsUpdate || s_InspectorTracker.activeEditors.Length == 0 );
    }

  #endregion STATIC

  } // end abstract class PropertyDrawerState

}
