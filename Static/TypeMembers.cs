/*! @file       Static/TypeMembers.cs
 *  @author     levianperez\@gmail.com
 *  @author     levi\@leviperez.dev
 *  @date       2022-06-20
**/

// ReSharper disable MemberCanBePrivate.Global

using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

using JetBrains.Annotations;

using Type = System.Type;


namespace Ore
{
  /// <summary>
  /// Reflection utilities for type (class/struct) members ported from PyroDK.
  /// </summary>
  public static class TypeMembers
  {

    public const BindingFlags        ALL =  BindingFlags.Instance |
                                            BindingFlags.Static   |
                                            BindingFlags.Public   |
                                            BindingFlags.NonPublic;

    public const BindingFlags   ALL_DECL =  ALL                   |
                                            BindingFlags.DeclaredOnly;

    public const BindingFlags   INSTANCE =  BindingFlags.Instance |
                                            BindingFlags.Public   |
                                            BindingFlags.NonPublic;

    public const BindingFlags     STATIC =  BindingFlags.Static   |
                                            BindingFlags.Public   |
                                            BindingFlags.NonPublic;

    public const BindingFlags     PUBLIC =  BindingFlags.Public   |
                                            BindingFlags.Instance |
                                            BindingFlags.Static;

    public const BindingFlags  NONPUBLIC =  BindingFlags.Instance |
                                            BindingFlags.Static   |
                                            BindingFlags.NonPublic;

    public const BindingFlags      ENUMS =  BindingFlags.Static   |
                                            BindingFlags.Public;


    public static bool IsDefined<T>([NotNull] this MemberInfo member, bool inherit = true)
      where T : System.Attribute
    {
      return member.IsDefined(typeof(T), inherit);
    }

    public static bool AreAnyDefined([NotNull] this MemberInfo member, [NotNull] IEnumerable<Type> attributes, bool inherit = true)
    {
      foreach (var attr in attributes)
      {
        if (member.IsDefined(attr, inherit))
          return true;
      }

      return false;
    }

    public static bool IsHidden([NotNull] this FieldInfo field)
    {
      return field.IsDefined<HideInInspector>() ||
             field.IsDefined<System.ObsoleteAttribute>();
    }

    public static bool IsArrayOrList([NotNull] this Type type)
    {
      return type.IsArray || ( type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>) );
    }

    public static bool IsUnityStruct([NotNull] this Type type)
    {
      while (IsArrayOrList(type))
      {
        type = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
        if (type is null)
          return false;
      }

      return type.IsValueType && ( type.Namespace?.StartsWith("Unity") ?? false );
    }


    public static bool TryGetField([CanBeNull] this Type type, [NotNull] string name, out FieldInfo field, BindingFlags blags = INSTANCE)
    {
      field = type?.GetField(name, blags);
      return field != null;
    }

    public static bool TryGetSerializableField([CanBeNull] this Type type, [NotNull] string name, out FieldInfo field)
    {
      return TryGetField(type, name, out field, INSTANCE) &&
             ( field.IsPublic || field.IsDefined<SerializeField>() );
    }

    public static bool TryGetInternalField([CanBeNull] this Type type, [NotNull] string name, out FieldInfo field)
    {
      return TryGetField(type, name, out field, NONPUBLIC);
    }


    public static bool TryGetStaticValue<T>([CanBeNull] this Type type, [NotNull] string name, out T value)
    {
      if (TryGetField(type, name, out var field, STATIC) &&
          typeof(T).IsAssignableFrom(field.FieldType))
      {
        value = (T)field.GetValue(null);
        return true;
      }

      value = default;
      return false;
    }

  } // end static class TypeMembers

}
