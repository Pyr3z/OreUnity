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
  /// Static utility class that allows you to invoke actions at different times,
  /// or in special ways.
  /// </summary>
  ///
  /// <remarks>
  /// If in a MonoBehaviour class, you will need to call this API as
  /// "Ore.OInvoke" rather than just "OInvoke"—even if you have a
  /// "using Ore" statement in your file! This is my bad... may rename
  /// this API later.
  /// </remarks>
  [PublicAPI]
  public static class OInvoke
  {

    public static void NextFrame([NotNull]   Action    action,
                                 [CanBeNull] Condition ifTrue = null)
    {
      ActiveScene.Coroutines.Run(new DelayedRoutine(action, ifTrue));
    }

    public static void NextFrame([NotNull]   Action    action,
                                 [CanBeNull] Object    ifAlive,
                                 [CanBeNull] Condition ifTrue  = null)
    {
      if (!ifAlive)
        return;

      ActiveScene.Coroutines.Run(new DelayedRoutine(action, ifTrue), ifAlive);
    }


    public static void AfterDelay([NotNull]   Action       action,
                                              TimeInterval delay,
                                  [CanBeNull] Condition    ifTrue  = null)
    {
      if (delay.Ticks > 0L)
      {
        ActiveScene.Coroutines.Run(new DelayedRoutine(action, delay, ifTrue));
      }
      else
      {
        action.Invoke();
      }
    }

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
      else if ((ifTrue is null || ifTrue.Invoke()) && (ifAlive is null || ifAlive))
      {
        action.Invoke();
      }
    }

  } // end static class OInvoke
}