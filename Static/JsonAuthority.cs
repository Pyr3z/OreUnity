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


    public static IDictionary<string,object> Deserialize(string rawJson, MapMaker  mapMaker  = null,
                                                                         ListMaker listMaker = null)
    {
      switch (Provider)
      {
        case JsonProvider.None:
          var dummyMap = mapMaker?.Invoke(1) ?? DefaultMapMaker(1);
          dummyMap[nameof(rawJson)] = rawJson;
          return dummyMap;

        default:
        case JsonProvider.MiniJson: // TODO pass mapMaker + listMaker thru
          return MiniJson.Deserialize(rawJson) as IDictionary<string,object>;

        case JsonProvider.NewtonsoftJson:
          #if NEWTONSOFT_JSON
          return NewtonsoftAuthority.GenericParse(rawJson, mapMaker, listMaker);
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


    internal static IDictionary<string,object> DefaultMapMaker(int capacity)
    {
      return new HashMap<string,object>(capacity);
    }
    internal static IList<object> DefaultListMaker(int capacity)
    {
      return new object[capacity];
    }


  #region DEPRECATIONS

    [System.Obsolete("Use JsonAuthority.Deserialize(*) instead.")]
    public static IDictionary<string,object> GenericParse(string                                       rawJson,
                                                          System.Func<int, IDictionary<string,object>> mapMaker  = null,
                                                          System.Func<int, IList<object>>              listMaker = null)
    {
      return Deserialize(rawJson, mapMaker, listMaker);
    }

    [System.Obsolete("Use NewtonsoftAuthority.Genericize(*) instead.")]
    public static IDictionary<string,object> Genericize([NotNull] object jObject, [CanBeNull] IDictionary<string,object> map,
                                                        System.Func<int, IDictionary<string,object>> mapMaker = null,
                                                        System.Func<int, IList<object>> listMaker = null)
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
                                           System.Func<int, IList<object>> listMaker = null,
                                           System.Func<int, IDictionary<string,object>> mapMaker = null)
    {
      #if NEWTONSOFT_JSON
      if (jArray is Newtonsoft.Json.Linq.JArray jarr)
      {
        return NewtonsoftAuthority.Genericize(jarr, list, listMaker, mapMaker);
      }
      #endif

      return list;
    }

  #endregion DEPRECATIONS

  } // end class JsonAuthority
}