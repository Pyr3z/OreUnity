/*! @file       Static/LogTypeFlags.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-03-03
**/

using LogType = UnityEngine.LogType;


namespace Ore
{
  // ReSharper disable once ShiftExpressionZeroLeftOperand

  [System.Flags]
  public enum LogTypeFlags
  {
    None      = (0 << 0),
    Error     = (1 << LogType.Error),
    Assert    = (1 << LogType.Assert),
    Warning   = (1 << LogType.Warning),
    Log       = (1 << LogType.Log),
    Exception = (1 << LogType.Exception),
  }
}