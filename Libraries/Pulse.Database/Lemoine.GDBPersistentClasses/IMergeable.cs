// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Reflection;

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// IMergeable interface for all
  /// the persistent classes that can be merged with another one
  /// </summary>
  public interface IMergeable<T>
  {
    /// <summary>
    /// Merge an external object into the current object
    /// </summary>
    /// <param name="other"></param>
    /// <param name="conflictResolution"></param>
    void Merge (T other,
                ConflictResolution conflictResolution);
  }
  
  /// <summary>
  /// Attribute to flag that the property should be automatically
  /// merged according to its type
  /// 
  /// The following basic types are supported:
  /// <item>int</item>
  /// <item>double</item>
  /// <item>string</item>
  /// <item>Nullable&lt;T&gt;</item>
  /// <item>IReferenceData interface</item>
  /// </summary>
  public class MergeAutoAttribute: Attribute
  {
  }
  
  /// <summary>
  /// Attribute to flag that this parent property should be
  /// automatically merged
  /// </summary>
  public class MergeParentAttribute: Attribute
  {
  }
  
  /// <summary>
  /// Attribute to flag this this children property should be
  /// automatically merged
  /// </summary>
  public class MergeChildrenAttribute: Attribute
  {
    string m_childProperty;

    /// <summary>
    /// Get the child property that is associated to the current object
    /// </summary>
    public string ChildProperty {
      get { return m_childProperty; }
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="childProperty">Child property that is associated to the current object</param>
    public MergeChildrenAttribute(string childProperty)
    {
      this.m_childProperty = childProperty;
    }
  }

  /// <summary>
  /// Attribute to flag that an object property should be automatically
  /// merged.
  /// 
  /// An object property is considered undefined if it is null
  /// </summary>
  public class MergeObjectAttribute: Attribute
  {
  }
  
  /// <summary>
  /// Attribute to flag that an DateTime property should be automatically
  /// merged keeping the oldest available DateTime.
  /// </summary>
  public class MergeDateTimeWithOldestAttribute: Attribute
  {
  }
  
  /// <summary>
  /// Attribute to flag that an DateTime property should be automatically
  /// merged keeping the newest available DateTime.
  /// </summary>
  public class MergeDateTimeWithNewestAttribute: Attribute
  {
  }
  
  /// <summary>
  /// Utility methods to merge two persistent classes
  /// </summary>
  internal static class Mergeable
  {
    static readonly ILog log = LogManager.GetLogger(typeof (Mergeable).FullName);

    /// <summary>
    /// Try to merge two properties that are never considered as undefined
    /// </summary>
    /// <param name="final">Final object (not null)</param>
    /// <param name="other">Other object (not null)</param>
    /// <param name="property">property name</param>
    /// <param name="conflictResolution">how to manage the conflicts</param>
    /// <exception cref="ConflictException">Two properties can't be merged and conflictResolution is Exception</exception>
    internal static void MergeDefined (object final, object other,
                                       string property,
                                       ConflictResolution conflictResolution)
    {
      Debug.Assert (null != final);
      Debug.Assert (null != other);
      
      Type type = final.GetType ();
      Debug.Assert (other.GetType ().Equals (type));
      PropertyInfo propertyInfo = type.GetProperty (property, typeof(string));
      MergeDefined (final, other, propertyInfo, conflictResolution);
    }
    
    /// <summary>
    /// Try to merge two properties that are never considered as undefined
    /// </summary>
    /// <param name="final"></param>
    /// <param name="other"></param>
    /// <param name="property"></param>
    /// <param name="conflictResolution">how to manage the conflicts</param>
    /// <exception cref="ConflictException">Two properties can't be merged and conflictResolution is Exception</exception>
    private static void MergeDefined (object final, object other,
                                      PropertyInfo property,
                                      ConflictResolution conflictResolution)
    {
      object otherItem = property.GetValue (other, null);
      object finalItem = property.GetValue (final, null);
      
      if (!object.Equals (finalItem, otherItem)) {
        switch (conflictResolution) {
          case ConflictResolution.Keep:
            log.Warn ($"MergeDefined: {property.Name} conflict final={finalItem} other={otherItem} => keep the final data");
            return;
          case ConflictResolution.Overwrite:
            log.Warn ($"MergeDefined: {property.Name} conflict final={finalItem} other={otherItem} => overwrite the final data with the other data");
            property.SetValue (final, otherItem, null);
            return;
          case ConflictResolution.Exception:
            log.Error ($"MergeDefined: {property.Name} conflict final={finalItem} other={otherItem} => raise ConflictException");
            throw new ConflictException (other, final, property.Name);
        }
      } // else keep the current value
    }
    
    /// <summary>
    /// Try to merge two string properties
    /// 
    /// A string is considered undefined if null or empty
    /// </summary>
    /// <param name="final">Final object (not null)</param>
    /// <param name="other">Other object (not null)</param>
    /// <param name="property">property name</param>
    /// <param name="conflictResolution">how to manage the conflicts</param>
    /// <exception cref="ConflictException">Two properties can't be merged and conflictResolution is Exception</exception>
    internal static void MergeString (object final, object other,
                                      string property,
                                      ConflictResolution conflictResolution)
    {
      Debug.Assert (null != final);
      Debug.Assert (null != other);
      
      Type type = final.GetType ();
      Debug.Assert (other.GetType ().Equals (type));
      PropertyInfo propertyInfo = type.GetProperty (property, typeof(string));
      MergeString (final, other, propertyInfo, conflictResolution);
    }
    
    /// <summary>
    /// Try to merge two string properties
    /// 
    /// A string is considered undefined if null or empty
    /// </summary>
    /// <param name="final"></param>
    /// <param name="other"></param>
    /// <param name="property"></param>
    /// <param name="conflictResolution">how to manage the conflicts</param>
    /// <exception cref="ConflictException">Two properties can't be merged and conflictResolution is Exception</exception>
    private static void MergeString (object final, object other,
                                     PropertyInfo property,
                                     ConflictResolution conflictResolution)
    {
      string otherString = property.GetValue (other, null) as string;
      string finalString = property.GetValue (final, null) as string;
      
      if ( (null != otherString) && (0 < otherString.Length)) {
        if ( (null != finalString) && (0 < finalString.Length)) {
          if (!object.Equals (finalString, otherString)) {
            switch (conflictResolution) {
              case ConflictResolution.Keep:
                log.Warn ($"MergeString: {property.Name} conflict final={finalString} other={otherString} => keep the final data");
                return;
              case ConflictResolution.Overwrite:
                log.Warn ($"MergeString: {property.Name} conflict final={finalString} other={otherString} => overwrite the final data with the other data");
                property.SetValue (final, otherString, null);
                return;
              case ConflictResolution.Exception:
                log.Error ($"MergeString: {property.Name} conflict final={finalString} other={otherString} => raise ConflictException");
                throw new ConflictException (other, final, property.Name);
            }
          } // else keep the current value
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"MergeString: get the name {otherString} from other");
          }
          property.SetValue (final, otherString, null);
        }
      }
    }
    
    /// <summary>
    /// Try to merge two Nullable&lt;T&gt; properties
    /// 
    /// A Nullable&lt;T&gt; is considered undefined if HasValue returns false
    /// </summary>
    /// <param name="final">Final object (not null)</param>
    /// <param name="other">Other object (not null)</param>
    /// <param name="property">property name</param>
    /// <param name="conflictResolution">how to manage the conflicts</param>
    /// <exception cref="ConflictException">Two properties can't be merged and conflictResolution is Exception</exception>
    internal static void MergeNullable<T> (object final, object other,
                                           string property,
                                           ConflictResolution conflictResolution)
      where T: struct
    {
      Debug.Assert (null != final);
      Debug.Assert (null != other);
      
      Type type = final.GetType ();
      Debug.Assert (other.GetType ().Equals (type));
      PropertyInfo propertyInfo = type.GetProperty (property, typeof(double?));
      MergeNullable<T> (final, other, propertyInfo, conflictResolution);
    }
    
    /// <summary>
    /// Try to merge two Nullable&lt;T&gt; properties
    /// 
    /// A Nullable&lt;T&gt; is considered undefined if HasValue returns false
    /// </summary>
    /// <param name="final"></param>
    /// <param name="other"></param>
    /// <param name="property"></param>
    /// <param name="conflictResolution">how to manage the conflicts</param>
    /// <exception cref="ConflictException">Two properties can't be merged and conflictResolution is Exception</exception>
    private static void MergeNullable<T> (object final, object other,
                                          PropertyInfo property,
                                          ConflictResolution conflictResolution)
      where T: struct
    {
      Nullable<T> otherValue = property.GetValue (other, null) as Nullable<T>;
      Nullable<T> finalValue = property.GetValue (final, null) as Nullable<T>;
      
      if (otherValue.HasValue) {
        if (finalValue.HasValue) {
          if (!object.Equals (finalValue, otherValue)) {
            switch (conflictResolution) {
              case ConflictResolution.Keep:
                log.Warn ($"MergeNullable: {property.Name} conflict final={finalValue} other={otherValue} => keep the final data");
                return;
              case ConflictResolution.Overwrite:
                log.Warn ($"MergeNullable: {property.Name} conflict final={finalValue} other={otherValue} => overwrite the final data with the other data");
                property.SetValue (final, otherValue, null);
                return;
              case ConflictResolution.Exception:
                log.Error ($"MergeNullable: {property.Name} conflict final={finalValue} other={otherValue} => raise ConflictException");
                throw new ConflictException (other, final, property.Name);
            }
          } // else keep the current value
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"MergeNullable: get the name {otherValue} from other");
          }
          property.SetValue (final, otherValue, null);
        }
      }
    }
    
    /// <summary>
    /// Try to merge two object properties
    /// 
    /// An object is considered undefined if null
    /// </summary>
    /// <param name="final">Final object (not null)</param>
    /// <param name="other">Other object (not null)</param>
    /// <param name="property">property name</param>
    /// <param name="conflictResolution">how to manage the conflicts</param>
    /// <exception cref="ConflictException">Two properties can't be merged and conflictResolution is Exception</exception>
    internal static void MergeObject (object final, object other,
                                      string property,
                                      ConflictResolution conflictResolution)
    {
      Debug.Assert (null != final);
      Debug.Assert (null != other);
      
      Type type = final.GetType ();
      Debug.Assert (other.GetType ().Equals (type));
      PropertyInfo propertyInfo = type.GetProperty (property, typeof(double?));
      MergeObject (final, other, propertyInfo, conflictResolution);
    }
    
    /// <summary>
    /// Try to merge two object properties
    /// 
    /// An object is considered undefined if null
    /// </summary>
    /// <param name="final"></param>
    /// <param name="other"></param>
    /// <param name="property"></param>
    /// <param name="conflictResolution">how to manage the conflicts</param>
    /// <exception cref="ConflictException">Two properties can't be merged and conflictResolution is Exception</exception>
    private static void MergeObject (object final, object other,
                                     PropertyInfo property,
                                     ConflictResolution conflictResolution)
    {
      object otherValue = property.GetValue (other, null);
      object finalValue = property.GetValue (final, null);
      
      if (null != otherValue) {
        if (null != finalValue) {
          if (!object.Equals (finalValue, otherValue)) {
            switch (conflictResolution) {
              case ConflictResolution.Keep:
                log.Warn ($"MergeObject: {property.Name} conflict final={finalValue} other={otherValue} => keep the final data");
                return;
              case ConflictResolution.Overwrite:
                log.Warn ($"MergeObject: {property.Name} conflict final={finalValue} other={otherValue} => overwrite the final data with the other data");
                property.SetValue (final, otherValue, null);
                return;
              case ConflictResolution.Exception:
                log.Error ($"MergeObject: {property.Name} conflict final={finalValue} other={otherValue} => raise ConflictException");
                throw new ConflictException (other, final, property.Name);
            }
          } // else keep the current value
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"MergeObject: get the name {otherValue} from other");
          }
          property.SetValue (final, otherValue, null);
        }
      }
    }
    
    /// <summary>
    /// Try to merge two reference data properties
    /// 
    /// A reference data is considered undefined if null or Id==1
    /// </summary>
    /// <param name="final">Final object (not null)</param>
    /// <param name="other">Other object (not null)</param>
    /// <param name="property">property name</param>
    /// <param name="conflictResolution">how to manage the conflicts</param>
    /// <exception cref="ConflictException">Two properties can't be merged and conflictResolution is Exception</exception>
    internal static void MergeReferenceData (object final, object other,
                                             string property,
                                             ConflictResolution conflictResolution)
    {
      Debug.Assert (null != final);
      Debug.Assert (null != other);
      
      Type type = final.GetType ();
      Debug.Assert (other.GetType ().Equals (type));
      PropertyInfo propertyInfo = type.GetProperty (property, typeof (IReferenceData));
      MergeReferenceData (final, other, propertyInfo, conflictResolution);
    }
    
    /// <summary>
    /// Try to merge two reference data properties
    /// 
    /// A reference data is considered undefined if null or Id==1
    /// </summary>
    /// <param name="final"></param>
    /// <param name="other"></param>
    /// <param name="property"></param>
    /// <param name="conflictResolution">how to manage the conflicts</param>
    /// <exception cref="ConflictException">Two properties can't be merged and conflictResolution is Exception</exception>
    private static void MergeReferenceData (object final, object other,
                                            PropertyInfo property,
                                            ConflictResolution conflictResolution)
    {
      IReferenceData otherValue = property.GetValue (other, null) as IReferenceData;
      IReferenceData finalValue = property.GetValue (final, null) as IReferenceData;
      
      if ((null != otherValue) && !otherValue.IsUndefined ()) {
        if ((null != finalValue) && !finalValue.IsUndefined ()) {
          if (!object.Equals (finalValue, otherValue)) {
            switch (conflictResolution) {
              case ConflictResolution.Keep:
                log.Warn ($"MergeReferenceData: {property.Name} conflict final={finalValue} other={otherValue} => keep the final data");
                return;
              case ConflictResolution.Overwrite:
                log.Warn ($"MergeReferenceData: {property.Name} conflict final={finalValue} other={otherValue} => overwrite the final data with the other data");
                property.SetValue (final, otherValue, null);
                return;
              case ConflictResolution.Exception:
                log.Error ($"MergeReferenceData: {property.Name} conflict final={finalValue} other={otherValue}=> raise ConflictException");
                throw new ConflictException (other, final, property.Name);
            }
          } // else keep the current value
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"MergeReferenceData: get the name {otherValue} from other");
          }
          property.SetValue (final, otherValue, null);
        }
      }
    }
    
    /// <summary>
    /// Try to merge two children collections
    /// </summary>
    /// <param name="final"></param>
    /// <param name="other"></param>
    /// <param name="property"></param>
    /// <param name="childProperty"></param>
    /// <param name="conflictResolution">how to manage the conflicts</param>
    /// <exception cref="ConflictException">Two properties can't be merged and conflictResolution is Exception</exception>
    private static void MergeChildrenCollection (object final, object other,
                                                 PropertyInfo property,
                                                 string childProperty,
                                                 ConflictResolution conflictResolution)
    {
      System.Collections.IEnumerable otherValue =
        property.GetValue (other, null) as System.Collections.IEnumerable;
      Debug.Assert (null != otherValue);

      System.Collections.ArrayList list = new System.Collections.ArrayList();
      foreach (object otherItem in otherValue) {
        list.Add (otherItem);
      }
      foreach (object otherItem in list) {
        otherItem.GetType ()
          .GetProperty (childProperty)
          .SetValue (otherItem, final, null);
      }
    }
    
    /// <summary>
    /// Try to merge two DateTime properties keeping the oldest
    /// </summary>
    /// <param name="final">Final object (not null)</param>
    /// <param name="other">Other object (not null)</param>
    /// <param name="property">property name</param>
    /// <param name="conflictResolution">how to manage the conflicts</param>
    /// <exception cref="ConflictException">Two properties can't be merged and conflictResolution is Exception</exception>
    internal static void MergeDateTimeWithOldest (object final, object other,
                                                  string property,
                                                  ConflictResolution conflictResolution)
    {
      Debug.Assert (null != final);
      Debug.Assert (null != other);
      
      Type type = final.GetType ();
      Debug.Assert (other.GetType ().Equals (type));
      PropertyInfo propertyInfo = type.GetProperty (property, typeof(DateTime));
      MergeDateTimeWithOldest (final, other, propertyInfo, conflictResolution);
    }
    
    /// <summary>
    /// Try to merge two DateTime properties keeping the oldest
    /// </summary>
    /// <param name="final"></param>
    /// <param name="other"></param>
    /// <param name="property"></param>
    /// <param name="conflictResolution">how to manage the conflicts</param>
    /// <exception cref="ConflictException">Two properties can't be merged and conflictResolution is Exception</exception>
    private static void MergeDateTimeWithOldest (object final, object other,
                                                 PropertyInfo property,
                                                 ConflictResolution conflictResolution)
    {
      DateTime otherValue = (DateTime) property.GetValue (other, null);
      DateTime finalValue = (DateTime) property.GetValue (final, null);
      if (otherValue < finalValue) {
        property.SetValue (final, otherValue, null);
      }
      else {
        property.SetValue (final, finalValue, null);
      }
    }
    
    /// <summary>
    /// Try to merge two DateTime properties keeping the newest
    /// </summary>
    /// <param name="final">Final object (not null)</param>
    /// <param name="other">Other object (not null)</param>
    /// <param name="property">property name</param>
    /// <param name="conflictResolution">how to manage the conflicts</param>
    /// <exception cref="ConflictException">Two properties can't be merged and conflictResolution is Exception</exception>
    internal static void MergeDateTimeWithNewest (object final, object other,
                                                  string property,
                                                  ConflictResolution conflictResolution)
    {
      Debug.Assert (null != final);
      Debug.Assert (null != other);
      
      Type type = final.GetType ();
      Debug.Assert (other.GetType ().Equals (type));
      PropertyInfo propertyInfo = type.GetProperty (property, typeof(DateTime));
      MergeDateTimeWithNewest (final, other, propertyInfo, conflictResolution);
    }
    
    /// <summary>
    /// Try to merge two DateTime properties keeping the newest
    /// </summary>
    /// <param name="final"></param>
    /// <param name="other"></param>
    /// <param name="property"></param>
    /// <param name="conflictResolution">how to manage the conflicts</param>
    /// <exception cref="ConflictException">Two properties can't be merged and conflictResolution is Exception</exception>
    private static void MergeDateTimeWithNewest (object final, object other,
                                                 PropertyInfo property,
                                                 ConflictResolution conflictResolution)
    {
      DateTime otherValue = (DateTime) property.GetValue (other, null);
      DateTime finalValue = (DateTime) property.GetValue (final, null);
      if (otherValue < finalValue) {
        property.SetValue (final, finalValue, null);
      }
      else {
        property.SetValue (final, otherValue, null);
      }
    }
    
    /// <summary>
    /// Merge the flagged properties with MergeAuto, MergeParent or MergeChildren
    /// </summary>
    /// <param name="final">not null</param>
    /// <param name="other">not null</param>
    /// <param name="conflictResolution">how to manage the conflicts</param>
    /// <exception cref="ConflictException">Two properties can't be merged and conflictResolution is set to Exception</exception>
    internal static void MergeAuto (object final, object other,
                                    ConflictResolution conflictResolution)
    {
      Debug.Assert (null != final);
      Debug.Assert (null != other);
      
      Type type = final.GetType ();
      
      PropertyInfo[] properties =
        type.GetProperties (BindingFlags.Instance
                            | BindingFlags.Public);
      foreach (PropertyInfo property in properties) {
        foreach (Attribute attribute in property.GetCustomAttributes (false)) {
          if (attribute.GetType () == typeof (MergeAutoAttribute)) {
            // MergeAuto
            if (property.PropertyType.Equals (typeof(int))
                || property.PropertyType.Equals (typeof(double))) {
              MergeDefined (final, other, property, conflictResolution);
            }
            else if (property.PropertyType == typeof(string)) {
              MergeString (final, other, property, conflictResolution);
            }
            else if (property.PropertyType == typeof(double?)) {
              MergeNullable<double> (final, other, property, conflictResolution);
            }
            else if (property.PropertyType == typeof(int?)) {
              MergeNullable<int> (final, other, property, conflictResolution);
            }
            else if (property.PropertyType == typeof (DateTime?)) {
              MergeNullable<DateTime> (final, other, property, conflictResolution);
            }
            else if (typeof (IReferenceData).IsAssignableFrom (property.PropertyType)) {
              MergeReferenceData (final, other, property, conflictResolution);
            }
            break;
          }
          else if (attribute.GetType () == typeof (MergeParentAttribute)) {
            // MergeParent
            MergeObject (final, other, property, conflictResolution);
            break;
          }
          else if (attribute.GetType () == typeof (MergeChildrenAttribute)) {
            // MergeChildren
            MergeChildrenAttribute mergeChildrenAttribute =
              attribute as MergeChildrenAttribute;
            MergeChildrenCollection (final, other, property,
                                     mergeChildrenAttribute.ChildProperty,
                                     conflictResolution);
            break;
          }
          else if (attribute.GetType () == typeof (MergeObjectAttribute)) {
            // MergeObject
            MergeObject (final, other, property, conflictResolution);
          }
          else if (attribute.GetType () == typeof (MergeDateTimeWithOldestAttribute)) {
            // MergeDateTimeWithOldest
            MergeDateTimeWithOldest (final, other, property, conflictResolution);
          }
          else if (attribute.GetType () == typeof (MergeDateTimeWithNewestAttribute)) {
            // MergeDateTimeWithNewest
            MergeDateTimeWithNewest (final, other, property, conflictResolution);
          }
        }
      }
    }
  }
}
