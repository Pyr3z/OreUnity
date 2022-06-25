/** @file       Editor/EditorSettingsValidator.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-23
**/

using UnityEngine;
using UnityEditor;


namespace Bore
{

  public class AssetValidator : UnityEditor.AssetModificationProcessor
  {
                         /* IDE0051 => "private method is unused" */
    #pragma warning disable IDE0051

    /* Available messages: (https://docs.unity3d.com/ScriptReference/AssetModificationProcessor.html)
     *  static bool CanOpenForEdit(string[] paths, List<string> outbadpaths, StatusQueryOptions opts)
     *  static void FileModeChanged(string[] paths, FileMode mode)
     *  static bool IsOpenForEdit(string[] paths, List<string> outbadpaths, StatusQueryOptions opts)
     *  static bool MakeEditable(string[] paths, string prompt, List<string> outbadpaths)
     *  static void OnWillCreateAsset(string name)
     *  static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions opts)
     *  static void OnWillMoveAsset(string src, string dest)
     *  static void OnWillSaveAssets(string[] paths)
     */

    private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions _ )
    {
      var asset = AssetDatabase.LoadAssetAtPath<Asset>(path);

      if (asset is IImmortalSingleton)
      {
        Debug.LogWarning($"I tried to delete \"{asset}\", but it is marked as an immortal singleton so I didn't.");
        return AssetDeleteResult.FailedDelete;
      }

      return AssetDeleteResult.DidNotDelete;
    }

    #pragma warning restore IDE0051

  } // end private class AssetValidator

}
