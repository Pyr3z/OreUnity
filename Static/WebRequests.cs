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

    /// <summary>
    ///   Handy extension to detect web request successfulness regardless of
    ///   Unity 2019 or 2020+.
    /// </summary>
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


    /// <summary>
    ///   Handy extension to get a request's error info for printing, regardless
    ///   of Unity 2019 vs 2020+.
    /// </summary>
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

    /// <summary>
    ///   Creates a <see cref="Promise{T}"/> object for the given web request. <br/> <br/>
    ///   As the name suggests, your request should be preconstructed to handle
    ///   downloading <b>text</b> (encoded in UTF8). <br/> <br/>
    ///   Does not support calling multiple times on the same request instance. <br/> <br/>
    ///   Finally, this API makes the firm guarantee that your request will be
    ///   disposed of properly in virtually all cases, so you should not attempt
    ///   to do any additional disposal on the request after calling this.
    /// </summary>
    ///
    /// <param name="request">
    ///   Assumed not-null, not-already-sent, and to have a valid
    ///   <see cref="DownloadHandlerBuffer"/> already attached.
    /// </param>
    ///
    /// <param name="errorSubstring">
    ///   If provided, the presence of this substring in the downloadHandler's
    ///   text body indicates an error has occurred, even if the HTTP result code
    ///   is 2XX.
    /// </param>
    ///
    /// <param name="promise">
    ///   If provided, this call will reuse an existing <see cref="Promise{T}"/>
    ///   object for the retval, instead of instantiating a new one. <br/>
    ///   <b>Note: This helper will not call promise.<see cref="Promise{T}.Reset"/> for
    ///   you</b>, so it's <i>your</i> responsibility to call it beforehand (if
    ///   applicable)!
    /// </param>
    public static Promise<string> PromiseText([NotNull] this UnityWebRequest request,
                                              string   errorSubstring = null,
                                              Promise<string> promise = null)
    {
      if (promise is null)
        promise = new Promise<string>();
      // else, intentionally NOT going to call promise.Reset() for you

      if (Application.internetReachability == NetworkReachability.NotReachable)
      {
        request.Dispose();
        return promise.Forget();
      }

      // some light validation
      if (!(request.downloadHandler is DownloadHandlerBuffer downloader))
      {
        request.Dispose();
        return promise.FailWith(new System.NullReferenceException(
                                  "request.downloadHandler as DownloadHandlerBuffer"
                                ));
      }

      UnityWebRequestAsyncOperation asyncOp;

      try
      {
        asyncOp = request.SendWebRequest();

        if (asyncOp is null)
        {
          request.Dispose();
          return promise.FailWith(new System.InvalidOperationException(request.url));
        }
      }
      catch (System.Exception ex)
      {
        request.Dispose();
        return promise.FailWith(ex);
      }

      asyncOp.completed += _ =>
      {
        try
        {
          #if UNITY_2020_1_OR_NEWER
          if (!downloader.error.IsEmpty())
          {
            promise.FailWith(downloader.error);
            return;
          }
          #endif

          OAssert.True(downloader.isDone);

          string response = downloader.text;

          if (request.Succeeded())
          {
            promise.Maybe(response);

            if (errorSubstring.IsEmpty() || !response.Contains(errorSubstring))
              promise.Complete();
            else
              promise.Fail();
          }
          else
          {
            promise.FailWith(request.error);
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