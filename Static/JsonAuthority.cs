/*! @file       Static/JsonAuthority.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-01-22
**/

using JetBrains.Annotations;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Encoding         = System.Text.Encoding;
using StringComparison = System.StringComparison;

using TextReader = System.IO.TextReader;
using TextWriter = System.IO.TextWriter;


namespace Ore
{
  [PublicAPI]
  public static class JsonAuthority
  {

    public static readonly Encoding Encoding     = Encoding.UTF8;
    public static readonly Encoding WideEncoding = Encoding.Unicode; // LE UTF16


    public static readonly DateParseHandling    DateParseHandling    = DateParseHandling.DateTime;
    public static readonly DateFormatHandling   DateFormatHandling   = DateFormatHandling.IsoDateFormat;
    public static readonly DateTimeZoneHandling DateTimezoneHandling = DateTimeZoneHandling.Utc;


    public static readonly JsonLoadSettings LoadSettings = new JsonLoadSettings
    {
      // klients may feel free to alter at runtime
      #if DEBUG
      CommentHandling               = CommentHandling.Ignore,
      DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error,
      LineInfoHandling              = LineInfoHandling.Load,
      #else
      CommentHandling               = CommentHandling.Ignore,
      DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace,
      LineInfoHandling              = LineInfoHandling.Ignore,
      #endif
    };

    public static readonly JsonMergeSettings MergeSettings = new JsonMergeSettings
    {
      PropertyNameComparison = StringComparison.InvariantCultureIgnoreCase,
      MergeArrayHandling     = MergeArrayHandling.Replace,
      MergeNullValueHandling = MergeNullValueHandling.Ignore
    };



    [NotNull]
    public static JsonTextReader MakeTextReader([NotNull] TextReader reader)
    {
      return new JsonTextReader(reader)
      {
        DateParseHandling    = DateParseHandling,
        DateTimeZoneHandling = DateTimezoneHandling,
      };
    }

    [NotNull]
    public static JsonTextWriter MakeTextWriter([NotNull] TextWriter writer, bool pretty = EditorBridge.IS_DEBUG)
    {
      return new JsonTextWriter(writer)
      {
        Formatting           = pretty ? Formatting.Indented : Formatting.None,
        DateFormatHandling   = DateFormatHandling,
        DateTimeZoneHandling = DateTimezoneHandling,
      };
    }

  } // end class JsonAuthority
}