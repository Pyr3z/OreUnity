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

    [PublicAPI]
    [System.Diagnostics.Conditional("DEBUG")]
    public void DebugLog(string message)
    {
      Orator.Log(message, this);
    }


    [PublicAPI]
    public void SpawnLocal(GameObject prefab)
    {
      // DISCLAIMER: SpawnPools would be FAR better to use instead of this!
      if (prefab)
        Instantiate(prefab, transform.position, transform.rotation * prefab.transform.rotation);
      else
        Debug.LogWarning($"{GetType().Name} \"{name}\" : Missing Prefab reference!");
    }

    [PublicAPI]
    public void SpawnWorld(GameObject prefab)
    {
      // DISCLAIMER: SpawnPools would be FAR better to use instead of this!
      if (prefab)
        Instantiate(prefab);
      else
        Debug.LogWarning($"{GetType().Name} \"{name}\" : Missing Prefab reference!");
    }


    [PublicAPI]
    public void ToggleSelf()
    {
      enabled = !enabled;
    }

    [PublicAPI]
    public void ToggleGameObject()
    {
      gameObject.SetActive(!gameObject.activeSelf);
    }


    [PublicAPI]
    public void DestroySelf(float inSeconds = 0f)
    {
      if (Application.isEditor && inSeconds.ApproximatelyZero())
        DestroyImmediate(this);
      else
        Destroy(this, inSeconds);
    }

    [PublicAPI]
    public void DestroyGameObject(float inSeconds = 0f)
    {
      if (Application.isEditor && inSeconds.ApproximatelyZero())
        DestroyImmediate(gameObject);
      else
        Destroy(gameObject, inSeconds);
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
