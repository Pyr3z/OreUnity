/*! @file       Static/OInvoke.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-12-07
**/

using JetBrains.Annotations;

using Action      = System.Action;
using Condition   = System.Func<bool>;
using Object      = UnityEngine.Object;


namespace Ore
{
  /// <summary>
  ///   Static utility class that allows you to invoke actions at different times,
  ///   or in special ways.
  /// </summary>
  [PublicAPI]
  public static class OInvoke
  {

    /// <inheritdoc cref="NextFrame(Action,Object,Condition)"/>>
    public static void NextFrame([NotNull]   Action    action,
                                 [CanBeNull] Condition ifTrue = null)
    {
      ActiveScene.Coroutines.Run(new DelayedRoutine(action, ifTrue));
    }

    /// <param name="action">
    ///   The payload delegate to queue up for invokation next frame.
    /// </param>
    ///
    /// <param name="ifAlive">
    ///   When provided, this Unity Object is checked for existence both right
    ///   now and in the following frame before we try to invoke the action
    ///   delegate; if it was destroyed, the action is cancelled. <br/>
    ///   This becomes critically useful if, say, your payload delegate captures
    ///   a reference to this Object, data owned by it, or data that depends on
    ///   it.
    /// </param>
    /// <seealso cref="ICoroutineRunner.Run(System.Collections.IEnumerator,Object)"/>
    ///
    /// <param name="ifTrue">
    ///   If provided and not null, this condition functor will be invoked next
    ///   frame to check if the action delegate should still be invoked.
    /// </param>
    public static void NextFrame([NotNull]   Action    action,
                                 [CanBeNull] Object    ifAlive,
                                 [CanBeNull] Condition ifTrue  = null)
    {
      if (!ifAlive)
        return;

      ActiveScene.Coroutines.Run(new DelayedRoutine(action, ifTrue), ifAlive);
    }


    /// <inheritdoc cref="AfterDelay(Action,TimeInterval,Object,Condition)"/>
    public static void AfterDelay([NotNull]   Action       action,
                                  TimeInterval delay,
                                  [CanBeNull] Condition    ifTrue  = null)
    {
      if (delay.Ticks > 0L)
      {
        ActiveScene.Coroutines.Run(new DelayedRoutine(action, delay, ifTrue));
      }
      else if (ifTrue is null || ifTrue.Invoke())
      {
        action.Invoke();
      }
    }

    /// <param name="action">
    ///   The payload delegate to queue up for invokation next frame.
    /// </param>
    ///
    /// <param name="delay">
    ///   A <see cref="TimeInterval"/> duration (or an implicit cast from
    ///   <see cref="System.TimeSpan"/>, long, or double thereto) defining how
    ///   long we should wait before invoking the action delegate. <br/>
    ///   <i>(See Remarks.)</i>
    /// </param>
    ///
    /// <param name="ifAlive">
    ///   When provided, this Unity Object is checked for existence both right
    ///   now and in the following frame before we try to invoke the action
    ///   delegate; if it was destroyed, the action is cancelled.
    /// </param>
    ///
    /// <param name="ifTrue">
    ///   If provided and not null, this condition functor will be invoked next
    ///   frame to check if the action delegate should still be invoked.
    /// </param>
    ///
    /// <remarks>
    ///   The minimum quanta we can delay for is hard-limited to 1
    ///   frame, so providing an interval smaller than a frame but larger than
    ///   zero will still delay for 1 whole frame. <br/>
    ///   This also means that the "precision" for how long we <i>actually</i>
    ///   delay versus how long was specified is similarly quantized to frame
    ///   steps, ceiling to the next frame until delay has fully elapsed. <br/>
    ///   These limitations are not new, being rooted in Unity's underlying
    ///   Coroutine system. <br/>
    ///   <br/>
    ///   <see cref="TimeInterval.Zero"/> and negative-valued intervals are
    ///   valid inputs, and will cause the payload action to be invoked
    ///   immediately (this frame)—still respecting the ifAlive and ifTrue
    ///   parameters, of course.
    /// </remarks>
    public static void AfterDelay([NotNull]   Action       action,
                                              TimeInterval delay,
                                  [NotNull]   Object       ifAlive,
                                  [CanBeNull] Condition    ifTrue  = null)
    {
      if (!ifAlive)
        return;

      if (delay.Ticks > 0L)
      {
        ActiveScene.Coroutines.Run(new DelayedRoutine(action, delay, ifTrue), ifAlive);
      }
      else if (ifTrue is null || ifTrue.Invoke())
      {
        action.Invoke();
      }
    }

  } // end static class OInvoke
}