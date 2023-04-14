/*! @file       Static/JsonAuthority.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-01-22
 *
 *  TODO XML documentation
**/

using JetBrains.Annotations;

using System.Text;
using System.Collections.Generic;

using Type = System.Type;
using TextWriter = System.IO.TextWriter;
using TextReader = System.IO.TextReader;


namespace Ore
{
  using MapMaker  = System.Func<int, IDictionary<string,object>>;
  using ListMaker = System.Func<int, IList<object>>;
    // using these statements because fully-qualified delegates can't participate in C# contravariance


  [PublicAPI]
  public static class JsonAuthority
  {

    public static readonly Encoding Encoding     = Encoding.UTF8;
    public static readonly Encoding WideEncoding = Encoding.Unicode; // LE UTF16

    public static JsonProvider Provider { get; private set; } = JsonProvider.Default;

    public static bool PrettyPrint
    {
      get => s_PrettyPrint;
      set
      {
        s_PrettyPrint = value;
        #if NEWTONSOFT_JSON
        NewtonsoftAuthority.SetPrettyPrint(value);
        #endif
      }
    }


    public static string Serialize([CanBeNull] object data, object serializer = null)
    {
      switch (Provider)
      {
        case JsonProvider.None:
          return data?.ToString() ?? "null";

        default:
        case JsonProvider.MiniJson:
          return MiniJson.Serialize(data, PrettyPrint);

        case JsonProvider.NewtonsoftJson:
        #if NEWTONSOFT_JSON
          using (new RecycledStringBuilder(out var strBuilder))
            return NewtonsoftAuthority.Serialize(data, serializer as Newtonsoft.Json.JsonSerializer, strBuilder);
        #else
          throw new UnanticipatedException("Provider should have never been set to NewtonsoftJson if it isn't available.");
        #endif
      }
    }


    public static object Deserialize(string    json,
                                     MapMaker  mapMaker  = null,
                                     ListMaker listMaker = null)
    {
      switch (Provider)
      {
        case JsonProvider.None:
          return json;

        default:
        case JsonProvider.MiniJson:
          using (new MiniJson.ParserScope(mapMaker, listMaker))
            return MiniJson.Deserialize(json);

        case JsonProvider.NewtonsoftJson:
          #if NEWTONSOFT_JSON
          return NewtonsoftAuthority.GenericParse(json, mapMaker, listMaker);
          #else
          throw new UnanticipatedException("Provider should have never been set to NewtonsoftJson if it isn't available.");
          #endif
      }
    }

    public static object DeserializeObject(string json, Type type,
                                           MapMaker  mapMaker  = null,
                                           ListMaker listMaker = null)
    {
      switch (Provider)
      {
        case JsonProvider.None:
          if (!type?.IsAssignableFrom(typeof(string)) ?? false)
            Orator.Warn(type, "JsonProvider is set to None");
          return json;

        default:
        case JsonProvider.MiniJson:
          using (new MiniJson.ParserScope(mapMaker, listMaker))
            return MiniJson.Deserialize(json, type);

        case JsonProvider.NewtonsoftJson:
          #if NEWTONSOFT_JSON
          // TODO? does not utilize mapMaker, listMaker
          _ = NewtonsoftAuthority.TryDeserializeObject(json, type, out object parsed);
          return parsed;
          #else
          throw new UnanticipatedException("Provider should have never been set to NewtonsoftJson if it isn't available.");
          #endif
      }
    }

    public static IDictionary<string,object> DeserializeObject(string    json,
                                                               MapMaker  mapMaker  = null,
                                                               ListMaker listMaker = null)
    {
      switch (Provider)
      {
        case JsonProvider.None:
          var dummyMap = mapMaker?.Invoke(1) ?? DefaultMapMaker(1);
          dummyMap[nameof(json)] = json;
          return dummyMap;

        default:
        case JsonProvider.MiniJson:
          using (new MiniJson.ParserScope(mapMaker, listMaker))
            return MiniJson.Deserialize(json) as IDictionary<string,object>;

        case JsonProvider.NewtonsoftJson:
          #if NEWTONSOFT_JSON
          return NewtonsoftAuthority.GenericParseObject(json, mapMaker, listMaker);
          #else
          throw new UnanticipatedException("Provider should have never been set to NewtonsoftJson if it isn't available.");
          #endif
      }
    }

