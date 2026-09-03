// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;

namespace Pomamo.CncModule
{
  /// <summary>
  /// Standard implementation of <see cref="ICncAlarm"/>
  /// </summary>
  [Serializable]
  public class CncAlarm : ICncAlarm
  {
    /// <summary>
    /// Value that is used when an identifying field is not known
    /// </summary>
    public const string Unknown = "unknown";

    string m_message = "";

    /// <summary>
    /// <see cref="ICncAlarm.CncInfo" />
    /// </summary>
    public string CncInfo { get; set; } = "";

    /// <summary>
    /// <see cref="ICncAlarm.CncSubInfo" />
    /// </summary>
    public string CncSubInfo { get; set; } = "";

    /// <summary>
    /// <see cref="ICncAlarm.Type" />
    /// </summary>
    public string Type { get; set; } = "";

    /// <summary>
    /// <see cref="ICncAlarm.Number" />
    /// </summary>
    public string Number { get; set; } = "";

    /// <summary>
    /// <see cref="ICncAlarm.Message" />
    ///
    /// The control characters are removed, since they are welcome
    /// neither in the data queue nor in the database
    /// </summary>
    public string Message
    {
      get { return m_message; }
      set { m_message = CncAlarmExtensions.CleanMessage (value); }
    }

    /// <summary>
    /// <see cref="ICncAlarm.Properties" />
    ///
    /// It is never null, since the alarm key relies on it
    /// </summary>
    public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string> ();

    /// <summary>
    /// Default constructor, for the serialization
    /// </summary>
    public CncAlarm ()
    {
    }

    /// <summary>
    /// Constructor without any sub info
    /// </summary>
    /// <param name="cncInfo">module that reports the alarm</param>
    /// <param name="type">type of the alarm</param>
    /// <param name="number">number of the alarm</param>
    public CncAlarm (string cncInfo, string type, string number)
    {
      this.CncInfo = string.IsNullOrEmpty (cncInfo) ? Unknown : cncInfo;
      this.Type = string.IsNullOrEmpty (type) ? Unknown : type;
      this.Number = string.IsNullOrEmpty (number) ? Unknown : number;
    }

    /// <summary>
    /// Constructor without any message
    /// </summary>
    /// <param name="cncInfo">module that reports the alarm</param>
    /// <param name="cncSubInfo">how the alarm was obtained</param>
    /// <param name="type">type of the alarm</param>
    /// <param name="number">number of the alarm</param>
    public CncAlarm (string cncInfo, string cncSubInfo, string type, string number)
      : this (cncInfo, type, number)
    {
      this.CncSubInfo = string.IsNullOrEmpty (cncSubInfo) ? Unknown : cncSubInfo;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cncInfo">module that reports the alarm</param>
    /// <param name="cncSubInfo">how the alarm was obtained</param>
    /// <param name="type">type of the alarm</param>
    /// <param name="number">number of the alarm</param>
    /// <param name="message">message of the alarm, nullable</param>
    public CncAlarm (string cncInfo, string cncSubInfo, string type, string number, string message)
      : this (cncInfo, cncSubInfo, type, number)
    {
      this.Message = message;
    }

    /// <summary>
    /// Constructor from the description <see cref="CncAlarmExtensions.ToDescription" /> returns
    ///
    /// It is the single string constructor the automatic converter looks for
    /// </summary>
    /// <param name="description">nullable</param>
    public CncAlarm (string description)
    {
      this.ParseDescription (description);
    }

    /// <summary>
    /// <see cref="object.Equals(object)" />
    ///
    /// Two alarms are equal when they carry the same values, whatever their implementation
    /// </summary>
    /// <param name="obj">nullable</param>
    public override bool Equals (object obj)
    {
      return this.HasSameValues (obj as ICncAlarm);
    }

    /// <summary>
    /// <see cref="object.GetHashCode" />
    /// </summary>
    public override int GetHashCode ()
    {
      return this.GetValueHashCode ();
    }

    /// <summary>
    /// <see cref="object.ToString" />
    /// </summary>
    public override string ToString ()
    {
      return this.ToDescription ();
    }

    /// <summary>
    /// Get a copy of this alarm
    /// </summary>
    public CncAlarm Clone ()
    {
      var other = new CncAlarm ();
      this.CopyTo (other);
      return other;
    }
  }
}
