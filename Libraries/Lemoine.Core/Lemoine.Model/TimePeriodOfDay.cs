// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Serialization;
using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// Time period in the day
  /// </summary>
  [Serializable]
  public struct TimePeriodOfDay : IEquatable<TimePeriodOfDay>
  {
    static readonly TimeSpan BEGIN_OF_DAY = TimeSpan.FromHours (0);
    static readonly TimeSpan END_OF_DAY = TimeSpan.FromHours (24);

    #region Members
    TimeSpan m_begin;
    TimeSpan m_end;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (TimePeriodOfDay).FullName);

    #region Getters / Setters
    /// <summary>
    /// Local begin time
    /// 
    /// Fraction of the day that has elapsed since local midnight
    /// </summary>
    [XmlIgnore]
    public TimeSpan Begin
    {
      get { return m_begin; }
    }

    /// <summary>
    /// Local end time
    /// 
    /// Fraction of the day that has elapsed since local midnight
    /// </summary>
    [XmlIgnore]
    public TimeSpan End
    {
      get { return m_end; }
    }

    /// <summary>
    /// End offset compared to the local begin time of the day
    /// 
    /// Convert 0:00 to 24:00 if applicable
    /// </summary>
    [XmlIgnore]
    public TimeSpan EndOffset
    {
      get {
        if (BEGIN_OF_DAY.Equals (m_end)) {
          return END_OF_DAY;
        }
        else {
          return m_end;
        }
      }
    }

    /// <summary>
    /// Value used for XML (de)serialization
    /// </summary>
    [XmlText]
    public string XmlValue
    {
      get { return this.ToString (); }
      set {
        string[] ss = value.Split (new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
        if (2 != ss.Length) {
          log.ErrorFormat ("XmlValue: " +
                           "invalid argument, no separator '-' in {0}",
                           value);
          throw new ArgumentException ("no separator '-'");
        }

        m_begin = TimeSpan.Parse (ss[0]);
        m_end = TimeSpan.Parse (ss[1]);
        if (END_OF_DAY == m_end) { // Replace 24:00 by 0:00
          log.DebugFormat ("XmlValue: " +
                           "replace 24:00 by 0:00");
          m_end = BEGIN_OF_DAY;
        }

        if (!IsValid ()) {
          log.ErrorFormat ("XmlValue: " +
                           "invalid TimePeriofOfDay {0}, begin after end",
                           ss);
          throw new ArgumentException ("begin after end");
        }
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor from HH:mm-HH:mm
    /// or HH:mm:ss-HH:mm:ss
    /// </summary>
    /// <param name="s"></param>
    public TimePeriodOfDay (string s)
    {
      string[] ss = s.Split (new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
      if (2 != ss.Length) {
        log.ErrorFormat ("TimePeriodOfDay: " +
                         "invalid argument, no separator '-' in {0}",
                         s);
        throw new ArgumentException ("no separator '-'");
      }

      m_begin = TimeSpan.Parse (ss[0]);
      m_end = TimeSpan.Parse (ss[1]);
      if (END_OF_DAY == m_end) { // Replace 24:00 by 0:00
        log.DebugFormat ("TimePeriofOfDay: " +
                         "replace 24:00 by 0:00");
        m_end = BEGIN_OF_DAY;
      }

      if (!IsValid ()) {
        log.ErrorFormat ("TimePeriodOfDay: " +
                         "invalid TimePeriofOfDay {0}, begin after end",
                         s);
        throw new ArgumentException ("begin after end");
      }
    }

    /// <summary>
    /// Constructor
    /// Throw an exception if the time period is not valid
    /// </summary>
    /// <param name="begin">local time</param>
    /// <param name="end">local time</param>
    public TimePeriodOfDay (TimeSpan begin, TimeSpan end) : this (begin, end, true) { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="begin">local time</param>
    /// <param name="end">local time</param>
    /// <param name="withException">an exception can be raised if the time period is not valid</param>
    public TimePeriodOfDay (TimeSpan begin, TimeSpan end, bool withException)
    {
      if (END_OF_DAY <= begin) {
        log.ErrorFormat ("TimePeriodOfDay: " +
                         "begin argument {0} is greater than 24 hours",
                         begin);
        throw new ArgumentException ("Invalid begin argument");
      }
      if (END_OF_DAY < end) {
        log.ErrorFormat ("TimePeriodOfDay: " +
                         "end argument {0} is strictly greater than 24 hours",
                         end);
        throw new ArgumentException ("Invalid end argument");
      }

      m_begin = begin;
      m_end = (END_OF_DAY == end) ? BEGIN_OF_DAY : end;

      if (withException && !IsValid ()) {
        log.ErrorFormat ("TimePeriodOfDay: " +
                         "invalid TimePeriofOfDay, begin {0} after end {1}",
                         begin, end);
        throw new ArgumentException ("begin after end");
      }
    }
    #endregion // Constructors

    /// <summary>
    /// Check if it is valid, it means End is strictly after Begin
    /// </summary>
    /// <returns></returns>
    public bool IsValid ()
    {
      return (BEGIN_OF_DAY == m_end) // End=0:00 is always valid
        || (m_begin < m_end);
    }

    /// <summary>
    /// Does the time period correspond to a full day ?
    /// It means is it 0:00-0:00 ?
    /// </summary>
    /// <returns></returns>
    public bool IsFullDay ()
    {
      return (BEGIN_OF_DAY == m_begin) && (BEGIN_OF_DAY == m_end);
    }

    /// <summary>
    /// <see cref="Object.ToString"></see>
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      return string.Format ("{0}-{1}", m_begin, m_end);
    }

    /// <summary>
    /// Do the two time periods overlap with each other ?
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Overlaps (TimePeriodOfDay other)
    {
      if (!this.IsValid () || !other.IsValid ()) {
        log.DebugFormat ("Overlaps: " +
                         "return false because one of the time periods {0} and {1} is not valid",
                         this, other);
        return false;
      }

      return (this.Begin < other.EndOffset) && (other.Begin < this.EndOffset);
    }

    /// <summary>
    /// Intersect two time periods
    /// 
    /// The two periods must intersect with each other, else the exception InvalidOperationException is raised
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">the two time periods do not overlap with each other</exception>
    public TimePeriodOfDay Intersects (TimePeriodOfDay other)
    {
      if (!this.Overlaps (other)) { // The two time periods do not overlap with each other => raise an exception
        log.ErrorFormat ("Intersects: " +
                         "the two time periods {0} and {1} does not overlap with each other",
                        this, other);
        throw new InvalidOperationException ("Intersects: not overlapping periods");
      }

      TimeSpan begin = this.Begin < other.Begin
        ? other.Begin
        : this.Begin; // Get the maximum
      TimeSpan end = this.EndOffset < other.EndOffset
        ? this.EndOffset
        : other.EndOffset; // Get the minimum
      Debug.Assert (begin < end); // Else the two time periods do not overlap
      return new TimePeriodOfDay (begin, end);
    }

    #region Equals and GetHashCode implementation
    /// <summary>
    /// Indicates whether the current object
    /// is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public override bool Equals (object other)
    {
      if (other is TimePeriodOfDay) {
        return Equals ((TimePeriodOfDay)other); // use Equals method below
      }
      else {
        return false;
      }
    }

    /// <summary>
    /// Determines whether the specified Object
    /// is equal to the current Object
    /// </summary>
    /// <param name="other">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public bool Equals (TimePeriodOfDay other)
    {
      if (null == other) {
        return false;
      }
      else {
        return m_begin.Equals (other.m_begin)
          && m_end.Equals (other.m_end);
      }
    }

    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode ()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * Begin.GetHashCode ();
        hashCode += 1000000011 * End.GetHashCode ();
      }
      return hashCode;
    }

    /// <summary>
    /// Equality
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator == (TimePeriodOfDay left, TimePeriodOfDay right)
    {
      return left.Equals (right);
    }

    /// <summary>
    /// Inequality
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator != (TimePeriodOfDay left, TimePeriodOfDay right)
    {
      return !left.Equals (right);
    }
    #endregion // Equals and GetHashCode implementation
  }
}
