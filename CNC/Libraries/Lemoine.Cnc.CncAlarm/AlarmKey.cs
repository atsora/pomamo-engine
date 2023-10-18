// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of AlarmKey.
  /// </summary>
  public class AlarmKey: IEquatable<AlarmKey>
  {
    #region Members
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
    bool IsKeyProperty (string k) {
      return !EXCLUDED_PROPERTIES.Contains (k);
    }

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

      if (false == object.Equals (this.CncInfo, other.CncInfo)
        && object.Equals (this.CncSubInfo, other.CncSubInfo)
        && object.Equals (this.Type, other.Type)
        && object.Equals (this.Number, other.Number)) {
        return false;
      }

      // Properties
      var a = this.Properties.Where (kv => IsKeyProperty (kv.Key));
      var b = this.Properties.Where (kv => IsKeyProperty (kv.Key));
      if (a.Count () != b.Count ()) {
        return false;
      }
      return object.Equals (a.ToDictionary (kv => kv.Key), a.ToDictionary (kv => kv.Key));
    }
    
    /// <summary>
    /// GetHashCode
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        if (CncInfo != null) {
          hashCode += 1000000007 * CncInfo.GetHashCode ();
        }

        if (CncSubInfo != null) {
          hashCode += 1000000093 * CncSubInfo.GetHashCode ();
        }

        if (Type != null) {
          hashCode += 1000000009 * Type.GetHashCode ();
        }

        if (Number != null) {
          hashCode += 1000000021 * Number.GetHashCode ();
        }

        if (Properties != null) {
          // Concat keys and values
          string concat = "";
          foreach (var key in Properties.Keys) {
            if (Array.IndexOf (EXCLUDED_PROPERTIES, key) == -1) { // The key must not be excluded
              concat += key + "|" + Properties[key] + "|";
            }
          }

          hashCode += 1000000033 * concat.GetHashCode ();
        }
      }
      return hashCode;
    }

    /// <summary>
    /// <see cref="IEquatable{T}"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals (AlarmKey other)
    {
      return Equals ((object)other);
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
