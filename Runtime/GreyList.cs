/*! @file       Runtime/GreyList.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-08-18
**/

// TODO another candidate for the "Collections" namespace/package

// ReSharper disable HeapView.BoxingAllocation

using JetBrains.Annotations;
using UnityEngine;


namespace Ore
{
  [System.Serializable]
  public class StringGreyList : GreyList<string> { }
  [System.Serializable]
  public class ObjectGreyList : GreyList<Object> { }

  public enum GreyListType
  {
    Blacklist,
    Whitelist,
    Disabled
  }
  
  [System.Serializable]
  public class GreyList<T> : SerialSet<T>
  {
    public GreyListType Type
    {
      get => m_Type;
      set => m_Type = value;
    }

    [SerializeField]
    private GreyListType m_Type = GreyListType.Blacklist;
    
    
    public bool Accepts([CanBeNull] T item)
    {
      return m_Type == GreyListType.Disabled || ((m_Type == GreyListType.Whitelist) == Contains(item));
    }
    
  } // end class GreyList<T>
}