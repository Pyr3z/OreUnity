/*! @file       Static/JsonAuthority.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-01-22
**/

using JetBrains.Annotations;

using System.Text;
using System.Collections.Generic;


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


    public static string Serialize(object data, StringBuilder cachedBuilder = null)
    {
      return Serialize(data, null, cachedBuilder);
    }

    public static string Serialize(object data, object serializer, StringBuilder cachedBuilder = null)
    {
      if (data is null)
        return "null";

      switch (Provider)
      {
        case JsonProvider.None:
          return data.ToString();

        default:
        case JsonProvider.MiniJson:
          return MiniJson.Serialize(data); // TODO utilize cachedBuilder

        case JsonProvider.NewtonsoftJson:
          #if NEWTONSOFT_JSON
          return NewtonsoftAuthority.Serialize(data, serializer as Newtonsoft.Json.JsonSerializer, cachedBuilder);
          #else
          throw new UnanticipatedException("Provider should have never been set to NewtonsoftJson if it isn't available.");
          #endif
      }
    }


    public static object Deserialize(string    rawJson,
                                     MapMaker  mapMaker  = null,
                                     ListMaker listMaker = null)
    {
      switch (Provider)
      {
        case JsonProvider.None:
          return rawJson;

        default:
        case JsonProvider.MiniJson:
          return MiniJson.Deserialize(rawJson); // TODO use mapMaker + listMaker

        case JsonProvider.NewtonsoftJson:
          #if NEWTONSOFT_JSON
          return NewtonsoftAuthority.GenericParse(rawJson, mapMaker, listMaker);
          #else
          throw new UnanticipatedException("Provider should have never been set to NewtonsoftJson if it isn't available.");
          #endif
      }
    }


    public static IDictionary<string,object> DeserializeObject(string    rawJson,
                                                               MapMaker  mapMaker  = null,
                                                               ListMaker listMaker = null)
    {
      switch (Provider)
      {
        case JsonProvider.None:
          var dummyMap = mapMaker?.Invoke(1) ?? DefaultMapMaker(1);
          dummyMap[nameof(rawJson)] = rawJson;
          return dummyMap;

        default:
        case JsonProvider.MiniJson: // TODO use mapMaker + listMaker
          return MiniJson.Deserialize(rawJson) as IDictionary<string,object>;

        case JsonProvider.NewtonsoftJson:
          #if NEWTONSOFT_JSON
          return NewtonsoftAuthority.GenericParseObject(rawJson, mapMaker, listMaker);
          #else
          throw new UnanticipatedException("Provider should have never been set to NewtonsoftJson if it isn't available.");
          #endif
      }
    }

    public static IList<object> DeserializeArray(string    rawJson,
                                                 MapMaker  mapMaker  = null,
                                                 ListMaker listMaker = null)
    {
      switch (Provider)
      {
        case JsonProvider.None:
          var dummyList = listMaker?.Invoke(1) ?? DefaultListMaker(1);
          dummyList.Add(rawJson);
          return dummyList;

        default:
        case JsonProvider.MiniJson: // TODO use mapMaker + listMaker
          return MiniJson.Deserialize(rawJson) as IList<object>;

        case JsonProvider.NewtonsoftJson:
          #if NEWTONSOFT_JSON
          return NewtonsoftAuthority.GenericParseArray(rawJson, mapMaker, listMaker);
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


    public sealed class ProviderScope : System.IDisposable
    {
      public ProviderScope(JsonProvider provider)
      {
        m_Restore = Provider;
        _         = TrySetProvider(provider);
      }

      public void Dispose()
      {
        Provider = m_Restore;
      }

      readonly JsonProvider m_Restore;
    } // end nested class ProviderScope


    internal static IDictionary<string,object> DefaultMapMaker(int capacity)
    {
      return new HashMap<string,object>(capacity);
    }
    internal static IList<object> DefaultListMaker(int capacity)
    {
      return new object[capacity];
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