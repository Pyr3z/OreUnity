/*! @file       Editor/SerializedProperties.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-15
**/

using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

using FieldInfo = System.Reflection.FieldInfo;


namespace Ore.Editor
{

  public static class SerializedProperties
  {

    public static bool IsDisposed(this SerializedProperty prop)
    {
      // JESUS CHRIST WAS IT SO HARD TO PROVIDE A METHOD LIKE THIS, UNITY ?!

      // I HAD TO SCOUR THE SOURCE CODE IN ORDER TO FIGURE OUT THAT
      // I COULD CHECK FOR DISPOSAL THIS WAY:

      try
      {
        return prop == null || SerializedProperty.EqualContents(null, prop) || !SerializedProperty.EqualContents(prop, prop);
        // (this checks the internal C++ pointer if it's nullptr.)
        // (the 3rd call checks for more recent (Unity 2020) fuckery, and is what throws the NRE.)
      }
      catch (System.NullReferenceException)
      {
        // I hate everyone. Except for you, reader <3
        return true;
      }
    }


    public static bool IsArrayElement(this SerializedProperty prop)
    {
      return prop.propertyPath.EndsWith("]");
    }

    public static bool IsNonReorderable(this SerializedProperty prop)
    {
      #if UNITY_2020_1_OR_NEWER
      return !TryGetFieldInfo(prop, out FieldInfo field) || field.IsDefined<NonReorderableAttribute>();
      #else
      return true;
      #endif
    }

    public static bool IsReadOnly(this SerializedProperty prop)
    {
      bool ok = TryGetFieldInfo(prop, out FieldInfo field);
      OAssert.True(ok, "prop.TryGetFieldInfo");
      return field.IsDefined<ReadOnlyAttribute>();
    }

    public static uint GetPropertyHash(this SerializedProperty prop)
    {
      if (prop.IsDisposed())
      {
        Orator.Log("Called GetPropertyHash() on a disposed SerializedProperty!");
        return 0;
      }

      return Hashing.MixHashes(prop.propertyPath.GetHashCode(),
                               prop.serializedObject.targetObject.GetInstanceID());
    }


    public static bool TryGetFieldInfo(this SerializedProperty prop, out FieldInfo field)
    {
      field = null;

      string[] splits = prop.propertyPath.Split('.');
      var currType = prop.serializedObject.targetObject.GetType();

      for (int i = 0, ilen = splits.Length; i < ilen; ++i)
      {
        if (currType.TryGetSerializableField(splits[i], out field))
        {
          currType = field.FieldType;

          if (i == ilen - 1)
            break;

          if (currType.IsArray)
            currType = currType.GetElementType();
          else if (currType.IsGenericType && currType.GetGenericTypeDefinition() == typeof(List<>))
            currType = currType.GetGenericArguments()[0];
          else // (skip the remaining block if it's just a normal field)
            continue;

          // Ignore "Array.data[idx]" slugs:
          i += 2;
        }
        else
        {
          Orator.Reached(typeof(SerializedProperties));
          return false;
        }
      }

      return field != null;
    }


    public static bool TryGetUnderlyingValue<T>(this SerializedProperty prop,
                                                out T field_value)
    {
      if (TryGetUnderlyingBoxedValue(prop, out object boxed))
      {
        if (boxed is T valid)
        {
          field_value = valid;
          return true;
        }
        else
        {
          Orator.Error($"Cannot cast found field to type {nameof(T)}!");
        }
      }

      field_value = default;
      return false;
    }

    public static bool TryGetUnderlyingBoxedValue(SerializedProperty prop,
                                                  out object boxed_value)
    {
      boxed_value = prop.serializedObject.targetObject;
      return TryGetUnderlyingBoxedValue(prop.propertyPath, ref boxed_value);
    }


    // private

    static bool TryGetUnderlyingBoxedValue(string prop_path, ref object boxed_value)
    {
      if (boxed_value is null)
        return false;

      // Strategy: use Reflection to follow the fully-qualified property path
      //           and obtain an exact reference to the underlying object.

      string[] path_splits = prop_path.Split('.');

      for (int i = 0; i < path_splits.Length; ++i)
      {
        // Look at the next segment of the property's path:
        var field_name = path_splits[i];

        // Special handling to support Array/List nested types:
        if (field_name == "Array" && boxed_value is IList boxed_list)
        {
          // increment past the injected array sub-properties:
          // parse for an integer index:
          if (Parsing.TryParseNextIndex(path_splits[++i], out int idx) && idx < boxed_list.Count)
          {
            boxed_value = boxed_list[idx];
            continue;
          }
          else
          {
            //Logging.ShouldNotReach(blame: prop_path);
            return false;
          }
        }

        // Find the FieldInfo of the target we are currently looking at:
        if (!boxed_value.GetType().TryGetSerializableField(field_name, out var field))
          // Early-out opportunity:
          return false;

        // Step finished.
        boxed_value = field.GetValue(boxed_value);

      } // end for-loop

      // We've walked to the end of the property path.
      // Returns true if success:
      return boxed_value != null;
    }

  } // end static class SerializedProperties

}
