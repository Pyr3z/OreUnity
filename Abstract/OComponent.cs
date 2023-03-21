/*! @file       Abstract/OComponent.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-01-20
**/

using JetBrains.Annotations;
using UnityEngine;

using IEnumerator = System.Collections.IEnumerator;
using Action      = System.Action;
using Condition   = System.Func<bool>;


namespace Ore
{
  /// <summary>
  ///   Base class for Bore MonoBehaviour components of GameObjects
  ///   (AKA "Scene objects").
  /// </summary>
  [PublicAPI]
  public abstract class OComponent : MonoBehaviour
  {

    /// <summary>
    ///   Debug log a message, with context, from virtually anywhere :O
    /// </summary>
    ///
    /// <remarks>
    ///   The purpose of this method—as well as similar methods provided in this
    ///   base class—is mainly to serve as a convenience utility when working
    ///   with Object references in serialized
    ///   <see cref="UnityEngine.Events.UnityEvent"/>s.
    /// </remarks>
    [System.Diagnostics.Conditional("DEBUG")]
    public void DebugLog([CanBeNull] string message)
    {
      Orator.Log(message, this);
    }


    /// <summary>
    ///   Spawns a new GameObject and transforms the original prefab's pose into
    ///   the local space of this component's GameObject (without childing,
    ///   ignoring scale).
    /// </summary>
    ///
    /// <inheritdoc cref="DebugLog"/>
    public void SpawnLocal([CanBeNull] GameObject prefab)
    {
      // DISCLAIMER: SpawnPools would be FAR better to use instead of this!
      if (prefab)
        Instantiate(prefab, transform.position, transform.rotation * prefab.transform.rotation);
    }

    /// <summary>
    ///   Spawns a new GameObject with the original prefab's transform.
    /// </summary>
    ///
    /// <inheritdoc cref="SpawnLocal"/>
    public void SpawnWorld([CanBeNull] GameObject prefab)
    {
      // DISCLAIMER: SpawnPools would be FAR better to use instead of this!
      if (prefab)
        Instantiate(prefab);
    }


    /// <summary>
    ///   Flips this component's <see cref="Behaviour.enabled"/> state.
    /// </summary>
    ///
    /// <inheritdoc cref="DebugLog"/>
    public void ToggleSelf()
    {
      enabled = !enabled;
    }

    /// <summary>
    ///   Flips this component's GameObject's <see cref="GameObject.activeSelf"/>
    ///   state.
    /// </summary>
    ///
    /// <inheritdoc cref="ToggleSelf"/>
    public void ToggleGameObject()
    {
      gameObject.SetActive(!gameObject.activeSelf);
    }


    /// <summary>
    ///   Destroys this component relatively immediately, thereby removing it
    ///   from its owning GameObject's list of components.
    /// </summary>
    ///
    /// <inheritdoc cref="DestroyOther"/>
    public void DestroySelf()
    {
      this.Destroy();
    }

    /// <summary>
    ///   Destroys the GameObject that owns this component relatively
    ///   immediately, thereby also destroying this component and any/all other
    ///   childed components and GameObjects.
    /// </summary>
    ///
    /// <seealso cref="UnityObjects.Destroy(Object)">
    ///   Ore.UnityObjects.Destroy(...)
    /// </seealso>
    ///
    /// <inheritdoc cref="DebugLog"/>
    public void DestroyGameObject()
    {
      gameObject.Destroy();
    }

    /// <summary>
    ///   Destroys the given object relatively immediately, if it exists. <br/>
    ///   If executed in-editor, any associated serialized asset will also be
    ///   destroyed.
    /// </summary>
    ///
    /// <seealso cref="UnityObjects.Destroy(Object)">
    ///   Ore.UnityObjects.Destroy(...)
    /// </seealso>
    ///
    /// <inheritdoc cref="DebugLog"/>
    public void DestroyOther([CanBeNull] Object other)
    {
      if (other)
        other.Destroy();
    }


  #region DEPRECATIONS

    [System.Obsolete("Construct a DelayedRoutine(*) directly, or use the new OInvoke.NextFrame(*) API instead.", false)]
    protected static DelayedRoutine InvokeNextFrame(Action action)
    {
      return new DelayedRoutine(action);
    }

    [System.Obsolete("Construct a DelayedRoutine(*) directly, or use the new OInvoke.NextFrame(*) API instead.", false)]
    protected static DelayedRoutine InvokeNextFrameIf(Action action, Condition condition)
    {
      return new DelayedRoutine(action, condition);
    }

    [System.Obsolete("Construct a DelayedRoutine(*) directly, or use the new OInvoke.AfterDelay(*) API instead.", false)]
    protected static DelayedRoutine DelayInvoke(Action action, TimeInterval t)
    {
      return new DelayedRoutine(action, t);
    }

  #endregion DEPRECATIONS

  } // end class OComponent

}
