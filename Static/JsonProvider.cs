/*! @file       Static/JsonProvider.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-04-03
**/

using JetBrains.Annotations;


namespace Ore
{
  [PublicAPI]
  public enum JsonProvider
  {
    /// <summary>
    ///   Dismantle the JsonAuthority. <br/>
    ///   Useful only in special cases.
    /// </summary>
    None,

    /// <summary>
    ///   (default) Use Ore's own flavor of MiniJSON.
    /// </summary>
    /// <seealso href="https://gist.github.com/darktable/1411710"/>
    MiniJson,

    /// <summary>
    ///   Use Unity's flavor of Newtonsoft Json.NET
    ///   (com.unity.nuget.newtonsoft-json), <b>if and only if it is available</b>.
    /// </summary>
    NewtonsoftJson,

    #if NEWTONSOFT_JSON && !PREFER_MINIJSON
    /// <inheritdoc cref="NewtonsoftJson"/>>
    Default = NewtonsoftJson,
    #else
    /// <inheritdoc cref="MiniJson"/>>
    Default = MiniJson,
    #endif
  }
}