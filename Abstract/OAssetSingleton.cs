/*! @file       Abstract/OAssetSingleton.cs
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
**/

using JetBrains.Annotations;

using UnityEngine;
using UnityEngine.Serialization;


namespace Ore
{
  /// <summary>
  ///   Base class for singleton objects which exists without a formal "parent"
  ///   to which it is dependent. (no scene, no GameObject.) <br/>
  ///   An asset instance is auto-instantiated upon editor reload if your type
  ///   is non-abstract, non-generic, and such an asset does not already exist.*
  /// </summary>
  /// <typeparam name="TSelf">
  ///   Successor should pass its own type
  ///   (<a href="https://en.wikipedia.org/wiki/Curiously_recurring_template_pattern">CRTP</a>),
  ///   or else its own "TSelf" generic type parameter if the intent is to
  ///   create a middleman base class.
  /// </typeparam>
  /// <remarks>
  ///   * The auto-instantiation behaviour can be tweaked/squelched with
  ///   <see cref="AutoCreateAssetAttribute">[AutoCreateAsset(path)]</see>,
  ///   and is implicitly disabled by the presence of either
  ///   <see cref="CreateAssetMenuAttribute">[CreateAssetMenu(...)]</see> or
  ///   <see cref="System.ObsoleteAttribute">[System.Obsolete]</see>.
  ///   <br/><br/>
  ///   <b>Pro Tip 1:</b> You can implement the built-in <c>void Reset()</c>
  ///   Unity message to define better defaults for this base class's instance
  ///   properties. This way, you can eliminate human configuration error when
  ///   your singleton expects and only works when, for example,
  ///   <see cref="IsRequiredOnLaunch"/> is set to <c>true</c>. <br/><br/>
  ///   <b>Pro Tip 2:</b> (extends Pro Tip 1) If you use <c>void OnValidate()</c>
  ///   instead of <c>Reset()</c>, you can <i>force</i> your singleton's
  ///   configuration to be certain way, as opposed to redefining default values.
  /// </remarks>
  /// <seealso cref="Ore.Editor.AssetsValidator"/>
  [PublicAPI]
  public abstract class OAssetSingleton<TSelf> : OAsset
    where TSelf : OAssetSingleton<TSelf>
  {
    /// <summary>
    ///   Get the current singleton instance.
    /// </summary>
    /// <remarks>
    ///   Some folks prefer <see cref="Instance">"Instance"</see>, some prefer
    ///   <see cref="Current">"Current"</see>, and <i>some</i> even prefer
    ///   <see cref="Agent">"Agent"</see>; they are all exactly the same. <br/><br/>
    ///   This is one of the few times I will attempt to make everyone happy ;)
    /// </remarks>
    public static TSelf Current  => s_Current;

    /// <inheritdoc cref="Current"/>
    public static TSelf Instance => s_Current;

    /// <inheritdoc cref="Current"/>
    public static TSelf Agent    => s_Current;

    /// <summary>
    ///   Check if there is an active instance of this singleton.
    /// </summary>
    public static bool IsActive  => s_Current;


    /// <summary>
    ///   Attempts to guarantee (with a very likelihood of success) the existence
    ///   of this singleton's instance.
    /// </summary>
    /// <returns>
    ///   False if no instance could be found or successfully created.
    /// </returns>
    /// <remarks>
    ///   If called from Editor code, this function's internal call to
    ///   <see cref="OAsset.TryCreate{T}(out T, string)"/> will attempt to create
    ///   and register a new asset. <br/>
    ///   <b>Conversely,</b> when called from "Runtime" code, a conventional
    ///   asset will <i>not</i> be created, but the creation of a purely-in-RAM
    ///   instance of this here singleton will be attempted nevertheless.
    /// </remarks>
    public static bool TryGuarantee(out TSelf instance)
    {
      return (instance = s_Current) || ( TryCreate(out instance) && s_Current == instance );
    }


    private static TSelf s_Current;


    // instance shtuff

    /// <summary>
    ///   Provided as a property for completeness, and potentially for the
    ///   purposes of unit testing. <br/>
    ///   If toggled in the Editor, the instance of this asset singleton will be
    ///   added to / removed from the global "Preloaded Assets" list.
    /// </summary>
    public bool IsRequiredOnLaunch => m_IsRequiredOnLaunch;

    /// <summary>
    ///   If true at runtime: <br/>
    ///   In the event that a new instance of this class gets instantiated, the
    ///   current instance will be destroyed and replaced. Otherwise, new
    ///   instances trying to replace the current instance will be
    ///   auto-destroyed before they finish initializing.
    /// </summary>
    public bool IsReplaceable
    {
      get => m_IsReplaceable;
      set => m_IsReplaceable = value;
    }


    [SerializeField]
    [Tooltip("If toggled in the Editor, the instance of this asset singleton will be " +
             "added to / removed from the global \"Preloaded Assets\" list.")]
    protected bool m_IsRequiredOnLaunch = false;

    [SerializeField]
    [Tooltip("If true at runtime:\nIn the event that a new instance of this "       +
             "class gets instantiated, the current instance will be destroyed and " +
             "replaced. Otherwise, new instances trying to replace the current "    +
             "instance will be auto-destroyed before they finish initializing.")]
    protected bool m_IsReplaceable = false;

    [SerializeField, FormerlySerializedAs("m_OnAfterInitialized")]
    protected DelayedEvent m_OnFirstInitialized = new DelayedEvent();


    protected virtual void OnEnable()
    {
      if (!TryInitialize((TSelf)this))
      {
        // Orator.Warn("OAssetSingleton failed to initialize!", this);
      }
    }

    protected virtual void OnDisable()
    {
      if (s_Current == this)
        s_Current = null;
    }

    protected override void OnValidate()
    {
      base.OnValidate();
      _ = EditorBridge.TrySetPreloadedAsset(this, m_IsRequiredOnLaunch);
    }


    // ReSharper disable once CognitiveComplexity
    protected bool TryInitialize(TSelf self)
    {
      OAssert.True(this == self, "Proper usage: this.TryInitialize(this)", this);

      if (s_Current && s_Current != self)
      {
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

      if (m_OnFirstInitialized.IsEnabled && !m_OnFirstInitialized.TryInvoke())
        return false;

      m_OnFirstInitialized.IsEnabled = false;

      s_Current = self;

      return true;
    }

  } // end class OAssetSingleton

}
