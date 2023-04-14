/*! @file       Static/JsonAuthority.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-01-22
**/

using JetBrains.Annotations;

using System.Text;
using System.Collections.Generic;

using Type = System.Type;


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


    public static string Serialize(object data)
    {
      return Serialize(data, null);
    }

    public static string Serialize(object data, object serializer)
    {
      if (data is null)
        return "null";

      switch (Provider)
      {
        case JsonProvider.None:
          return data.ToString(); // TODO simple printer for lists / dicts

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

    public static object Deserialize(string json, Type type,
                                     MapMaker  mapMaker  = null,
                                     ListMaker listMaker = null)
    {
      switch (Provider)
      {
        case JsonProvider.None:
          if (!type.IsAssignableFrom(typeof(string)))
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