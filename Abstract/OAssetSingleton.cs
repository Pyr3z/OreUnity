/** @file       Abstract/OAssetSingleton.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-02-17
**/

using UnityEngine;


namespace Bore.Abstract
{
  /// <summary>
  ///   Base class for singleton objects which exists without a formal "parent"
  ///   to which it is dependendent. (no scene, no GameObject.)
  /// </summary>
  /// <typeparam name="TSelf">
  ///   Successor should pass its own type (CRTP).
  /// </typeparam>
  public abstract class OAssetSingleton<TSelf> : OAsset
    where TSelf : OAssetSingleton<TSelf>
  {
    public static TSelf Current => s_Current;
    private static TSelf s_Current;


    public static bool IsActive       => s_Current;
    public static bool IsReplaceable  => !s_Current || s_Current.m_IsReplaceable;


  [Header("Asset Singleton")]
    [SerializeField]
    protected bool m_IsRequiredOnLaunch = false;
    [SerializeField]
    protected bool m_IsReplaceable      = false;
    [SerializeField]
    protected HideFlags m_AdvancedFlags = HideFlags.DontUnloadUnusedAsset;

    [SerializeField]
    protected DelayedEvent m_OnAfterInitialized = new DelayedEvent();



    protected virtual void OnEnable()
    {
      OnValidate();
      _ = TryInitialize((TSelf)this);
    }

    protected virtual void OnDisable()
    {
      if (s_Current == this)
      {
        s_Current = null;
      }
    }

    protected virtual void OnValidate()
    {
      this.hideFlags = m_AdvancedFlags;

      EditorBridge.TrySetPreloadedAsset(this, m_IsRequiredOnLaunch);
    }


    protected bool TryInitialize(TSelf self)
    {
      Debug.Assert(this == self, "Proper usage: this.TryInitialize(this)");

      if (s_Current)
      {
        if (s_Current == self)
          return true;

        if (!s_Current.m_IsReplaceable)
        {
          Destroy(this);
          return false;
        }

        Destroy(s_Current);
      }

      s_Current = self;

      m_OnAfterInitialized.Invoke();

      return true;
    }

  } // end class OAssetSingleton

}
