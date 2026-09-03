// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Pomamo.CncModule;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Cnc alarm a module reports.
  ///
  /// Deprecated: use <see cref="Pomamo.CncModule.CncAlarm"/> instead, which is the same
  /// implementation in an assembly whose license, MIT, suits every cnc module, including the ones
  /// that are not compatible with the Apache-2.0 license of this one. Both implement
  /// <see cref="Pomamo.CncModule.ICncAlarm"/>, they compare equal to each other, and they share
  /// their behaviour through Pomamo.CncModule.CncAlarmExtensions, so the two are interchangeable.
  ///
  /// This class is kept, and must not be removed, so that the alarms an older version enqueued,
  /// which carry this type name, are still deserialized after an upgrade.
  /// </summary>
  [Serializable]
  [Obsolete ("Use Pomamo.CncModule.CncAlarm instead. This class is only kept to deserialize the alarms enqueued by an older version.")]
  public class CncAlarm: Pomamo.CncModule.ICncAlarm
  {
    string m_message = "";

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
    public string Number { get; set; } = "";

    /// <summary>
    /// Alarm Message
    /// </summary>
    public string Message
    {
      get { return m_message; }
      set {
        // Remove the control characters, in the same way as the other implementations
        m_message = CncAlarmExtensions.CleanMessage (value);
      }
    }

    /// <summary>
    /// Additionnal properties that might have an alarm
    /// Can be empty but not null
    /// Warning: using Dictionary with string / object will create a parsing bug in LemDataService (Fifo) 
    /// </summary>
    public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string> ();

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
      : this (cncInfo, alarmType, number)
    {
      CncSubInfo = String.IsNullOrEmpty (cncSubInfo) ? "unknown" : cncSubInfo;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public CncAlarm (string cncInfo, string cncSubInfo, string alarmType, string number, string message)
      : this (cncInfo, cncSubInfo, alarmType, number)
    {
      Message = message;
    }

    /// <summary>
    /// Create a CncAlarm based on a textual description
    /// </summary>
    /// <param name="objectStr"></param>
    public CncAlarm (string objectStr)
    {
      this.ParseDescription (objectStr);
    }

    /// <summary>
    /// Get a copy of this object
    /// </summary>
    /// <returns></returns>
    public CncAlarm Clone ()
    {
      var other = new CncAlarm ();
      this.CopyTo (other);
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
      // Compared through the interface, so that an alarm of this implementation and an alarm of
      // another one are equal when they carry the same values, in both directions
      return this.HasSameValues (obj as Pomamo.CncModule.ICncAlarm);
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
      return this.GetValueHashCode ();
    }

    /// <summary>
    /// ToString() method
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      return this.ToDescription ();
    }
  }
}
