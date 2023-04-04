/*! @file       Static/JsonAuthority+Newtonsoft.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-04-03
**/

#if NEWTONSOFT_JSON
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
#endif

using JetBrains.Annotations;

using System.Collections.Generic;
using System.Linq;

using System.IO;
using System.Text;

using StringComparison = System.StringComparison;
using CultureInfo      = System.Globalization.CultureInfo;


namespace Ore
{
  using MapMaker  = System.Func<int, IDictionary<string,object>>;
  using ListMaker = System.Func<int, IList<object>>;
    // using these statements because fully-qualified delegates can't participate in C# contravariance

  [PublicAPI]
  public static class NewtonsoftAuthority
  {
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
    public static string Serialize(object obj, JsonSerializer serializer = null, StringBuilder cachedBuilder = null)
    {
      if (serializer is null)
        serializer = JsonSerializer.CreateDefault(SerializerSettings);

      if (cachedBuilder is null)
        cachedBuilder = new StringBuilder(2048);
      else
        cachedBuilder.Clear();

      var strWriter = new StringWriter(cachedBuilder, SerializerSettings.Culture);

      using (var jsonWriter = new JsonTextWriter(strWriter))
      {
        jsonWriter.Formatting = Formatting;
        serializer.Serialize(jsonWriter, obj);
      }

      return strWriter.ToString();
    }


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


    [CanBeNull]
    public static object GenericParse(string    rawJson,
                                      MapMaker  mapMaker  = null,
                                      ListMaker listMaker = null)
    {
      var parsed = JsonConvert.DeserializeObject(rawJson);

      if (parsed is null)
        return null;

      if (parsed is JValue jval)
      {
        return jval.Value;
      }
      if (parsed is JObject jobj)
      {
        return Genericize(jobj, map: null, mapMaker, listMaker);
      }
      if (parsed is JArray jarr)
      {
        return Genericize(jarr, list: null, mapMaker, listMaker);
      }

      return parsed;
    }

    [NotNull]
    public static IDictionary<string,object> GenericParseObject(string    rawJson,
                                                                MapMaker  mapMaker  = null,
                                                                ListMaker listMaker = null)
    {
      if (mapMaker is null)
      {
        mapMaker = JsonAuthority.DefaultMapMaker;
      }

      var jobj = JsonConvert.DeserializeObject<JObject>(rawJson, SerializerSettings);

      if (jobj is null)
      {
        return mapMaker(1);
      }

      return Genericize(jobj, null, mapMaker: mapMaker, listMaker: listMaker);
    }

    [NotNull]
    public static IList<object> GenericParseArray(string    rawJson,
                                                  MapMaker  mapMaker  = null,
                                                  ListMaker listMaker = null)
    {
      if (listMaker is null)
      {
        listMaker = JsonAuthority.DefaultListMaker;
      }

      var jarr = JsonConvert.DeserializeObject<JArray>(rawJson, SerializerSettings);

      if (jarr is null)
      {
        return listMaker(1);
      }

      return Genericize(jarr, null, mapMaker: mapMaker, listMaker: listMaker);
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
    public static IDictionary<string,object> Genericize([NotNull] JObject jObject, [CanBeNull] IDictionary<string,object> map,
                                                        MapMaker  mapMaker  = null,
                                                        ListMaker listMaker = null)
    {
      if (mapMaker is null)
      {
        mapMaker = JsonAuthority.DefaultMapMaker;
      }

      if (listMaker is null)
      {
        listMaker = JsonAuthority.DefaultListMaker;
      }

      if (map is null)
      {
        map = mapMaker(jObject.Count);
      }
      else
      {
        map.Clear();

        if (map is HashMap<string,object> hashMap)
        {
          hashMap.EnsureCapacity(jObject.Count);
        }
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
            map[property.Key] = Genericize(jobj, map: null, mapMaker, listMaker);
            break;

          case JArray jarr:
            map[property.Key] = Genericize(jarr, list: null, mapMaker, listMaker);
            break;

          default:
            throw new UnanticipatedException($"Json.NET JToken type={property.Value.Type}");
        }
      }

      return map;
    }

    public static IList<object> Genericize([NotNull] JArray jArray, [CanBeNull] IList<object> list,
                                           MapMaker  mapMaker  = null,
                                           ListMaker listMaker = null)
    {
      if (listMaker is null)
      {
        listMaker = JsonAuthority.DefaultListMaker;
      }

      if (mapMaker is null)
      {
        mapMaker = JsonAuthority.DefaultMapMaker;
      }

      if (list is null)
      {
        list = listMaker(jArray.Count);
      }
      else
      {
        list.Clear();
      }

      foreach (var token in jArray)
      {
        switch (token)
        {
          case JValue jval:
            list.Add(jval.Value);
            break;

          case JObject jobj:
            list.Add(Genericize(jobj, map: null, mapMaker, listMaker));
            break;

          case JArray jarr:
            list.Add(Genericize(jarr, list: null, mapMaker, listMaker));
            break;

          case null:
            throw new UnanticipatedException("Json.NET JToken type=<null>");

          default:
            throw new UnanticipatedException($"Json.NET JToken type={token.Type}");
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


  #else // !NEWTONSOFT_JSON

    // TODO ?

  #endif // !NEWTONSOFT_JSON

  } // end static class NewtonsoftAuthority

}

