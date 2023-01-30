// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
#if NET6_0_OR_GREATER
using System.Text.Json.Serialization;
#endif // NET6_0_OR_GREATER

namespace Lemoine.Model
{
  /// <summary>
  /// Pattern rule for a cnc alarm severity
  /// </summary>
  public class CncAlarmSeverityPatternRules
  {
    string m_cncSubInfo = null;
    string m_type = null;
    string m_number = null;
    string m_message = null;

    #region Properties
    /// <summary>
    /// Match with a CncSubInfo
    /// </summary>
    [JsonProperty ("cncsubinfo")]
#if NET6_0_OR_GREATER
    [JsonPropertyName ("cncsubinfo")]
#endif // NET6_0_OR_GREATER
    [DefaultValue (null)]
    public string CncSubInfo
    {
      get { return m_cncSubInfo; }
      set {
        if (string.IsNullOrEmpty (value)) {
          m_cncSubInfo = null;
        }
        else {
          m_cncSubInfo = value;
        }
      }
    }

    /// <summary>
    /// Match with a type
    /// </summary>
    [JsonProperty ("type")]
#if NET6_0_OR_GREATER
    [JsonPropertyName ("type")]
#endif // NET6_0_OR_GREATER
    [DefaultValue (null)]
    public string Type
    {
      get {
        return m_type;
      }
      set {
        if (string.IsNullOrEmpty (value)) {
          m_type = null;
        }
        else {
          m_type = value;
        }
      }
    }

    /// <summary>
    /// Match with a number
    /// </summary>
    [JsonProperty ("number")]
#if NET6_0_OR_GREATER
    [JsonPropertyName ("number")]
#endif // NET6_0_OR_GREATER
    [DefaultValue (null)]
    public string Number
    {
      get {
        return m_number;
      }
      set {
        if (string.IsNullOrEmpty (value)) {
          m_number = null;
        }
        else {
          m_number = value;
        }
      }
    }

    /// <summary>
    /// Match with a message
    /// </summary>
    [JsonProperty ("message")]
#if NET6_0_OR_GREATER
    [JsonPropertyName ("message")]
#endif // NET6_0_OR_GREATER
    [DefaultValue (null)]
    public string Message
    {
      get {
        return m_message;
      }
      set {
        if (string.IsNullOrEmpty (value)) {
          m_message = null;
        }
        else {
          m_message = value;
        }
      }
    }

    /// <summary>
    /// Match with properties
    /// </summary>
    [JsonProperty ("properties")]
#if NET6_0_OR_GREATER
    [JsonPropertyName ("properties")]
#endif // NET6_0_OR_GREATER
    public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string> ();
    #endregion Properties

    /// <summary>
    /// Override of the method "Equals"
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public override bool Equals (object value)
    {
      CncAlarmSeverityPatternRules other = value as CncAlarmSeverityPatternRules;
      if (other == null) {
        return false;
      }

      if (!string.Equals (this.CncSubInfo, other.CncSubInfo)) {
        return false;
      }

      if (!string.Equals (this.Type, other.Type)) {
        return false;
      }

      if (!string.Equals (this.Number, other.Number)) {
        return false;
      }

      if (!string.Equals (this.Message, other.Message)) {
        return false;
      }

      if (this.Properties == null && other.Properties == null) {
        return true;
      }

      if (this.Properties == null || other.Properties == null) {
        return false;
      }

      if (this.Properties.Count != other.Properties.Count) {
        return false;
      }

      foreach (string key in this.Properties.Keys) {
        if (!other.Properties.ContainsKey (key) || !string.Equals (other.Properties[key], this.Properties[key])) {
          return false;
        }
      }

      // Everything is the same
      return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode ()
    {
      int hashCode = 0;

      unchecked {
        if (CncSubInfo != null) {
          hashCode += 1000000007 * CncSubInfo.GetHashCode ();
        }
        if (Type != null) {
          hashCode += 1000000009 * Type.GetHashCode ();
        }
        if (Number != null) {
          hashCode += 1000000021 * Number.GetHashCode ();
        }
        if (Message != null) {
          hashCode += 1000000033 * Message.GetHashCode ();
        }
        if (Properties != null) {
          hashCode += 1000000087 * Properties.GetHashCode ();
        }
      }

      return hashCode;
    }
  }

  /// <summary>
  /// Description of ICncAlarmSeverityPattern.
  /// </summary>
  public interface ICncAlarmSeverityPattern : IVersionable
  {
    /// <summary>
    /// Cnc that can have the severity pattern
    /// Cannot be null or empty
    /// </summary>
    string CncInfo { get; set; }

    /// <summary>
    /// CncAlarmSeverityPattern name
    /// Can be null or empty
    /// Used to recognize pattern and update them
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Status of the alarm severity pattern, useful to know if it can be updated or not
    /// 0: new (this is a new item that won't be updated)
    /// 1: default (update allowed)
    /// 2: edited (update forbidden)
    /// 3: deactivated (update forbidden and severity pattern disabled)
    /// </summary>
    EditStatus Status { get; set; }

    /// <summary>
    /// Rules used to detect severities in a CncAlarm
    /// (Stored as json in the database)
    /// </summary>
    CncAlarmSeverityPatternRules Rules { get; set; }

    /// <summary>
    /// Severity associated to the pattern and the cncInfo
    /// Cannot be null
    /// </summary>
    ICncAlarmSeverity Severity { get; set; }

    /// <summary>
    /// Get a textual description of what is in the pattern
    /// </summary>
    string Description { get; }
  }
}
