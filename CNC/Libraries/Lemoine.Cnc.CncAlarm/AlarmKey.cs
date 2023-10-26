// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023 Atsora Solutions
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
  public class AlarmKey : IEquatable<AlarmKey>
  {
    // Some properties may change constantly but we still want to merge the alarms
    // In that case, the first occurrence of the property will be taken into account
    static readonly string[] EXCLUDED_PROPERTIES = new string[] {
      "Execution block" // MML3
    };

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

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="cncInfo"></param>
    /// <param name="cncSubInfo"></param>
    /// <param name="type"></param>
    /// <param name="number"></param>
    /// <param name="properties"></param>
    public AlarmKey (string cncInfo, string cncSubInfo, string type, string number, IDictionary<string, object> properties)
    {
      this.CncInfo = cncInfo;
      this.CncSubInfo = cncSubInfo;
      this.Type = type;
      this.Number = number;
      this.Properties = new Dictionary<string, string> ();
      foreach (var kv in properties) {
        this.Properties[kv.Key] = kv.Value?.ToString ();
      }
    }

    /// <summary>
    /// Constructor based on a cnc alarm
    /// </summary>
    /// <param name="alarm"></param>
    public AlarmKey (CncAlarm alarm)
    {
      CncInfo = alarm.CncInfo;
      CncSubInfo = alarm.CncSubInfo;
      Type = alarm.Type;
      Number = alarm.Number;
      Properties = alarm.Properties;
    }

    /// <summary>
    /// ToString override
    /// </summary>
    /// <returns></returns>
    public override string ToString () => $"[AlarmKey CncInfo={this.CncInfo}, CncSubInfo={this.CncSubInfo}, Type={this.Type}, Number={this.Number}, Properties number={this.Properties.Count}]";

    #region Equals and GetHashCode implementation
    bool IsKeyProperty (string k) => !EXCLUDED_PROPERTIES.Contains (k);

    /// <summary>
    /// Equals
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals (object obj)
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
      var b = other.Properties.Where (kv => IsKeyProperty (kv.Key));
      if (a.Count () != b.Count ()) {
        return false;
      }
      return !a.Except (b).Any ();
    }

    /// <summary>
    /// GetHashCode
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode ()
    {
      int hashCode = 0;
      unchecked {
        if (this.CncInfo != null) {
          hashCode += 1000000007 * CncInfo.GetHashCode ();
        }

        if (this.CncSubInfo != null) {
          hashCode += 1000000093 * CncSubInfo.GetHashCode ();
        }

        if (this.Type != null) {
          hashCode += 1000000009 * Type.GetHashCode ();
        }

        if (this.Number != null) {
          hashCode += 1000000021 * Number.GetHashCode ();
        }

        if (this.Properties != null) {
          // Concat keys and values
          string concat = "";
          foreach (var kv in this.Properties.OrderBy (kv => kv.Key)) {
            if (Array.IndexOf (EXCLUDED_PROPERTIES, kv.Key) == -1) { // The key must not be excluded
              concat += kv.Key + "|" + kv.Value + "|";
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
      return this.Equals ((object)other);
    }

    /// <summary>
    /// ==
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static bool operator == (AlarmKey lhs, AlarmKey rhs)
    {
      if (ReferenceEquals (lhs, rhs)) {
        return true;
      }

      if (ReferenceEquals (lhs, null) || ReferenceEquals (rhs, null)) {
        return false;
      }

      return lhs.Equals (rhs);
    }

    /// <summary>
    /// !=
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static bool operator != (AlarmKey lhs, AlarmKey rhs)
    {
      return !(lhs == rhs);
    }
    #endregion // Equals and GetHashCode implementation
  }
}
