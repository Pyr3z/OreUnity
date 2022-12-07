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

    #region EVENT CALLBACK ACTIONS

    [System.Diagnostics.Conditional("DEBUG")]
    [PublicAPI]
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
      if (Application.isEditor && inSeconds.IsZero())
        DestroyImmediate(this);
      else
        Destroy(this, inSeconds);
    }

    [PublicAPI]
    public void DestroyGameObject(float inSeconds = 0f)
    {
      if (Application.isEditor && inSeconds.IsZero())
        DestroyImmediate(gameObject);
      else
        Destroy(gameObject, inSeconds);
    }

    #endregion  EVENT CALLBACK ACTIONS


    #region STATIC SECTION

    protected static DeferringRoutine InvokeNextFrame(Action action)
    {
      return new DeferringRoutine(action);
    }

    protected static DeferringRoutine InvokeNextFrameIf(Action action, Condition condition)
    {
      return new DeferringRoutine(action, condition);
    }

    protected static DeferringRoutine DelayInvoke(Action action, TimeInterval t)
    {
      return new DeferringRoutine(action, t);
    }

    #endregion STATIC SECTION

  } // end class OComponent

}
