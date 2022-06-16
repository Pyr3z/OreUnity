/** @file       Editor/SerializedProperties.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-15
**/

using UnityEditor;


namespace Bore
{

  public static class SerializedProperties
  {

    public static bool IsDisposed(this SerializedProperty prop)
    {
      // JESUS CHRIST WAS IT SO HARD TO PROVIDE A METHOD LIKE THIS, UNITY ?!

      // I HAD TO SCOUR THE SOURCE CODE IN ORDER TO FIGURE OUT THAT
      // I COULD CHECK FOR DISPOSAL THIS WAY:

      return SerializedProperty.EqualContents(null, prop);
    }

    public static uint GetPropertyHash(this SerializedProperty prop)
    {
      if (IsDisposed(prop))
      {
        Orator.Log("Called GetPropertyHash() on a disposed SerializedProperty!");
        return 0;
      }

      return Hashing.MixHashes(prop.propertyPath.GetHashCode(),
                               prop.serializedObject.targetObject.GetInstanceID());
    }

  } // end static class SerializedProperties

}
