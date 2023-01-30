// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml.Serialization;
using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// Lower bound: null represents -oo
  /// </summary>
  [Serializable]
  public struct LowerBound<T>: IBound<T>, IComparable<Bound<T>>, IComparable, IEquatable<Bound<T>>, IComparable<LowerBound<T>>, IEquatable<LowerBound<T>>
    where T: struct, IComparable<T>, IComparable, IEquatable<T>
  {
    #region Members
    [NonSerialized]
    readonly T? m_boundValue;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Internal nullable value
    /// </summary>
    [XmlText]
    public T? NullableValue {
      get { return m_boundValue; }
    }
    
    /// <summary>
    /// Is the value null ? Does it correspond to infinity ?
    /// </summary>
    [XmlIgnore]
    public bool HasValue {
      get { return m_boundValue.HasValue; }
    }
    
    /// <summary>
    /// Return the date/time if it is not null
    /// 
    /// Else an exception is raised
    /// </summary>
    [XmlIgnore]
    public T Value {
      get { return m_boundValue.Value; }
    }
    
    /// <summary>
    /// Bound type: always Lower
    /// </summary>
    [XmlIgnore]
    public BoundType BoundType {
      get { return BoundType.Lower; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="v"></param>
    public LowerBound (T v)
    {
      this.m_boundValue = v;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="v"></param>
    public LowerBound (T? v)
    {
      this.m_boundValue = v;
    }
    #endregion // Constructors

    /// <summary>
    /// To string
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return m_boundValue.ToString ();
    }

    /// <summary>
    /// Conversion operator
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static explicit operator T? (LowerBound<T> v)
    {
      return v.NullableValue;
    }
    
    /// <summary>
    /// Conversion operator
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static explicit operator T (LowerBound<T> v)
    {
      return v.Value;
    }
    
    /// <summary>
    /// Conversion operator
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static implicit operator LowerBound<T> (T v)
    {
      return new LowerBound<T> (v);
    }
    
    /// <summary>
    /// Conversion operator
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static explicit operator LowerBound<T> (T? v)
    {
      return new LowerBound<T> (v);
    }
    
    /// <summary>
    /// Convert a LowerBound into a Bound
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static implicit operator Bound<T> (LowerBound<T> v)
    {
      return Bound.CreateLowerBound (v.NullableValue);
    }
    
    /// <summary>
    /// Conversion operator
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static explicit operator LowerBound<T> (Bound<T> v)
    {
      if (!v.HasValue) {
        if (BoundType.Lower == v.BoundType) {
          return new LowerBound<T> (null);
        }
        else {
          throw new ArgumentOutOfRangeException ();
        }
      }
      
      return new LowerBound<T> (v.Value);
    }

    /// <summary>
    /// Conversion operator
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static explicit operator LowerBound<T> (UpperBound<T> v)
    {
      if (!v.HasValue) {
        throw new ArgumentOutOfRangeException ();
      }
      
      return new LowerBound<T> (v.Value);
    }

    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public bool Equals(T other)
    {
      return HasValue && object.Equals (this.Value, other);
    }

    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public bool Equals(Bound<T> other)
    {
      return other.Equals ((Bound<T>)this);
    }

    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals(object obj)
    {
      return ((Bound<T>)this).Equals (obj);
    }
    
    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode()
    {
      return ((Bound<T>)this).GetHashCode ();
    }

    /// <summary>
    /// IComparable.CompareTo implementation.
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    public int CompareTo(object obj)
    {
      if (obj is Bound<T>) {
        Bound<T> other = (Bound<T>) obj;
        return Bound.Compare<T> (this, other);
      }
      else if (obj is T) {
        Bound<T> other = new Bound<T> ((T) obj, BoundType.Upper);
        return Bound.Compare<T> (this, other);
      }
      
      throw new ArgumentException ("object is not a Bound<T>, a T? or a T");
    }
    
    /// <summary>
    /// IComparable.CompareTo implementation.
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    public int CompareTo(Bound<T> other)
    {
      return Bound.Compare<T> (this, other);
    }

    /// <summary>
    /// IComparable.CompareTo implementation.
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    public int CompareTo (LowerBound<T> other)
    {
      return LowerBound.Compare<T> (this, other);
    }

    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public bool Equals (LowerBound<T> other)
    {
      return LowerBound.Equals<T> (this, other);
    }

    /// <summary>
    /// Comparison operator
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static bool operator< (LowerBound<T> v1, Bound<T> v2)
    {
      return Bound.Compare<T> (v1, v2) < 0;
    }
    
    /// <summary>
    /// Comparison operator
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static bool operator<= (LowerBound<T> v1, Bound<T> v2)
    {
      return Bound.Compare<T> (v1, v2) <= 0;
    }
    
    /// <summary>
    /// Comparison operator
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static bool operator> (LowerBound<T> v1, Bound<T> v2)
    {
      return Bound.Compare<T> (v1, v2) > 0;
    }
    
    /// <summary>
    /// Comparison operator
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static bool operator>= (LowerBound<T> v1, Bound<T> v2)
    {
      return Bound.Compare<T> (v1, v2) >= 0;
    }

    /// <summary>
    /// Comparison operator
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static bool operator== (LowerBound<T> v1, Bound<T> v2)
    {
      return Bound.Equals<T> ((Bound<T>)v1, v2);
    }

    /// <summary>
    /// Comparison operator
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static bool operator!= (LowerBound<T> v1, Bound<T> v2)
    {
      return !(v1 == v2);
    }
  }
  
  /// <summary>
  /// static class for the static methods
  /// </summary>
  public static class LowerBound
  {
    /// <summary>
    /// Compare two nullable date/times considering their bound
    /// 
    /// If one of the two date/times is local while the other one is universal,
    /// do the necessary conversions
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns></returns>
    public static int Compare<T> (LowerBound<T> t1, LowerBound<T> t2)
      where T : struct, IComparable<T>, IComparable, IEquatable<T>
    {
      return Bound.Compare<T> (t1, t2);
    }

    /// <summary>
    /// Check the equality
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool Equals<T> (LowerBound<T> a, LowerBound<T> b)
      where T : struct, IComparable<T>, IComparable, IEquatable<T>
    {
      return Bound.Equals<T> (a, b);
    }

    /// <summary>
    /// Check the equality
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool Equals<T> (LowerBound<T> a, T b)
      where T : struct, IComparable<T>, IComparable, IEquatable<T>
    {
      return Bound.Equals<T> (a, b);
    }

    /// <summary>
    /// Get the maximum value of two nullable date/times
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns></returns>
    public static LowerBound<T> GetMaximum<T> (LowerBound<T> t1, LowerBound<T> t2)
      where T: struct, IComparable<T>, IComparable, IEquatable<T>
    {
      if (Bound.Compare<T> (t1, t2) <= 0) {
        return t2;
      }
      else {
        return t1;
      }
    }

    /// <summary>
    /// Get the minimum value of two nullable date/times
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns></returns>
    public static LowerBound<T> GetMinimum<T> (LowerBound<T> t1, LowerBound<T> t2)
      where T: struct, IComparable<T>, IComparable, IEquatable<T>
    {
      if (Bound.Compare<T> (t1, t2) <= 0) {
        return t1;
      }
      else {
        return t2;
      }
    }
  }
}
