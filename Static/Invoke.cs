/*! @file       Static/Invoke.cs
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
  /// "Ore.Invoke" rather than just "Invoke"—even if you have a
  /// "using Ore" statement in your file! This is my bad... may rename
  /// this API later.
  /// </remarks>
  [PublicAPI]
  public static class Invoke
  {

    public static void NextFrame([NotNull]   Action    action,
                                 [CanBeNull] Object    ifAlive = null,
                                 [CanBeNull] Condition ifTrue  = null)
    {
      var routine = new DeferringRoutine(action, ifTrue);

      if (ifAlive is null)
      {
        ActiveScene.Coroutines.Run(routine);
      }
      else
      {
        ActiveScene.Coroutines.Run(routine, ifAlive);
      }
    }


    public static void AfterDelay([NotNull]   Action       action,
                                              TimeInterval delay,
                                  [CanBeNull] Object       ifAlive = null,
                                  [CanBeNull] Condition    ifTrue  = null)
    {
      if (delay.Ticks > 0L)
      {
        if (ifAlive is null)
        {
          ActiveScene.Coroutines.Run(new DeferringRoutine(action, delay, ifTrue));
        }
        else
        {
          ActiveScene.Coroutines.Run(new DeferringRoutine(action, delay, ifTrue), ifAlive);
        }
      }
      else if ((ifTrue is null || ifTrue.Invoke()) && (ifAlive is null || ifAlive))
      {
        action.Invoke();
      }
    }

  } // end static class Invoke
}