/*! @file       Runtime/ABI.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-01-20
**/

namespace Ore
{
  public enum ABI
  {
    ARMv7  = 0,
    ARM64  = 1,
    x86    = 2, // x86* = ChromeOS
    x86_64 = 3,

    ARM   = ARMv7,
    ARM32 = ARMv7,
    ARMv8 = ARM64,
  }
}