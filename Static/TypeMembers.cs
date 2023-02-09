/*! @file       Static/TypeMembers.cs
 *  @author     levianperez\@gmail.com
 *  @author     levi\@leviperez.dev
 *  @date       2022-06-20
**/

// ReSharper disable MemberCanBePrivate.Global

using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

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


    public static bool IsDefined<T>(this MemberInfo member, bool inherit = true)
      where T : System.Attribute
    {
      OAssert.NotNull(member, "member");

      return member.IsDefined(typeof(T), inherit);
    }

    public static bool AreAnyDefined(this MemberInfo member, IEnumerable<Type> attributes, bool inherit = true)
    {
      OAssert.AllNotNull(member, attributes);

      foreach (var attr in attributes)
      {
        if (member.IsDefined(attr, inherit))
          return true;
      }

      return false;
    }

    public static bool IsHidden(this FieldInfo field)
    {
      OAssert.NotNull(field, "field");

      return field.IsDefined<HideInInspector>() ||
             field.IsDefined<System.ObsoleteAttribute>();
    }


    public static bool TryGetField(this Type type, string name, out FieldInfo field, BindingFlags blags = INSTANCE)
    {
      field = type?.GetField(name, blags);
      return field != null;
    }

    public static bool TryGetSerializableField(this Type type, string name, out FieldInfo field)
    {
      return type.TryGetField(name, out field, INSTANCE) &&
             (field.IsPublic || field.IsDefined<SerializeField>());
    }

    public static bool TryGetInternalField(this Type type, string name, out FieldInfo field)
    {
      return type.TryGetField(name, out field, NONPUBLIC);
    }


    public static bool TryGetStaticValue<T>(this Type type, string name, out T value)
    {
      if (type.TryGetField(name, out var field, STATIC) &&
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
