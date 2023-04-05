/*! @file       Runtime/JsonAuthorityInEditor.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-08
**/

using Ore;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using System.Collections;
using System.Collections.Generic;

#if NEWTONSOFT_JSON
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif

using JsonObj = System.Collections.Generic.Dictionary<string,object>;
using JsonArr = System.Collections.Generic.List<object>;


// ReSharper disable once CheckNamespace
internal static class JsonAuthorityInEditor
{

  static readonly (string json, object expected)[] s_TestJsons = new (string json, object expected)[]
  {
    ("null", null),
    ("true", true),
    ("123",  123),
    ("{\"fef\":123}", new JsonObj { ["fef"] = 123 }),
    ("{\n  \"m_Fef12\": \"feffity fef fef\\nfef.\"\n}", new JsonObj { ["m_Fef12"] = "feffity fef fef\nfef." }),
    ("[1,2,3]", new JsonArr { 1, 2, 3 }),
    ("{\"fefArray\": ['f', 'e', 'f'], \"nothing\": true}", new JsonObj { ["fefArray"] = new JsonArr { 'f', 'e', 'f' },
                                                                         ["nothing"]  = true }),
  };


  static void AssertAreEqual(object expected, object actual, string message = null)
  {
    if (actual is null)
    {
      Assert.Null(expected, message);
      return;
    }

    if (expected is IDictionary<string,object> jobj)
    {
      var dict = actual as IDictionary<string,object>;

      Assert.NotNull(dict, message);

      int countdown = jobj.Count;

      foreach (var kvp in dict)
      {
        Assert.True(jobj.TryGetValue(kvp.Key, out var exp), $"(did not contain kvp: {kvp})");

        AssertAreEqual(exp, kvp.Value, message);

        -- countdown;
      }

      Assert.Zero(countdown, message);
    }
    else if (expected is ICollection jarr)
    {
      var list = actual as ICollection;

      Assert.NotNull(list, message);

      int countdown = jarr.Count;

      foreach (var item in list)
      {
        Assert.Contains(item, jarr, message);
        -- countdown;
      }

      Assert.Zero(countdown, message);
    }
    else
    {
      Assert.AreEqual(expected, actual, message);
    }
  }


  [Test]
  public static void Serialize()
  {
    Assert.Inconclusive("test not implemented.");
  }

  [Test]
  public static void Deserialize()
  {
    Debug.Log($"JsonAuthority.Provider: {JsonAuthority.Provider}");

    foreach (var (test, expected) in s_TestJsons)
    {
      object parsed = JsonAuthority.Deserialize(test);

      AssertAreEqual(expected, parsed, $"'{test}' (parsed type: {parsed?.GetType().Name ?? "null"})");
    }
  }



  [Test]
  public static void MiniJsonSerialize()
  {
    using (new JsonAuthority.ProviderScope(JsonProvider.MiniJson))
    {
      Assert.AreEqual(JsonProvider.MiniJson, JsonAuthority.Provider);

      Serialize();
    }
  }

  [Test]
  public static void MiniJsonDeserialize()
  {
    using (new JsonAuthority.ProviderScope(JsonProvider.MiniJson))
    {
      Assert.AreEqual(JsonProvider.MiniJson, JsonAuthority.Provider);

      Deserialize();
    }
  }



#if NEWTONSOFT_JSON

  [Test]
  public static void NewtonsoftJsonFromObject()
  {
    var map = new HashMap<object,object>
    {
      ["fef"]                     = DeviceDimension.DisplayHz,
      [DeviceDimension.OSVersion] = DeviceSpy.OSVersion,
      ["guid"]                    = System.Guid.NewGuid(),
      ["platform"]                = Application.platform,
      ["platformStr"]             = Application.platform.ToInvariant(),
    };

    var serializer = JsonSerializer.CreateDefault();

    Assert.DoesNotThrow(() =>
                        {
                          var jobj = JObject.FromObject(map, serializer);
                          Debug.Log(jobj.ToString(Formatting.Indented));
                        });

    Assert.DoesNotThrow(() =>
                        {
                          var jobj = JObject.FromObject(HashMapParams.Default, serializer);
                          Debug.Log(jobj.ToString(Formatting.Indented));
                        });

    Assert.DoesNotThrow(() =>
                        {
                          var jarr = new JArray(JObject.FromObject(map, serializer));
                          Debug.Log(jarr.ToString(Formatting.Indented));
                        });
  }

  [Test]
  public static void NewtonsoftJsonToObject()
  {
    string jObj = @"{
    ""fef"": ""fef!"",
    ""one"": 1
    }";

    var map = JsonConvert.DeserializeObject<HashMap<string,object>>(jObj);

    Assert.NotNull(map);
    Assert.Positive(map.Count);
    Assert.True(map.Find("fef", out var fef) && fef.ToString() == "fef!");
    Assert.True(map.Find("one", out var one) && one.GetHashCode() == 1);
  }

  [Test]
  public static void NewtonsoftJsonSerialize()
  {
    var serializer = JsonSerializer.CreateDefault();
    var sbob = new System.Text.StringBuilder(2048);

    var map = new HashMap<object,object>
    {
      ["fef"]                     = DeviceDimension.DisplayHz,
      [DeviceDimension.OSVersion] = DeviceSpy.OSVersion,
      ["guid"]                    = System.Guid.NewGuid(),
      ["platform"]                = Application.platform,
      ["platformStr"]             = Application.platform.ToInvariant(),
    };

    Assert.DoesNotThrow(() =>
                        {
                          string json = NewtonsoftAuthority.Serialize(map, serializer, sbob);
                          Debug.Log(json);
                        });

    var list = new List<IDictionary<object,object>>
    {
      map
    };

    Assert.DoesNotThrow(() =>
                        {
                          string json = NewtonsoftAuthority.Serialize(list, serializer, sbob);
                          Debug.Log(json);
                        });

    var map2 = new HashMap<string,object>
    {
      ["data"] = list
    };

    Assert.DoesNotThrow(() =>
                        {
                          string json = NewtonsoftAuthority.Serialize(map2, serializer, sbob);
                          Debug.Log(json);
                        });
  }

#endif // NEWTONSOFT_JSON

} // end class JsonAuthorityInEditor
