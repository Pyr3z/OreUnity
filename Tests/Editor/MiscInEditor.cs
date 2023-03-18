/*! @file       Tests/Editor/MiscInEditor.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-02-28
**/

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using System.Collections.Generic;
using System.Text.RegularExpressions;

#if NEWTONSOFT_JSON
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif

using Ore;

using AssException = UnityEngine.Assertions.AssertionException;


internal static class MiscInEditor
{

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
                          string json = JsonAuthority.Serialize(map, serializer, sbob);
                          Debug.Log(json);
                        });

    var list = new List<IDictionary<object,object>>
    {
      map
    };

    Assert.DoesNotThrow(() =>
                        {
                          string json = JsonAuthority.Serialize(list, serializer, sbob);
                          Debug.Log(json);
                        });

    var map2 = new HashMap<string,object>
    {
      ["data"] = list
    };

    Assert.DoesNotThrow(() =>
                        {
                          string json = JsonAuthority.Serialize(map2, serializer, sbob);
                          Debug.Log(json);
                        });
  }

  #endif // NEWTONSOFT_JSON


  [Test]
  public static void MultiException()
  {
    var nse = new System.NotSupportedException("(top)");
    var ioe = new System.InvalidOperationException("(middle)");
    var nie = new System.NotImplementedException("(bottom)");

    Assert.Throws<MultiException>(() =>
    {
      var mex = Ore.MultiException.Create(nse, ioe, nie);
      LogAssert.Expect(LogType.Exception, new Regex(@"NotSupportedException: \(top\)"));
      Orator.NFE(mex);
      throw mex;
    });
  }

  [Test]
  public static void AssException()
  {
    Assert.Throws<AssException>(() => throw new AssException("message", "userMessage"));
  }

  [Test]
  public static void OAssertExceptionContract()
  {
    bool expectException = Orator.RaiseExceptions;

    if (expectException)
    {
      Assert.Throws<AssException>(() => OAssert.True(false));
    }
    else
    {
      Assert.DoesNotThrow(() => OAssert.True(false));
    }

    bool ok = false;
    Assert.DoesNotThrow(() =>
    {
      bool pop = LogAssert.ignoreFailingMessages;
      LogAssert.ignoreFailingMessages = true;

      ok = OAssert.Fails(false) &&
          !OAssert.Fails(true) &&
           OAssert.FailsNullCheck(null) &&
           OAssert.FailsNullChecks(new object(), null, new object());

      LogAssert.ignoreFailingMessages = pop;
    });
    Assert.True(ok);
  }


  [Test]
  public static void ActionNullability()
  {
    System.Action action = null;

    Assert.DoesNotThrow(() =>
                        {
                          action += Orator.Reached;
                        });

    Assert.NotNull(action);

    Assert.DoesNotThrow(() =>
                        {
                          action -= Orator.Reached;
                        });

    Assert.Null(action);

    Assert.DoesNotThrow(() =>
                        {
                          action -= Orator.Reached;
                        });

    Assert.Null(action);
  }

} // end class MiscInEditor
