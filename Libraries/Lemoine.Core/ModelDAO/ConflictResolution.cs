// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Core.Log;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// How to resolve a conflict ?
  /// </summary>
  public enum ConflictResolution
  {
    /// <summary>
    /// Keep unchanged the data
    /// </summary>
    Keep,
    /// <summary>
    /// Overwrite the data
    /// </summary>
    Overwrite,
    /// <summary>
    /// Raise <see cref="ConflictException" />
    /// </summary>
    Exception
  }
  
  /// <summary>
  /// ConflictResolution utility methods
  /// </summary>
  public static class ConflictResolutionMethods
  {
    /// <summary>
    /// Inverse the conflict resolution value
    /// </summary>
    /// <param name="initial"></param>
    /// <returns></returns>
    public static ConflictResolution Inverse(ConflictResolution initial)
    {
      switch (initial) {
        case ConflictResolution.Keep:
          return ConflictResolution.Overwrite;
        case ConflictResolution.Overwrite:
          return ConflictResolution.Keep;
        case ConflictResolution.Exception:
          return ConflictResolution.Exception;
        default:
          Debug.Assert (false);
          throw new NotImplementedException ();
      }
    }
  }
  
  /// <summary>
  /// Conflict between two data that may prevent a given operation
  /// </summary>
  public class ConflictException: Exception
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ConflictException).FullName);
    
    object m_oldData;
    object m_newData;
    string m_property;
    
    #region Getters / Setters
    /// <summary>
    /// Old data
    /// </summary>
    public object OldData {
      get { return m_oldData; }
    }
    
    /// <summary>
    /// New data
    /// </summary>
    public object NewData {
      get { return m_newData; }
    }
    
    /// <summary>
    /// Property impacted
    /// </summary>
    public string Property {
      get { return m_property; }
    }
    #endregion // Getters / Setters
    
    /// <summary>
    /// Forbidden constructor
    /// </summary>
    protected ConflictException ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="oldData"></param>
    /// <param name="newData"></param>
    /// <param name="property"></param>
    public ConflictException(object oldData, object newData, string property)
    {
      this.m_oldData = oldData;
      this.m_newData = newData;
      this.m_property = property;
    }
    
    /// <summary>
    /// <see cref="Exception.ToString" />
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return string.Format("ConflictException [old={0} new={1} property={2}] {3}",
                           m_oldData, m_newData, m_property, base.ToString ());
    }
    
  }
}
