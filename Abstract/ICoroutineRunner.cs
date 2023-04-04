/*! @file       Abstract/ICoroutineRunner.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-12-06
**/

using UnityEngine;

using JetBrains.Annotations;

using IEnumerator = System.Collections.IEnumerator;


namespace Ore
{
  [PublicAPI]
  public interface ICoroutineRunner
  {
    /// <param name="key">
    ///   The non-null key perhaps associated with running coroutines.
    /// </param>
    /// <returns>
    ///   True if anything at the given key is currently running.
    /// </returns>
    bool IsRunning([NotNull] object key);

    /// <summary>
    ///   Enqueues the routine to be run by this ICoroutineRunner. Primarily
    ///   useful for non-Scene-bound code to start a coroutine even if not part
    ///   of a MonoBehaviour, and even if there aren't any scenes loaded yet.
    /// </summary>
    ///
    /// <param name="routine">
    ///   A valid IEnumerator object representing a coroutine function body (or
    ///   clever equivalent).
    /// </param>
    ///
    /// <remarks>
    ///   This overload can only be halted by natural completion of the routine,
    ///   or by the parent runner halting altogether.
    /// </remarks>
    void Run([NotNull] IEnumerator routine);

     /// <summary>
     ///   Enqueues the routine to be run by this ICoroutineRunner. Primarily
     ///   useful for non-Scene-bound code to start a coroutine even if not part
     ///   of a MonoBehaviour, and even if there aren't any scenes loaded yet.
     /// </summary>
     ///
     /// <param name="routine">
     ///   A valid IEnumerator object representing a coroutine function body (or
     ///   clever equivalent).
     /// </param>
     ///
     /// <param name="key">
     ///   A reference to this contractual key will be stored. If/When the contract
     ///   object expires (either due to GC cleanup or Unity Object deletion), the
     ///   associated coroutine will halt itseslf, if it is still running.
     ///   Multiple routines may be associated with the same key.
     ///   A retained key can be used to Halt its associated coroutine(s) manually.
     /// </param>
    void Run([NotNull] IEnumerator routine, [NotNull] Object key);

    /// <summary>
    ///   Enqueues the routine to be run by this ICoroutineRunner. Primarily
    ///   useful for non-Scene-bound code to start a coroutine even if not part
    ///   of a MonoBehaviour, and even if there aren't any scenes loaded yet.
    /// </summary>
    ///
    /// <param name="routine">
    ///   A valid IEnumerator object representing a coroutine function body (or
    ///   clever equivalent).
    /// </param>
    ///
    /// <param name="key">
    ///   A non-null string key to associate with <paramref name="routine"/>.
    ///   Multiple routines may be associated with the same key.
    ///   A retained key can be used to Halt its associated coroutine(s) manually.
    /// </param>
    void Run([NotNull] IEnumerator routine, [NotNull] string key);

    /// <summary>
    ///   Enqueues the routine to be run by this ICoroutineRunner. Primarily
    ///   useful for non-Scene-bound code to start a coroutine even if not part
    ///   of a MonoBehaviour, and even if there aren't any scenes loaded yet.
    /// </summary>
    ///
    /// <param name="routine">
    ///   A valid IEnumerator object representing a coroutine function body (or
    ///   clever equivalent).
    /// </param>
    ///
    /// <param name="guidKey">
    ///   A randomly-generated GUID string that is associated with the running
    ///   <paramref name="routine"/>.
    ///   You may associate other routines with this same key.
    ///   A retained key can be used to Halt its associated coroutine(s) manually.
    /// </param>
    void Run([NotNull] IEnumerator routine, [NotNull] out string guidKey);

    /// <summary>
    ///   Cancels all enqueued coroutines owned by this runner that are paired
    ///   with the given <paramref name="key"/>, if any.
    /// </summary>
    ///
    /// <param name="key">
    ///   The non-null thing you associated with the intended routine(s) when
    ///   Run(...) was called.
    /// </param>
    void Halt([NotNull] object key);

  }
}