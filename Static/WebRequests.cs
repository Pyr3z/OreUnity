/*! @file       Runtime/WebRequests.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-03-17
**/

using UnityEngine.Networking;

using JetBrains.Annotations;
using UnityEngine;


namespace Ore
{
  [PublicAPI]
  public static class WebRequests
  {

    public static Promise<string> Promise([NotNull] this UnityWebRequest request,
                                          string errorSubstring = null)
    {
      var promise = new Promise<string>();

      if (Application.internetReachability == NetworkReachability.NotReachable)
      {
        return promise.Forget();
      }

      UnityWebRequestAsyncOperation asyncOp;

      try
      {
        asyncOp = request.SendWebRequest();
      }
      catch (System.Exception ex)
      {
        return promise.FailWith(ex);
      }

      asyncOp.completed += (ao) =>
      {
        try
        {
          var req = ((UnityWebRequestAsyncOperation)ao).webRequest;

          promise.Maybe(req.downloadHandler.text);

        #if UNITY_2020_1_OR_NEWER
          if (req.result == UnityWebRequest.Result.Success)
        #else
          if (!req.isHttpError && !req.isNetworkError)
        #endif
          {
            if (errorSubstring.IsEmpty() || !promise.Value.Contains(errorSubstring))
            {
              promise.Complete();
            }
            else
            {
              promise.Fail();
            }
          }
          else
          {
            promise.Fail();
          }
        }
        catch (System.Exception ex)
        {
          promise.FailWith(ex);
        }
      };

      return promise;
    }

  } // end static class WebRequests
}