// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023-2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Identity of a cnc alarm, as opposed to its content: two alarms that share the same key are
  /// considered to be the same alarm, reported at two different moments.
  ///
  /// The message is deliberately not part of the key, so that an alarm whose wording changes
  /// remains the same alarm. The properties are part of it, except the ones that change
  /// constantly, see EXCLUDED_PROPERTIES.
  ///
  /// It is used:
  /// <list type="bullet">
  /// <item>
  /// to keep the continuity of the alarms when they are imported. Lemoine.CncDataImport keys its
  /// cache (Cache.CacheAlarm) with it, so that ImportDataCncAlarm extends the period of the alarm
  /// that is already stored instead of creating a new one, unless the alarm was not reported for
  /// longer than the maximum gap. This runs in Lem_CncDataService, through Lemoine.CncDataImport;
  /// </item>
  /// <item>
  /// to maintain the table of the current alarms, in the Current modules, both the C# one
  /// (Lemoine.Cnc.Current) and the F# one (Atsora.Cnc.Current): the alarms an acquisition reports
  /// are compared with the rows that are stored, to know which ones to add, keep or remove.
  /// </item>
  /// </list>
  ///
  /// It is only a key of an in-memory dictionary: it is never serialized, neither in the data
  /// queue nor in the database.
  ///
  /// It is built either from a <see cref="Pomamo.CncModule.ICncAlarm"/>, which is what a cnc
  /// module reports, or from the fields of a stored alarm, whose properties are objects rather
  /// than strings.
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
    /// <param name="alarm">not null</param>
    public AlarmKey (Pomamo.CncModule.ICncAlarm alarm)
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

      if (!object.Equals (this.CncInfo, other.CncInfo)
        || !object.Equals (this.CncSubInfo, other.CncSubInfo)
        || !object.Equals (this.Type, other.Type)
        || !object.Equals (this.Number, other.Number)) {
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
