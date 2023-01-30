// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// Bound type
  /// <item>Lower: null corresponds to -oo</item>
  /// <item>Upper: null corresponds to +oo</item>
  /// </summary>
  public enum BoundType {
    /// <summary>
    /// Lower bound
    /// 
    /// null corresponds to -oo
    /// </summary>
    Lower,
    /// <summary>
    /// Upper bound
    /// 
    /// null corresponds to +oo
    /// </summary>
    Upper
  }
  
  /// <summary>
  /// Range bound
  /// </summary>
  [Serializable]
  public struct Bound<T>: IBound<T>, IComparable<Bound<T>>, IComparable, IEquatable<Bound<T>>
    where T: struct, IComparable<T>, IComparable, IEquatable<T>
  {
    #region Members
    [NonSerialized]
    readonly T? m_boundValue;
    readonly BoundType m_boundType;
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
    /// Bound type
    /// <item>Lower: null corresponds to -oo</item>
    /// <item>Upper: null corresponds to +oo</item>
    /// </summary>
    [XmlAttribute("BoundType")]
    public BoundType BoundType {
      get { return m_boundType; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="v"></param>
    /// <param name="bound"></param>
    public Bound (T? v, BoundType bound)
    {
      this.m_boundValue = v;
      this.m_boundType = bound;
    }
    
    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="v"></param>
    public Bound (Bound<T> v)
    {
      this.m_boundValue = v.NullableValue;
      this.m_boundType = v.BoundType;
    }
    #endregion // Constructors

    #region IEquatable implementation
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public bool Equals(T other)
    {
      return this.HasValue && System.Collections.Generic.EqualityComparer<T>.Default.Equals (this.Value, other);
    }

    bool EqualsBound(Bound<T> other)
    {
      if (!HasValue) {
        return !other.HasValue && (this.BoundType == other.BoundType);
      }
      else {
        return other.HasValue && System.Collections.Generic.EqualityComparer<T>.Default.Equals (this.Value, other.Value);
      }
    }
    
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public bool Equals(Bound<T> other)
    {
      return EqualsBound (other);
    }
    
    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals(object obj)
    {
      if (obj is Bound<T>) {
        Bound<T> other = (Bound<T>)obj;
        return this.EqualsBound (other);
      }
      else if (obj is LowerBound<T>) {
        LowerBound<T> other = (LowerBound<T>) obj;
        if (!HasValue) {
          return !other.HasValue && (other.BoundType == this.BoundType);
        }
        else {
          return other.HasValue && System.Collections.Generic.EqualityComparer<T>.Default.Equals (this.Value, other.Value);
        }
      }
      else if (obj is UpperBound<T>) {
        UpperBound<T> other = (UpperBound<T>) obj;
        if (!HasValue) {
          return !other.HasValue && (other.BoundType == this.BoundType);
        }
        else {
          return other.HasValue && System.Collections.Generic.EqualityComparer<T>.Default.Equals (this.Value, other.Value);
        }
      }
      else if (obj is T) {
        T other = (T) obj;
        return HasValue && System.Collections.Generic.EqualityComparer<T>.Default.Equals (this.Value, other);
      }
      else {
        if (null == obj) {
          return !HasValue;
        }
        else {
          return HasValue && object.Equals (m_boundValue, obj);
        }
      }
    }
    
    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode()
    {
      if (!this.HasValue) {
        switch (this.BoundType) {
          case BoundType.Lower:
            return 1;
          case BoundType.Upper:
            return 2;
          default:
            throw new InvalidOperationException ();
        }
      }
      else { // HasValue
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * this.Value.GetHashCode();
          hashCode *= 4;
        }
        return hashCode;
      }
    }
    #endregion // IEquatable implementation
    
    #region Methods
    /// <summary>
    /// To string
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      if (HasValue) {
        return Value.ToString ();
      }
      else {
        switch (m_boundType) {
          case BoundType.Lower:
            return "-oo";
          case BoundType.Upper:
            return "+oo";
          default:
            throw new InvalidOperationException ();
        }
      }
    }

    /// <summary>
    /// IComparable.CompareTo implementation.
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    public int CompareTo(object obj)
    {
      if (obj is Bound<T>) {
        Bound<T> other = (Bound<T>) obj;
        return Bound.Compare (this, other);
      }
      else if (obj is T) {
        Bound<T> other = new Bound<T> ((T) obj, BoundType.Upper);
        return Bound.Compare (this, other);
      }
      
      throw new ArgumentException ("object is not a Bound<T>, a T? or a T");
    }
    
    /// <summary>
    /// IComparable.CompareTo implementation.
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    public int CompareTo(Bound<T> other)
    {
      return Bound.Compare (this, other);
    }

    /// <summary>
    /// Conversion operator
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static explicit operator T? (Bound<T> v)
    {
      return v.NullableValue;
    }
    
    /// <summary>
    /// Conversion operator
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static explicit operator T (Bound<T> v)
    {
      return v.Value;
    }
    
    /// <summary>
    /// Conversion operator
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static implicit operator Bound<T> (T v)
    {
      return new Bound<T> (v, BoundType.Upper); // The bound type is not important here
    }

    /// <summary>
    /// Comparison operator
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static bool operator< (Bound<T> v1, Bound<T> v2)
    {
      return Bound.Compare (v1, v2) < 0;
    }
    
    /// <summary>
    /// Comparison operator
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static bool operator<= (Bound<T> v1, Bound<T> v2)
    {
      return Bound.Compare (v1, v2) <= 0;
    }
    
    /// <summary>
    /// Comparison operator
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static bool operator> (Bound<T> v1, Bound<T> v2)
    {
      return Bound.Compare (v1, v2) > 0;
    }
    
    /// <summary>
    /// Comparison operator
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static bool operator>= (Bound<T> v1, Bound<T> v2)
    {
      return Bound.Compare (v1, v2) >= 0;
    }

    /// <summary>
    /// Comparison operator
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static bool operator< (Bound<T> v1, T v2)
    {
      return Bound.Compare (v1, Bound.CreateUpperBound<T> (v2)) < 0;
    }

    /// <summary>
    /// Comparison operator
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static bool operator<= (Bound<T> v1, T v2)
    {
      return Bound.Compare (v1, Bound.CreateUpperBound<T> (v2)) <= 0;
    }

    /// <summary>
    /// Comparison operator
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static bool operator> (Bound<T> v1, T v2)
    {
      return Bound.Compare (v1, Bound.CreateUpperBound<T> (v2)) > 0;
    }

    /// <summary>
    /// Comparison operator
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static bool operator>= (Bound<T> v1, T v2)
    {
      return Bound.Compare (v1, Bound.CreateUpperBound<T> (v2)) >= 0;
    }

    /// <summary>
    /// Comparison operator
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static bool operator== (Bound<T> v1, T v2)
    {
      return v1.HasValue && object.Equals (v1.Value, v2);
    }

    /// <summary>
    /// Comparison operator
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static bool operator!= (Bound<T> v1, T v2)
    {
      return !(v1 == v2);
    }
  }

  
  /// <summary>
  /// static class for the static methods of Bound&lt;T&gt;
  /// </summary>
  public static class Bound
  {
    /// <summary>
    /// Create a new lower bound
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Bound<T> CreateLowerBound<T> (T? v)
      where T: struct, IComparable<T>, IComparable, IEquatable<T>
    {
      return new Bound<T> (v, BoundType.Lower);
    }

    /// <summary>
    /// Create a new upper bound
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Bound<T> CreateUpperBound<T> (T? v)
      where T: struct, IComparable<T>, IComparable, IEquatable<T>
    {
      return new Bound<T> (v, BoundType.Upper);
    }

    /// <summary>
    /// Check the equality
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool Equals<T> (Bound<T> a, Bound<T> b)
      where T: struct, IComparable<T>, IComparable, IEquatable<T>
    {
      if (a.HasValue) {
        return b.HasValue && System.Collections.Generic.EqualityComparer<T>.Default.Equals (a.Value, b.Value);
      }
      else { // !HasValue
        return !b.HasValue && (a.BoundType == b.BoundType);
      }
    }

    /// <summary>
    /// Check the equality
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool Equals<T> (Bound<T> a, T b)
      where T: struct, IComparable<T>, IComparable, IEquatable<T>
    {
      return a.HasValue && System.Collections.Generic.EqualityComparer<T>.Default.Equals (a.Value, b);
    }

    /// <summary>
    /// Compare two nullable date/times considering their bound
    /// 
    /// If one of the two date/times is local while the other one is universal,
    /// do the necessary conversions
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns></returns>
    public static int Compare<T> (Bound<T> t1, Bound<T> t2)
      where T: struct, IComparable<T>, IComparable, IEquatable<T>
    {
      if (t1.HasValue) {
        if (t2.HasValue) {
          return Comparer<T>.Default.Compare(t1.Value, t2.Value);
        }
        else { // (!t2.HasValue) => check t2.Bound
          if (BoundType.Upper == t2.BoundType) { // t2=+oo, t1 < t2 => -1
            return -1;
          }
          else { // t2=-oo, t2 < t1 => +1
            return +1;
          }
        }
      }
      else {
        if (t2.HasValue) { // check t1.Bound
          if (BoundType.Upper == t1.BoundType) { // t1=+oo, t2 < t1 => +1
            return +1;
          }
          else { // t1=-oo, t1 < t2 => -1
            return -1;
          }
        }
        else { // t1 = t2 = null
          if (t1.BoundType == t2.BoundType) {
            return 0;
          }
          else { // different bounds
            switch (t1.BoundType) {
              case BoundType.Lower: // t1 < t2 => -1
                Debug.Assert (BoundType.Upper == t2.BoundType);
                return -1;
              case BoundType.Upper: // t2 < t1 => +1
                Debug.Assert (BoundType.Lower == t2.BoundType);
                return +1;
              default:
                throw new InvalidOperationException ();
            }
          }
        }
      }
    }

    /// <summary>
    /// Get the maximum value of two nullable date/times
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns></returns>
    public static Bound<T> GetMaximum<T> (Bound<T> t1, Bound<T> t2)
      where T: struct, IComparable<T>, IComparable, IEquatable<T>
    {
      if (Compare<T> (t1, t2) <= 0) {
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
    public static Bound<T> GetMinimum<T> (Bound<T> t1, Bound<T> t2)
      where T: struct, IComparable<T>, IComparable, IEquatable<T>
    {
      if (Compare<T> (t1, t2) <= 0) {
        return t1;
      }
      else {
        return t2;
      }
    }
    
    /// <summary>
    ///
    /// </summary>
    /// <param name="nullableType"></param>
    /// <returns></returns>
    public static Type GetUnderlyingType(Type nullableType)
    {
      if((object)nullableType == null) {
        throw new ArgumentNullException("nullableType");
      }
      Type result = null;
      if( nullableType.IsGenericType && !nullableType.IsGenericTypeDefinition) {
        // instantiated generic type only
        Type genericType = nullableType.GetGenericTypeDefinition();
        if( Object.ReferenceEquals(genericType, typeof(Bound<>))) {
          result = nullableType.GetGenericArguments()[0];
        }
      }
      return result;
    }
    #endregion // Methods
  }
}
