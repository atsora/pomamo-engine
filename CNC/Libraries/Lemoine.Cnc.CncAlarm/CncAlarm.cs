// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of CncAlarm.
  /// </summary>
  [Serializable]
  public class CncAlarm
  {
    #region Members
    string m_message = "";
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Alarm Cnc info
    /// Identify the CNC module that has created the alarm
    /// </summary>
    public string CncInfo { get; set; } = "";

    /// <summary>
    /// Alarm Sub info
    /// Identify more thoroughly the way an alarm has been created
    /// For example a specific version for the Fanuc CNC module (30i, 15, PH)
    /// or a file that has been used (machine alarms for Fanuc)
    /// </summary>
    public string CncSubInfo { get; set; } = "";

    /// <summary>
    /// Type of the alarm
    /// </summary>
    public string Type { get; set; } = "";

    /// <summary>
    /// Alarm Number
    /// </summary>
    public string Number { get; private set; } = "";

    /// <summary>
    /// Alarm Message
    /// </summary>
    public string Message
    {
      get { return m_message; }
      set {
        // Remove special characters (0-31)
        m_message = Regex.Replace (value, @"[\x00-\x1f]+", "");
      }
    }

    /// <summary>
    /// Additionnal properties that might have an alarm
    /// Can be empty but not null
    /// Warning: using Dictionary with string / object will create a parsing bug in LemDataService (Fifo) 
    /// </summary>
    public IDictionary<string, string> Properties { get; private set; } = new Dictionary<string, string> ();
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor for serialization
    /// </summary>
    public CncAlarm ()
    {
    }

    /// <summary>
    /// Creation of an alarm with a cncInfo
    /// </summary>
    /// <param name="cncInfo"></param>
    /// <param name="alarmType"></param>
    /// <param name="number"></param>
    public CncAlarm (string cncInfo, string alarmType, string number)
    {
      CncInfo = String.IsNullOrEmpty (cncInfo) ? "unknown" : cncInfo;
      Type = String.IsNullOrEmpty (alarmType) ? "unknown" : alarmType;
      Number = String.IsNullOrEmpty (number) ? "unknown" : number;
    }

    /// <summary>
    /// Creation of an alarm with a cncInfo and a cncSubInfo
    /// </summary>
    /// <param name="cncInfo"></param>
    /// <param name="cncSubInfo"></param>
    /// <param name="alarmType"></param>
    /// <param name="number"></param>
    public CncAlarm (string cncInfo, string cncSubInfo, string alarmType, string number)
    {
      CncInfo = String.IsNullOrEmpty (cncInfo) ? "unknown" : cncInfo;
      CncSubInfo = String.IsNullOrEmpty (cncSubInfo) ? "unknown" : cncSubInfo;
      Type = String.IsNullOrEmpty (alarmType) ? "unknown" : alarmType;
      Number = String.IsNullOrEmpty (number) ? "unknown" : number;
    }

    /// <summary>
    /// Create a CncAlarm based on a textual description
    /// </summary>
    /// <param name="objectStr"></param>
    public CncAlarm (string objectStr)
    {
      ParseString (objectStr);
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get a copy of this object
    /// </summary>
    /// <returns></returns>
    public CncAlarm Clone ()
    {
      var other = new CncAlarm (this.CncInfo, this.Type, this.Number);
      other.Message = this.Message;
      foreach (var key in this.Properties.Keys) {
        other.Properties[key] = this.Properties[key];
      }

      return other;
    }

    /// <summary>
    /// Return true if two alarms can be considered as similar
    /// (the time being not checked)
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }

      var other = obj as CncAlarm;
      if (other == null) {
        return false;
      }

      if (Object.Equals (this.Type, other.Type) &&
          Object.Equals (this.CncInfo, other.CncInfo) &&
          Object.Equals (this.CncSubInfo, other.CncSubInfo) &&
          Object.Equals (this.Number, other.Number) &&
          Object.Equals (this.Message, other.Message) &&
          this.Properties.Count == other.Properties.Count) {
        // Check all properties
        var keys = this.Properties.Keys;
        foreach (var key in keys) {
          if (!other.Properties.ContainsKey (key) ||
              !string.Equals (this.Properties[key], other.Properties[key])) {
            return false;
          }
        }
      }
      else {
        return false;
      }

      return true;
    }

    /// <summary>
    /// Determines whether the specified Object is equal to the current Object
    /// </summary>
    /// <param name="other">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public virtual bool Equals (CncAlarm other)
    {
      return this.Equals ((object)other);
    }

    /// <summary>
    /// Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode ()
    {
      int hashCode = 0;

      unchecked {
        if (CncInfo != null) {
          hashCode += 1000000007 * CncInfo.GetHashCode ();
        }

        if (Type != null) {
          hashCode += 1000000009 * Type.GetHashCode ();
        }

        hashCode += 1000000021 * Number.GetHashCode ();
        if (Message != null) {
          hashCode += 1000000033 * Message.GetHashCode ();
        }

        if (Properties != null) {
          hashCode += 1000000087 * Properties.GetHashCode ();
        }

        if (CncSubInfo != null) {
          hashCode += 1000000093 * CncSubInfo.GetHashCode ();
        }
      }

      return hashCode;
    }

    /// <summary>
    /// ToString() method
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      return String.Format ("CncInfo={0}; Type={1}; Number={2}; Message={3}", CncInfo, Type, Number, Message);
    }

    void ParseString (string objectStr)
    {
      var mainSplit = objectStr.Split (';');
      foreach (var elt in mainSplit) {
        var split = elt.Trim ().Split ('=');
        if (split.Length == 2) {
          switch (split[0]) {
          case "CncInfo":
            CncInfo = split[1];
            break;
          case "Type":
            Type = split[1];
            break;
          case "Number":
            Number = split[1];
            break;
          case "Message":
            Message = split[1];
            break;
          default:
            // Ignore
            break;
          }
        }
      }
    }
    #endregion // Methods
  }
}
