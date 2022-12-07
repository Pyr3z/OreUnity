/*! @file       Abstract/ICoroutineRunner.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-12-06
**/

using UnityEngine;

using JetBrains.Annotations;

using IEnumerator = System.Collections.IEnumerator;


namespace Ore
{
  public interface ICoroutineRunner
  {
     void EnqueueCoroutine([NotNull] IEnumerator routine, [NotNull] Object key);

     void EnqueueCoroutine([NotNull] IEnumerator routine, [NotNull] string key);

     void EnqueueCoroutine([NotNull] IEnumerator routine, [NotNull] out string key);

     void EnqueueCoroutine([NotNull] IEnumerator routine);

     void CancelCoroutinesFor([NotNull] object key);

     void CancelAllCoroutines();
  }
}