    public static IList<object> DeserializeArray(string    json,
                                                 MapMaker  mapMaker  = null,
                                                 ListMaker listMaker = null)
    {
      switch (Provider)
      {
        case JsonProvider.None:
          var dummyList = listMaker?.Invoke(1) ?? DefaultListMaker(1);
          dummyList.Add(json);
          return dummyList;

        default:
        case JsonProvider.MiniJson:
          using (new MiniJson.ParserScope(mapMaker, listMaker))
            return MiniJson.Deserialize(json) as IList<object>;

        case JsonProvider.NewtonsoftJson:
          #if NEWTONSOFT_JSON
          return NewtonsoftAuthority.GenericParseArray(json, mapMaker, listMaker);
          #else
          throw new UnanticipatedException("Provider should have never been set to NewtonsoftJson if it isn't available.");
          #endif
      }
    }


    public static bool TryDeserialize<T>(string json, out T obj,
                                         MapMaker  mapMaker  = null,
                                         ListMaker listMaker = null)
      where T : new()
    {
      switch (Provider)
      {
        case JsonProvider.None:
          if (typeof(T).IsAssignableFrom(typeof(string)))
          {
            obj = (T)(object)json;
            return true;
          }
          obj = default;
          return false;

        default:
        case JsonProvider.MiniJson:
          using (new MiniJson.ParserScope(mapMaker, listMaker))
            return MiniJson.TryDeserialize(json, out obj);

        case JsonProvider.NewtonsoftJson:
          #if NEWTONSOFT_JSON
          // TODO? does not utilize mapMaker, listMaker
          return NewtonsoftAuthority.TryDeserializeObject(json, out obj);
          #else
          throw new UnanticipatedException("Provider should have never been set to NewtonsoftJson if it isn't available.");
          #endif
      }
    }

    public static bool TryDeserializeOverwrite<T>(string json, [NotNull] ref T obj,
                                                  MapMaker  mapMaker  = null,
                                                  ListMaker listMaker = null)
    {
      switch (Provider)
      {
        case JsonProvider.None:
          if (typeof(T).IsAssignableFrom(typeof(string)))
          {
            obj = (T)(object)json;
            return true;
          }
          return false;

        default:
        case JsonProvider.MiniJson:
          using (new MiniJson.ParserScope(mapMaker, listMaker))
            return MiniJson.TryDeserializeOverwrite(json, ref obj);

        case JsonProvider.NewtonsoftJson:
          #if NEWTONSOFT_JSON
          // TODO? does not utilize mapMaker, listMaker
          return NewtonsoftAuthority.TryDeserializeOverwrite(json, ref obj);
          #else
          throw new UnanticipatedException("Provider should have never been set to NewtonsoftJson if it isn't available.");
          #endif
      }
    }


    public static void SerializeTo([NotNull] TextWriter stream, object data, object serializer = null)
    {
      switch (Provider)
      {
        case JsonProvider.None:
          stream.Write(data);
          break;

        default:
        case JsonProvider.MiniJson:
          MiniJson.SerializeTo(stream, data, PrettyPrint);
          break;

        case JsonProvider.NewtonsoftJson:
          #if NEWTONSOFT_JSON
          NewtonsoftAuthority.SerializeTo(stream, data, serializer as Newtonsoft.Json.JsonSerializer);
          break;
          #else
          throw new UnanticipatedException("Provider should have never been set to NewtonsoftJson if it isn't available.");
          #endif
      }
    }

    [CanBeNull]
    public static object DeserializeStream([NotNull] TextReader stream, Type type = null,
                                           MapMaker  mapMaker  = null,
                                           ListMaker listMaker = null)
    {
      switch (Provider)
      {
        case JsonProvider.None:
          if (!type?.IsAssignableFrom(typeof(string)) ?? false)
            Orator.Warn(type, "JsonProvider is set to None");
          return stream.ReadToEnd();

        default:
        case JsonProvider.MiniJson:
          using (new MiniJson.ParserScope(mapMaker, listMaker))
            return MiniJson.DeserializeStream(stream, type);

        case JsonProvider.NewtonsoftJson:
          #if NEWTONSOFT_JSON
          return NewtonsoftAuthority.DeserializeStream(stream, type);
          #else
          throw new UnanticipatedException("Provider should have never been set to NewtonsoftJson if it isn't available.");
          #endif
      }
    }

    public static bool TryDeserializeStream<T>([NotNull] TextReader stream, out T obj,
                                               MapMaker  mapMaker  = null,
                                               ListMaker listMaker = null)
      where T : new()
    {
      switch (Provider)
      {
        case JsonProvider.None:
          if (typeof(T) == typeof(string))
          {
            try
            {
              obj = (T)(object)stream.ReadToEnd();
              return true;
            }
            finally
            {
              stream.Close();
            }
          }
          break;

        default:
        case JsonProvider.MiniJson:
          using (new MiniJson.ParserScope(mapMaker, listMaker))
            return MiniJson.TryDeserializeStream(stream, out obj);

        case JsonProvider.NewtonsoftJson:
          #if NEWTONSOFT_JSON
          return NewtonsoftAuthority.TryDeserializeStream(stream, out obj);
          #else
          throw new UnanticipatedException("Provider should have never been set to NewtonsoftJson if it isn't available.");
          #endif
      }

      obj = default;
      return false;
    }

