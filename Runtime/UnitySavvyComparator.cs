/*! @file       Runtime/UnitySavvyComparator.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-12-07
**/

using Object = UnityEngine.Object;


namespace Ore
{

  public class UnitySavvyComparator<T> : Comparator<object>, IComparator<T>
    where T : Object
  {

    public new static readonly UnitySavvyComparator<T> Default = new UnitySavvyComparator<T>();


    public override bool IsNone(in object obj)
    {
      return obj is Object uobj ? !uobj : base.IsNone(in obj);
    }


    private readonly Comparator<T> m_ObjComparator = Comparator<T>.Default;


    public bool Equals(T a, T b)
    {
      return m_ObjComparator.Equals(a, b);
    }

    public int GetHashCode(T obj)
    {
      return obj.GetInstanceID();
    }

    public int Compare(T a, T b)
    {
      return m_ObjComparator.Compare(a, b);
    }

    public System.TypeCode GetTypeCode(in T obj)
    {
      return System.TypeCode.Object;
    }

    public bool IsNone(in T obj)
    {
      return !obj;
    }

    public bool Equals(in T a, in T b)
    {
      return m_ObjComparator.Equals(a, b);
    }

    public int GetHashCode(in T obj)
    {
      return obj is null ? 0 : obj.GetInstanceID();
    }

    public int Compare(in T a, in T b)
    {
      return m_ObjComparator.Compare(a, b);
    }

  } // end sealed class UnitySavvyComparator<T>


  public sealed class UnitySavvyComparator : UnitySavvyComparator<Object>
  {

    public new static readonly UnitySavvyComparator Default = new UnitySavvyComparator();

  }

}