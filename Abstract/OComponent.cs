/*! @file       Abstract/OComponent.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-01-20
**/

using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

using Action = System.Action;
using Condition = System.Func<bool>;


namespace Ore
{
  /// <summary>
  ///   Base class for Bore MonoBehaviour components of GameObjects
  ///   (AKA "Scene objects").
  /// </summary>
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
    public void DestroySelf(float in_seconds = 0f)
    {
      if (Application.isEditor && in_seconds.IsZero())
        DestroyImmediate(this);
      else
        Destroy(this, in_seconds);
    }

    [PublicAPI]
    public void DestroyGameObject(float in_seconds = 0f)
    {
      if (Application.isEditor && in_seconds.IsZero())
        DestroyImmediate(gameObject);
      else
        Destroy(gameObject, in_seconds);
    }

    #endregion  EVENT CALLBACK ACTIONS


    #region STATIC SECTION

    protected static IEnumerator InvokeNextFrame(Action action)
    {
      yield return new WaitForEndOfFrame();
      action();
    }

    protected static IEnumerator InvokeNextFrameIf(Action action, Condition condition)
    {
      yield return new WaitForEndOfFrame();

      if (condition())
        action();
    }

    protected static IEnumerator InvokeNextFrameIf(Action action, Condition condition, Action else_action)
    {
      yield return new WaitForEndOfFrame();

      if (condition())
        action();
      else
        else_action();
    }

    protected static IEnumerator InvokeInSeconds(Action action, float s)
    {
      if (s < Floats.EPSILON)
        action();
      else
      {
        yield return new WaitForSeconds(s);
        action();
      }
    }

    #endregion STATIC SECTION

  } // end class OComponent

}
