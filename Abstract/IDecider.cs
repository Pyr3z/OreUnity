/*! @file     Runtime/IDecider.cs
 *  @author   Levi Perez (levi\@leviperez.dev)
 *  @date     2022-04-12
 *
 *  A data structure that evaluates variable input conditions against some
 *  preconfigured threshold in order to determine a simple (but intelligent)
 *  "yes" or "no".
**/


namespace Ore
{
  public interface IDecider<TFactor> where TFactor : IEvaluator
  {
    bool Decide();
  }

}