/*! @file       Tests/Editor/MiscInEditor.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-02-28
**/

using NUnit.Framework;

using UnityEngine;

using Ore;

#if NEWTONSOFT_JSON
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif


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
      ["guid"]                    = System.Guid.NewGuid()
    };

    Assert.DoesNotThrow(() =>
                        {
                          Debug.Log(JObject.FromObject(map).ToString(Formatting.Indented));
                        });

    Assert.DoesNotThrow(() =>
                        {
                          Debug.Log(JObject.FromObject(HashMapParams.Default).ToString(Formatting.Indented));
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

  #endif // NEWTONSOFT_JSON


  [Test]
  public static void ExceptionMessages()
  {
    var nie = new System.NotImplementedException();
    var nse = new System.NotSupportedException();

    Assert.Throws<MultiException>(() =>
                                  {
                                    var mex = MultiException.Create(nse, nie);
                                    Orator.NFE(mex);
                                    throw mex;
                                  });
  }

} // end class MiscInEditor