    public static bool TryDeserializeStreamOverwrite<T>([NotNull] TextReader stream, ref T obj,
                                                        MapMaker  mapMaker  = null,
                                                        ListMaker listMaker = null)
    {
      switch (Provider)
      {
        case JsonProvider.None:
          if (typeof(T) == typeof(string))
          {
            try
            {
              obj = (T)(object)stream.ReadToEnd();
              return true;
            }
            finally
            {
              stream.Close();
            }
          }
          break;

        default:
        case JsonProvider.MiniJson:
          using (new MiniJson.ParserScope(mapMaker, listMaker))
            return MiniJson.TryDeserializeStreamOverwrite(stream, ref obj);

        case JsonProvider.NewtonsoftJson:
          #if NEWTONSOFT_JSON
          // TODO? lazy ReadToEnd()
          return NewtonsoftAuthority.TryDeserializeOverwrite(stream.ReadToEnd(), ref obj);
          #else
          throw new UnanticipatedException("Provider should have never been set to NewtonsoftJson if it isn't available.");
          #endif
      }

      return false;
    }


    public static bool TrySetProvider(JsonProvider provider)
    {
      if (Provider == provider)
        return true;

      if (provider == JsonProvider.NewtonsoftJson)
      {
        #if NEWTONSOFT_JSON
        #else
        return false;
        #endif
      }

      Provider = provider;
      return true;
    }


    public struct Scope : System.IDisposable
    {
      public Scope(JsonProvider provider)
      {
        m_Disposable = true;
        m_Restore    = Provider;
        _            = TrySetProvider(provider);
        m_WasPretty  = PrettyPrint;
      }

      public Scope(bool prettyPrint)
      {
        m_Disposable = true;
        m_Restore    = Provider;
        m_WasPretty  = PrettyPrint;
        PrettyPrint  = prettyPrint;
      }

      public Scope(JsonProvider provider, bool prettyPrint)
      {
        m_Disposable = true;
        m_Restore    = Provider;
        _            = TrySetProvider(provider);
        m_WasPretty  = PrettyPrint;
        PrettyPrint  = prettyPrint;
      }

      void System.IDisposable.Dispose()
      {
        if (m_Disposable)
        {
          Provider     = m_Restore;
          PrettyPrint  = m_WasPretty;
          m_Disposable = false;
        }
      }

      bool m_Disposable;

      readonly JsonProvider m_Restore;
      readonly bool         m_WasPretty;

    } // end nested class Scope


    static bool s_PrettyPrint = EditorBridge.IS_DEBUG;


    internal static IDictionary<string,object> DefaultMapMaker(int capacity)
    {
      return new HashMap<string,object>(capacity);
    }
    internal static IList<object> DefaultListMaker(int capacity)
    {
      return new List<object>(capacity);
    }


  #region DEPRECATIONS

    [System.Obsolete("Use JsonAuthority.DeserializeObject(*) instead.")]
    public static IDictionary<string,object> GenericParse(string    rawJson,
                                                          MapMaker  mapMaker  = null,
                                                          ListMaker listMaker = null)
    {
      return DeserializeObject(rawJson, mapMaker, listMaker);
    }

    [System.Obsolete("Use NewtonsoftAuthority.Genericize(*) instead.")]
    public static IDictionary<string,object> Genericize([NotNull] object jObject, [CanBeNull] IDictionary<string,object> map,
                                                        MapMaker  mapMaker  = null,
                                                        ListMaker listMaker = null)
    {
      #if NEWTONSOFT_JSON
      if (jObject is Newtonsoft.Json.Linq.JObject jobj)
      {
        return NewtonsoftAuthority.Genericize(jobj, map, mapMaker, listMaker);
      }
      #endif

      return map;
    }

    [System.Obsolete("Use NewtonsoftAuthority.Genericize(*) instead.")]
    public static IList<object> Genericize([NotNull] object jArray, [CanBeNull] IList<object> list,
                                           ListMaker listMaker = null,
                                           MapMaker  mapMaker  = null)
    {
      #if NEWTONSOFT_JSON
      if (jArray is Newtonsoft.Json.Linq.JArray jarr)
      {
        return NewtonsoftAuthority.Genericize(jarr, list, mapMaker, listMaker);
      }
      #endif

      return list;
    }

  #endregion DEPRECATIONS

  } // end class JsonAuthority
}