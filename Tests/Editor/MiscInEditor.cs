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

  #endif // NEWTONSOFT_JSON

} // end class MiscInEditor
