// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of AlarmKey.
  /// </summary>
  public class AlarmKey
  {
    #region Members
    readonly int m_hashCode = 0;

    // Some properties may change constantly but we still want to merge the alarms
    // In that case, the first occurrence of the property will be taken into account
    static readonly string[] EXCLUDED_PROPERTIES = new string[] {
      "Execution block" // MML3
    };
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// CncInfo
    /// </summary>
    public string CncInfo { get; private set; }
    
    /// <summary>
    /// CncInfo
    /// </summary>
    public string CncSubInfo { get; private set; }
    
    /// <summary>
    /// Type
    /// </summary>
    public string Type { get; private set; }
    
    /// <summary>
    /// Number
    /// </summary>
    public string Number { get; private set; }
    
    /// <summary>
    /// Properties
    /// </summary>
    public IDictionary<string, string> Properties { get; private set; }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="cncInfo"></param>
    /// <param name="cncSubInfo"></param>
    /// <param name="type"></param>
    /// <param name="number"></param>
    /// <param name="properties"></param>
    public AlarmKey(string cncInfo, string cncSubInfo, string type, string number, IDictionary<string, object> properties)
    {
      CncInfo = cncInfo;
      CncSubInfo = cncSubInfo;
      Type = type;
      Number = number;
      Properties = new Dictionary<string, string>();
      foreach (var key in properties.Keys) {
        Properties[key] = (properties[key] == null ? null : properties[key].ToString());
      }

      m_hashCode = ComputeHashCode();
    }
    
    /// <summary>
    /// Constructor based on a cnc alarm
    /// </summary>
    /// <param name="alarm"></param>
    public AlarmKey(CncAlarm alarm)
    {
      CncInfo = alarm.CncInfo;
      CncSubInfo = alarm.CncSubInfo;
      Type = alarm.Type;
      Number = alarm.Number;
      Properties = alarm.Properties;
      m_hashCode = ComputeHashCode();
    }
    #endregion // Constructors
    
    /// <summary>
    /// ToString override
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return string.Format("[AlarmKey CncInfo={0}, CncSubInfo={1}, Type={2}, Number={3}, Properties number={4}]",
                           CncInfo, CncSubInfo, Type, Number, Properties.Count);
    }
    
    #region Equals and GetHashCode implementation
    /// <summary>
    /// Equals
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
      var other = obj as AlarmKey;
      if (other == null) {
        return false;
      }

      return this.m_hashCode == other.m_hashCode;
    }
    
    int ComputeHashCode()
    {
      int hashCode = 0;
      unchecked {
        if (CncInfo != null) {
          hashCode += 1000000007 * CncInfo.GetHashCode();
        }

        if (CncSubInfo != null) {
          hashCode += 1000000093 * CncSubInfo.GetHashCode();
        }

        if (Type != null) {
          hashCode += 1000000009 * Type.GetHashCode();
        }

        if (Number != null) {
          hashCode += 1000000021 * Number.GetHashCode();
        }

        if (Properties != null) {
          // Concat keys and values
          string concat = "";
          foreach (var key in Properties.Keys) {
            if (Array.IndexOf (EXCLUDED_PROPERTIES, key) == -1) { // The key must not be excluded
              concat += key + "|" + Properties[key] + "|";
            }
          }

          hashCode += 1000000033 * concat.GetHashCode();
        }
      }
      return hashCode;
    }

    /// <summary>
    /// GetHashCode
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      return m_hashCode;
    }

    /// <summary>
    /// ==
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static bool operator ==(AlarmKey lhs, AlarmKey rhs) {
      if (ReferenceEquals(lhs, rhs)) {
        return true;
      }

      if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null)) {
        return false;
      }

      return lhs.Equals(rhs);
    }

    /// <summary>
    /// !=
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static bool operator !=(AlarmKey lhs, AlarmKey rhs) {
      return !(lhs == rhs);
    }
    #endregion // Equals and GetHashCode implementation
  }
}
