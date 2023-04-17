/*! @file       Runtime/JsonAuthorityInEditor.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-08
**/

using Ore;

using NUnit.Framework;
using NUnit.Framework.Constraints;

using UnityEngine;

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

  internal static readonly (string json, object expected)[] TestJsons =
  {
    ("null", null),
    ("true", true),
    ("123",  123),
    ("{\"fef\":123}", new JsonObj { ["fef"] = 123 }),
    ("{\n  \"m_Fef12\": \"feffity fef fef\\nfef.\"\n}", new JsonObj { ["m_Fef12"] = "feffity fef fef\nfef." }),
    ("[1,2,3]", new JsonArr { 1, 2, 3 }),
    ("{\"fefArray\": [\"fef1\", \"fef2\"], \"nothing\": true}", new JsonObj { ["fefArray"] = new JsonArr { "fef1", "fef2" }, 
                                                                              ["nothing"]  = true }),
  };


  internal static void AssertAreEqual(object expected, object actual, string message = null)
  {
    if (expected is null)
    {
      Assert.Null(actual, message);
    }
    else if (expected is IDictionary<string,object> jobj)
    {
      var dict = actual as IDictionary<string,object>;

      Assert.NotNull(dict, message);

      int countdown = jobj.Count;

      foreach (var kvp in dict)
      {
        Assert.True(jobj.TryGetValue(kvp.Key, out var exp), $"(did not contain kvp: {kvp})");

        // regrettable recursion
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
    else if (expected is string)
    {
      var isExpected = new EqualConstraint(expected)
        .Using((System.Comparison<string>)Strings.CompareIgnoreWhitespace);
      Assert.That(actual, isExpected);
    }
    else if (expected is System.TimeSpan span)
    {
      expected = span.Ticks;

      Assert.AreEqual(expected, actual, message);
    }
    else
    {
      var enumType = expected.GetType();
      if (enumType.IsEnum)
      {
        if (actual is string enumStr)
        {
          actual = System.Enum.Parse(enumType, enumStr);
        }
        else if (actual is long enumVal)
        {
          actual = System.Enum.ToObject(enumType, enumVal);
        }
        else
        {
          Assert.Fail($"enum conversion: from \"{actual}\" to {enumType.Name}.{expected}");
        }

        Assert.NotNull(actual, "actual");
      }

      Assert.AreEqual(expected, actual, message);
    }
  }


  [Test]
  public static void SerializeCurrentProvider([Values(true, false)] bool pretty)
  {
    using (new JsonAuthority.Scope(pretty))
    {
      Assert.AreEqual(JsonAuthority.PrettyPrint, pretty, "pretty");

      Debug.Log($"JsonAuthority.PrettyPrint: {pretty}");
      Debug.Log($"JsonAuthority.Provider: {JsonAuthority.Provider}");

      foreach (var (expected, test) in TestJsons)
      {
        string json = JsonAuthority.Serialize(test);

        AssertAreEqual(expected, json);
      }
    }
  }

  [Test]
  public static void DeserializeCurrentProvider()
  {
    Debug.Log($"JsonAuthority.Provider: {JsonAuthority.Provider}");

    foreach (var (test, expected) in TestJsons)
    {
      object parsed = JsonAuthority.Deserialize(test);

      AssertAreEqual(expected, parsed, $"'{test}' (parsed type: {parsed?.GetType().Name ?? "null"})");
    }
  }

  // [Test]
  public static void DeserializeOneTest([Values(0, 1, 2, 3, 4, 5, 6)] int idx)
  {
    Assert.True(idx.IsIndexTo(TestJsons), "test.IsIndexTo(TestJsons)");

    var (test, expected) = TestJsons[idx];

    object parsed = JsonAuthority.Deserialize(test);

    AssertAreEqual(expected, parsed, $"'{test}' (parsed type: {parsed?.GetType().Name ?? "null"})");
  }



  [Test]
  public static void Serialize([Values(true, false)] bool pretty,
                               [Values(JsonProvider.MiniJson, JsonProvider.NewtonsoftJson)] JsonProvider provider)
  {
    using (new JsonAuthority.Scope(provider))
    {
      Assert.AreEqual(provider, JsonAuthority.Provider);

      if (provider == JsonProvider.NewtonsoftJson)
      {
        #if NEWTONSOFT_JSON
        Assert.AreEqual(pretty ? Formatting.Indented : Formatting.None,
                        NewtonsoftAuthority.SerializerSettings.Formatting,
                        "SerializerSettings.Formatting");
        #endif
      }

      SerializeCurrentProvider(pretty);
    }
  }

  [Test]
  public static void Deserialize([Values(JsonProvider.MiniJson, JsonProvider.NewtonsoftJson)] JsonProvider provider)
  {
    using (new JsonAuthority.Scope(provider))
    {
      Assert.AreEqual(provider, JsonAuthority.Provider);

      DeserializeCurrentProvider();
    }
  }


  [Test]
  public static void ReflectSerialize([Values(true, false)] bool pretty,
                                      [Values(JsonProvider.MiniJson, JsonProvider.NewtonsoftJson)] JsonProvider provider)
  {
    using (new JsonAuthority.Scope(provider, pretty))
    {
      var ti = TimeInterval.OfSeconds(1);

      string expected = $"{{\"Ticks\":{ti.Ticks},\"m_AsFrames\":{ti.TicksAreFrames.ToInvariantLower()}}}";
      string actual   = JsonAuthority.Serialize(ti);

      Debug.Log(actual);

      AssertAreEqual(expected, actual, nameof(TimeInterval));

      var ivec = new Vector2Int(3, 4);

      expected = $"{{\"m_X\":{ivec.x},\"m_Y\":{ivec.y}}}";
      actual   = JsonAuthority.Serialize(ivec);

      Debug.Log(actual);

      AssertAreEqual(expected, actual, nameof(Vector2Int));
    }
  }

  [Test]
  public static void ReflectStatic([Values(true, false)] bool pretty,
                                   [Values(JsonProvider.MiniJson, JsonProvider.NewtonsoftJson)] JsonProvider provider)
  {
    using (new JsonAuthority.Scope(provider, pretty))
    {
      string json = JsonAuthority.Serialize(typeof(DeviceSpy));

      Debug.Log(json);

      Assert.IsNotEmpty(json);
      Assert.Greater(json.Length, 2);

      var parsed = JsonAuthority.DeserializeObject(json);

      Assert.NotNull(parsed, "parsed");
      Assert.Positive(parsed.Count, "parsed.Count");

      foreach (var kvp in parsed)
      {
        var prop = typeof(DeviceSpy).GetProperty(kvp.Key);

        Assert.NotNull(prop, $"DeviceSpy.{kvp.Key}");

        var expected = prop.GetMethod.Invoke(null, System.Array.Empty<object>());

        Assert.NotNull(expected, "expected");

        var actual = kvp.Value;

        Assert.NotNull(actual, "actual");

        AssertAreEqual(expected, actual, $"DeviceSpy.{prop.Name}");
      }
    }
  }



#if NEWTONSOFT_JSON

  [Test]
  public static void NsoftMiscJsonFromMap()
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
  public static void NsoftMiscJsonToObject()
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
  public static void NsoftMiscJsonSerialize()
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
