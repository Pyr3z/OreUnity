/*! @file       Runtime/OratorFilter.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-03-03
**/

using UnityEngine;

using System.Text.RegularExpressions;


namespace Ore
{

  [System.Serializable]
  public struct OratorFilter
  {

    public LogTypeFlags Flags;

    [System.NonSerialized]
    public Regex MessageFilter;

  } // end struct OratorFilter

}