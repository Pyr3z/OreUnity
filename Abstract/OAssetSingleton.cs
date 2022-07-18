/** @file       Abstract/OAssetSingleton.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-02-17
 *  
 *  @brief      Base class for singleton objects which exists without a
 *              formal "parent" to which it is dependendent.
 *
 *  @remark     No scene, No GameObject ;)
 *  
 *  @remark     For non-abstract, non-generic subclasses, an asset
 *              (ScriptableObject) is auto-generated on Editor reload
 *              if an instance does not already exist (or cannot be found
 *              in a standard locaton).
 *
 *  @TODO Implement a class attribute, [AssetPathAttribute(string)], which
 *        gives subclasses the ability to define an explicit location for
 *        where (or where not) their singleton asset should exist.
**/

using UnityEngine;

using UnityEvent = UnityEngine.Events.UnityEvent;


namespace Bore
{
  /// <summary>
  ///   Base class for singleton objects which exists without a formal "parent"
  ///   to which it is dependendent. (no scene, no GameObject.)
  ///   Auto-instantiated on Editor reload if an instance does not exist.
  /// </summary>
  /// <typeparam name="TSelf">
  ///   Successor should pass its own type (CRTP).
  /// </typeparam>
  public abstract class OAssetSingleton<TSelf> : OAsset
    where TSelf : OAssetSingleton<TSelf>
  {
    public static TSelf Current  => s_Current;
    public static TSelf Instance => s_Current; // compatibility API

    private static TSelf s_Current;


    public static bool IsActive       => s_Current;
    public static bool IsReplaceable  => !s_Current || s_Current.m_IsReplaceable;


  [Header("Asset Singleton Properties")]
    [SerializeField]
    private bool        m_IsRequiredOnLaunch  = false;
    [SerializeField]
    protected bool      m_IsReplaceable       = false;
    [SerializeField]
    protected HideFlags m_AdvancedFlags       = HideFlags.DontUnloadUnusedAsset;

    [Space]

    [SerializeField]
    protected UnityEvent m_OnAfterInitialized = new UnityEvent();



    protected virtual void OnEnable()
    {
      if (!TryInitialize((TSelf)this))
      {
        Orator.Warn("OAssetSingleton failed to initialize!", this);
      }
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

      if (this is IImmortalSingleton)
      {
        m_IsReplaceable       = false;
        m_IsRequiredOnLaunch  = true;
      }

      _ = EditorBridge.TrySetPreloadedAsset(this, m_IsRequiredOnLaunch);
    }


    protected bool TryInitialize(TSelf self)
    {
      Orator.Assert.True(this == self, "Proper usage: this.TryInitialize(this)", this);

      if (s_Current)
      {
        if (s_Current == self)
          return true;

        if (!s_Current.m_IsReplaceable)
        {
          if (Application.isEditor)
            DestroyImmediate(this, allowDestroyingAssets: true);
          else
            Destroy(this);

          return false;
        }

        if (Application.isEditor)
          DestroyImmediate(s_Current, allowDestroyingAssets: true);
        else
          Destroy(s_Current);
      }

      s_Current = self;

      m_OnAfterInitialized.Invoke();

      return true;
    }

  } // end class OAssetSingleton

}
