/** @file       Abstract/SceneComponent.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-01-20
**/

using System.Collections;

using UnityEngine;


namespace Bore.Abstract
{
  using Action    = System.Action;
  using Condition = System.Func<bool>;


  /// <summary>
  ///   Base class for MonoBehaviour components of GameObjects
  ///   (AKA "Scene objects").
  /// </summary>
  public abstract class SceneComponent : MonoBehaviour
  {

    #region     Static Section

    public static IEnumerator InvokeNextFrame(Action action)
    {
      yield return new WaitForEndOfFrame();
      action();
    }

    public static IEnumerator InvokeNextFrameIf(Action action, Condition condition)
    {
      yield return new WaitForEndOfFrame();

      if (condition())
        action();
    }

    public static IEnumerator InvokeNextFrameIf(Action action, Condition condition, Action else_action)
    {
      yield return new WaitForEndOfFrame();

      if (condition())
        action();
      else
        else_action();
    }

    public static IEnumerator InvokeInSeconds(Action action, float s)
    {
      if (s < Floats.EPSILON)
      {
        action();
      }
      else
      {
        yield return new WaitForSeconds(s);
        action();
      }
    }

    #endregion  Static Section


    #region     UnityEvent Actions

    [System.Diagnostics.Conditional("DEBUG")]
    public void DebugLog(string message)
    {
      // TODO change out this quick n easy line
      Debug.Log($"{GetType().Name}: {message}\n(GameObject \"{name}\")");
    }


    public void SpawnLocal(GameObject prefab)
    {
      // DISCLAIMER: SpawnPools would be FAR better to use instead of this!
      if (prefab)
        Instantiate(prefab, transform.position, transform.rotation * prefab.transform.rotation);
      else
        Debug.LogWarning($"{GetType().Name} \"{name}\" : Missing Prefab reference!");
    }

    public void SpawnWorld(GameObject prefab)
    {
      // DISCLAIMER: SpawnPools would be FAR better to use instead of this!
      if (prefab)
        Instantiate(prefab);
      else
        Debug.LogWarning($"{GetType().Name} \"{name}\" : Missing Prefab reference!");
    }

    
    public void ToggleSelf()
    {
      enabled = !enabled;
    }

    public void ToggleGameObject()
    {
      gameObject.SetActive(!gameObject.activeSelf);
    }


    public void DestroySelf(float in_seconds = 0f)
    {
      Destroy(this, in_seconds);
    }

    public void DestroyGameObject(float in_seconds = 0f)
    {
      Destroy(gameObject, in_seconds);
    }

    #endregion  UnityEvent Actions

  } // end class SceneComponent

}
