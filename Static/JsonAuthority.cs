/*! @file       Static/JsonAuthority.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-01-22
**/

using JetBrains.Annotations;

#if NEWTONSOFT_JSON
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif

using System.Collections;
using System.Linq;

using UnityEngine;

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


    #if NEWTONSOFT_JSON

    public static Formatting Formatting => SerializerSettings.Formatting;


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


    [NotNull]
    public static IList<object> FixupNestedContainers([NotNull] IList<object> list, int maxRecursionDepth = 3)
    {
      int ilen = list.Count;

      if (maxRecursionDepth < 0)
      {
        Orator.Error($"{nameof(JsonAuthority)}.{nameof(FixupNestedContainers)}: Max recursion depth reached.");
        return list;
      }

      for (int i = 0; i < ilen; ++i)
      {
        switch (list[i])
        {
          case JValue jval:
            list[i] = jval.Value;
            break;

          case JObject jobj:
            var newMap = new HashMap<string,object>(jobj.Count);

            foreach (var property in jobj)
            {
              newMap.Add(property.Key, property.Value);
            }

            list[i] = FixupNestedContainers(newMap, maxRecursionDepth - 1); // fake recursion
            break;

          case JArray jarr:
            var newArr = new object[jarr.Count];

            for (int n = 0; n < newArr.Length; ++n)
            {
              newArr[n] = jarr[n];
            }

            list[i] = FixupNestedContainers(newArr, maxRecursionDepth - 1); // regrettable recursion
            break;

          case JToken jtok:
            Orator.Reached($"ping Levi? type={jtok.Type}");
            list[i] = jtok.ToString(Formatting.None);
            break;
        }
      }

      return list;
    }

    [NotNull]
    public static Dictionary<string,object> FixupNestedContainers([NotNull] Dictionary<string,object> dict, int maxRecursionDepth = 3)
    {
      if (maxRecursionDepth < 0)
      {
        Orator.Error($"{nameof(JsonAuthority)}.{nameof(FixupNestedContainers)}: Max recursion depth reached.");
        return dict;
      }

      var sneakySwap = new Dictionary<string,object>(dict.Count);

      foreach (var pair in dict)
      {
        switch (pair.Value)
        {
          case JValue jval:
            sneakySwap.Add(pair.Key, jval.Value);
            break;

          case JObject jobj:
            var newMap = new HashMap<string,object>(jobj.Count);

            foreach (var property in jobj)
            {
              newMap.Map(property.Key, property.Value);
            }

            sneakySwap.Add(pair.Key, FixupNestedContainers(newMap, maxRecursionDepth - 1));
            break;

          case JArray jarr:
            var newArr = new object[jarr.Count];

            for (int i = 0; i < newArr.Length; ++i)
            {
              newArr[i] = jarr[i];
            }

            sneakySwap.Add(pair.Key, FixupNestedContainers(newArr, maxRecursionDepth - 1));
            break;

          case JToken jtok:
            Orator.Reached($"ping Levi? type={jtok.Type}");
            sneakySwap.Add(pair.Key, jtok.ToString(Formatting.None));
            break;

          default:
            sneakySwap.Add(pair.Key, pair.Value);
            break;
        }
      }

      return sneakySwap;
    }

    [NotNull]
    public static HashMap<string,object> FixupNestedContainers([NotNull] HashMap<string,object> map, int maxRecursionDepth = 3)
    {
      if (maxRecursionDepth < 0)
      {
        Orator.Error($"{nameof(JsonAuthority)}.{nameof(FixupNestedContainers)}: Max recursion depth reached.");
        return map;
      }

      // HashMap algo optimization <3

      using (var it = map.GetEnumerator())
      {
        while (it.MoveNext())
        {
          switch (it.CurrentValue)
          {
            case JValue jval:
              it.RemapCurrent(jval.Value);
              break;

            case JObject jobj:
              var newMap = new HashMap<string,object>(jobj.Count);

              foreach (var property in jobj)
              {
                newMap.Map(property.Key, property.Value);
              }

              it.RemapCurrent(FixupNestedContainers(newMap, maxRecursionDepth - 1)); // regrettable recursion
              break;

            case JArray jarr:
              var newArr = new object[jarr.Count];

              for (int j = 0; j < newArr.Length; ++j)
              {
                newArr[j] = jarr[j];
              }

              it.RemapCurrent(FixupNestedContainers(newArr, maxRecursionDepth - 1)); // fake recursion
              break;

            case JToken jtok:
              Orator.Reached($"ping Levi? type={jtok.Type}");
              it.RemapCurrent(jtok.ToString(Formatting.None));
              break;
          }
        }
      }

      return map;
    }


    [NotNull]
    public static HashMap<string,object> Genericize([NotNull] JObject jObject, [CanBeNull] HashMap<string,object> map,
                                                    System.Func<int, HashMap<string,object>> mapMaker = null,
                                                    System.Func<int, IList<object>> listMaker = null)
    {
      static HashMap<string,object> defaultMapMaker(int cap) => new HashMap<string,object>(cap);

      static IList<object> defaultListMaker(int cap) => new object[cap];

      if (mapMaker is null)
      {
        mapMaker = defaultMapMaker;
      }

      if (listMaker is null)
      {
        listMaker = defaultListMaker;
      }

      if (map is null)
      {
        map = mapMaker(jObject.Count);
      }
      else
      {
        map.Clear();
        map.EnsureCapacity(jObject.Count);
      }

      foreach (var property in jObject)
      {
        switch (property.Value)
        {
          case null:
            continue;

          case JValue jval:
            map[property.Key] = jval.Value;
            break;

          case JObject jobj:
            map[property.Key] = Genericize(jobj, null, mapMaker, listMaker);
            break;

          case JArray jarr:
            map[property.Key] = Genericize(jarr, null, listMaker, mapMaker);
            break;

          default:
            Orator.Reached($"ping Levi? type={property.Value.Type}");
            break;
        }
      }

      return map;
    }

    public static IList<object> Genericize([NotNull] JArray jArray, [CanBeNull] IList<object> list,
                                           System.Func<int, IList<object>> listMaker = null,
                                           System.Func<int, HashMap<string,object>> mapMaker = null)
    {
      static IList<object> defaultListMaker(int cap) => new object[cap];

      static HashMap<string,object> defaultMapMaker(int cap) => new HashMap<string,object>(cap);

      if (listMaker is null)
      {
        listMaker = defaultListMaker;
      }

      if (mapMaker is null)
      {
        mapMaker = defaultMapMaker;
      }

      if (list is null)
      {
        list = listMaker(jArray.Count);
      }
      else
      {
        list.Clear();
      }

      for (int i = 0; i < list.Count; ++i)
      {
        switch (jArray[i])
        {
          case JValue jval:
            list.Add(jval.Value);
            break;

          case JObject jobj:
            list.Add(Genericize(jobj, null, mapMaker, listMaker));
            break;

          case JArray jarr:
            list.Add(Genericize(jarr, null, listMaker, mapMaker));
            break;

          default:
            Orator.Reached($"ping Levi? type={jArray[i].Type}");
            break;
        }
      }

      return list;
    }


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    #if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
    #endif
    static void OverrideDefaultSettings()
    {
      JsonConvert.DefaultSettings = () => SerializerSettings;
        // makes the settings defined here in JsonAuthority apply to any default
        // Json.NET serializers created from now on
    }

    #endif // NEWTONSOFT_JSON

  } // end class JsonAuthority
}