// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;

namespace Lemoine.Model
{
  /// <summary>
  /// List of common application state keys
  /// </summary>
  public enum ApplicationStateKey
  {
    /// <summary>
    /// Key to track the latest update date/time of any config
    /// </summary>
    ConfigUpdate = 1
  }
  
  /// <summary>
  /// Extensions to AssociationOption
  /// </summary>
  public static class ApplicationStateKeyExtensions
  {
    /// <summary>
    /// Convert the enum value to the corresponding key
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static string ToKey (this ApplicationStateKey v)
    {
      switch (v) {
        case ApplicationStateKey.ConfigUpdate:
          return "config.update";
      }
      
      Debug.Assert (false);
      throw new NotImplementedException ();
    }
    
    /// <summary>
    /// Return the key
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static string ToString (this ApplicationStateKey v)
    {
      return v.ToKey ();
    }
  }
  
  /// <summary>
  /// Model for table applicationstate
  /// </summary>
  public interface IApplicationState: IVersionable
  {
    /// <summary>
    /// Application state key
    /// 
    /// Use the '.' separator to structure the key
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Application state value, a serializable object
    /// </summary>
    object Value { get; set; }
  }
}
