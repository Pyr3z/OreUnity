/*! @file       Static/IOResult.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-03
 *
 *  See also: Static/Filesystem.cs
**/


namespace Ore
{
  public enum IOResult
  {
    None = -1,
    Success,
    PathNotFound,
    PathNotValid,
    FileAlreadyInUse,
    NotPermitted,
    DiskFull,
    UnknownFailure
  }
}