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

    public static bool Succeeded([CanBeNull] this UnityWebRequest request)
    {
      // ReSharper disable once MergeIntoPattern
      return request != null &&
             #if UNITY_2020_1_OR_NEWER
             request.result == UnityWebRequest.Result.Success;
             #else
             !request.isNetworkError && !request.isHttpError;
             #endif
    }

    public static string GetErrorInfo([NotNull] this UnityWebRequest request)
    {
      #if UNITY_2020_1_OR_NEWER

      return $"result={request.result}, error=\"{request.error}\"\n" +
             $"response={request.downloadHandler?.text}";

      #else

      return $"isHttpError={request.isHttpError}, isNetworkError={request.isNetworkError}, error=\"{request.error}\"\n" +
             $"response={request.downloadHandler?.text}";

      #endif
    }

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

        if (asyncOp is null)
        {
          return promise.FailWith(new System.InvalidOperationException(request.url));
        }
      }
      catch (System.Exception ex)
      {
        return promise.FailWith(ex);
      }

      asyncOp.completed += _ =>
      {
        try
        {
          promise.Maybe(request.downloadHandler.text);

          if (request.Succeeded() && ( errorSubstring.IsEmpty() ||
                                       !promise.Value.Contains(errorSubstring) ))
          {
            promise.Complete();
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
        finally
        {
          request.Dispose();
        }
      };

      return promise;
    }

  } // end static class WebRequests
}