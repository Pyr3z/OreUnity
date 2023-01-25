/*! @file       Static/JsonAuthority.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-01-22
**/

using JetBrains.Annotations;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Collections.Generic;
using System.Linq;

using Encoding         = System.Text.Encoding;
using StringComparison = System.StringComparison;

using TextReader = System.IO.TextReader;
using TextWriter = System.IO.TextWriter;

using CultureInfo = System.Globalization.CultureInfo;


namespace Ore
{
  [PublicAPI]
  public static class JsonAuthority
  {

    public static readonly Encoding Encoding     = Encoding.UTF8;
    public static readonly Encoding WideEncoding = Encoding.Unicode; // LE UTF16


    public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
    {
      // if in a pickle, klients may feel free to alter at runtime
      // (especially SerializerSettings.Converters!)

      #if DEBUG
      ReferenceLoopHandling = ReferenceLoopHandling.Error,
      Formatting            = Formatting.Indented,
      #else
      ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
      Formatting            = Formatting.None,
      #endif

      Culture = CultureInfo.InvariantCulture,

      DateParseHandling    = DateParseHandling.DateTime,
      DateFormatHandling   = DateFormatHandling.IsoDateFormat,
      DateTimeZoneHandling = DateTimeZoneHandling.Utc,

      ConstructorHandling    = ConstructorHandling.AllowNonPublicDefaultConstructor,
      MissingMemberHandling  = MissingMemberHandling.Ignore,
      ObjectCreationHandling = ObjectCreationHandling.Auto,
      NullValueHandling      = NullValueHandling.Ignore,
      DefaultValueHandling   = DefaultValueHandling.IgnoreAndPopulate,
        // -> see: https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_DefaultValueHandling.htm

      MetadataPropertyHandling       = MetadataPropertyHandling.Ignore,
      PreserveReferencesHandling     = PreserveReferencesHandling.None,
      TypeNameHandling               = TypeNameHandling.None,
      TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
    };

    public static readonly JsonLoadSettings LoadStrict = new JsonLoadSettings
    {
      // if in a pickle, klients may feel free to alter at runtime

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

    public static readonly JsonMergeSettings MergeReplace = new JsonMergeSettings
    {
      PropertyNameComparison = StringComparison.InvariantCultureIgnoreCase,
      MergeArrayHandling     = MergeArrayHandling.Replace,
      MergeNullValueHandling = MergeNullValueHandling.Ignore
    };

    public static readonly JsonMergeSettings MergeConcat = new JsonMergeSettings
    {
      PropertyNameComparison = StringComparison.InvariantCultureIgnoreCase,
      MergeArrayHandling     = MergeArrayHandling.Concat,
      MergeNullValueHandling = MergeNullValueHandling.Ignore
    };


    [NotNull]
    public static JsonTextWriter MakeWriter([NotNull] TextWriter writer, [NotNull] out JsonConverter[] converters,
                                            JsonSerializerSettings overrides = null)
    {
      converters = SerializerSettings.Converters.ToArray();

      if (overrides is null)
      {
        overrides = SerializerSettings;
      }
      else if (!overrides.Converters.IsEmpty())
      {
        converters = overrides.Converters.Concat(converters).ToArray();
      }

      var jwriter = new JsonTextWriter(writer)
      {
        Culture              = overrides.Culture,
        Formatting           = overrides.Formatting,
        DateFormatHandling   = overrides.DateFormatHandling,
        DateTimeZoneHandling = overrides.DateTimeZoneHandling,
        DateFormatString     = overrides.DateFormatString,
        FloatFormatHandling  = overrides.FloatFormatHandling,
        StringEscapeHandling = overrides.StringEscapeHandling,
      };

      return jwriter;
    }

    [NotNull]
    public static JsonTextReader MakeReader([NotNull] TextReader reader, [NotNull] out JsonConverter[] converters,
                                            JsonSerializerSettings overrides = null)
    {
      converters = SerializerSettings.Converters.ToArray();

      if (overrides is null)
      {
        overrides = SerializerSettings;
      }
      else if (!overrides.Converters.IsEmpty())
      {
        converters = overrides.Converters.Concat(converters).ToArray();
      }

      var jreader = new JsonTextReader(reader)
      {
        Culture              = overrides.Culture,
        MaxDepth             = overrides.MaxDepth,
        DateParseHandling    = overrides.DateParseHandling,
        DateTimeZoneHandling = overrides.DateTimeZoneHandling,
        DateFormatString     = overrides.DateFormatString,
        FloatParseHandling   = overrides.FloatParseHandling,
      };

      return jreader;
    }


    [NotNull]
    public static JsonSerializerSettings EnableMetadata([CanBeNull] this JsonSerializerSettings settings,
                                                        bool enable = true)
    {
      if (settings is null)
      {
        settings = new JsonSerializerSettings();
      }

      settings.MetadataPropertyHandling = enable ? MetadataPropertyHandling.ReadAhead : MetadataPropertyHandling.Ignore;
        // --> have to use ReadAhead since server uses JavaScript to make JSON,
        // which can give unordered results.
        // I know... The perf!

      return settings;
    }

    [NotNull]
    public static JsonSerializerSettings EnableReferences([CanBeNull] this JsonSerializerSettings settings,
                                                          bool enable = true)
    {
      if (settings is null)
      {
        settings = new JsonSerializerSettings();
      }

      settings.PreserveReferencesHandling = enable ? PreserveReferencesHandling.All : PreserveReferencesHandling.None;

      return settings;
    }

    [NotNull]
    public static JsonSerializerSettings EnableExplicitTypes([CanBeNull] this JsonSerializerSettings settings,
                                                             bool enable = true)
    {
      if (settings is null)
      {
        settings = new JsonSerializerSettings();
      }

      settings.TypeNameHandling = enable ? TypeNameHandling.Objects : TypeNameHandling.None;

      return settings;
    }



    static JsonAuthority()
    {
      JsonConvert.DefaultSettings = () => SerializerSettings;
        // makes the settings defined here in JsonAuthority apply to any default
        // Json.NET serializers created from now on
    }

  } // end class JsonAuthority
}