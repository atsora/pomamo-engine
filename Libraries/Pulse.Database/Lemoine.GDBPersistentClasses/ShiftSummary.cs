// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table shiftsummary
  ///
  /// <see cref="IShiftSummary"/>
  /// </summary>
  public class ShiftSummary
    : IShiftSummary
    , IEquatable<IShiftSummary>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ShiftSummary).FullName);

    int m_id = 0;
    int m_version = 0;
    IMachine m_machine;
    DateTime m_day;
    IShift m_shift;
    DateTime m_dateTime = DateTime.UtcNow;

    /// <summary>
    /// <see cref="IShiftSummary"/>
    /// </summary>
    public virtual int Id => m_id;

    /// <summary>
    /// <see cref="IShiftSummary"/>
    /// </summary>
    public virtual int Version => m_version;

    /// <summary>
    /// <see cref="IShiftSummary"/>
    /// </summary>
    public virtual IMachine Machine => m_machine;

    /// <summary>
    /// <see cref="IShiftSummary"/>
    /// </summary>
    public virtual DateTime Day => m_day;

    /// <summary>
    /// <see cref="IShiftSummary"/>
    /// </summary>
    public virtual IShift Shift => m_shift;

    /// <summary>
    /// <see cref="IShiftSummary"/>
    /// </summary>
    public virtual UtcDateTimeRange DateTimeRange { get; set; }

    /// <summary>
    /// <see cref="IShiftSummary"/>
    /// </summary>
    public virtual DateTime DateTime => m_dateTime;

    /// <summary>
    /// Protected constructor with no arguments
    /// </summary>
    protected ShiftSummary ()
    {
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="day"></param>
    /// <param name="shift">nullable</param>
    /// <param name="range"></param>
    public ShiftSummary (IMachine machine, DateTime day, IShift shift, UtcDateTimeRange range)
    {
      if (machine is null) {
        log.Fatal ("ShiftSummary: machine is null");
        throw new ArgumentNullException (nameof (machine));
      }

      m_machine = machine;
      m_day = day.Date;
      m_shift = shift;
      this.DateTimeRange = range;
    }

    /// <summary>
    /// ToString
    /// </summary>
    /// <returns></returns>
    public override string ToString () => $"[ShiftSummary {this.Id}]";

    /// <summary>
    /// <see cref="IEquatable{T}"/>
    /// </summary>
    public virtual bool Equals (IShiftSummary other) => this.Equals ((object)other);

    /// <summary>
    /// <see cref="object.Equals(object)"/>
    /// </summary>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }
      // Note: do not compare the types, it may be compared with a proxy
      return (obj is IShiftSummary other) && (0 != this.Id) && (this.Id == other.Id);
    }

    /// <summary>
    /// <see cref="object.GetHashCode"/>
    /// </summary>
    public override int GetHashCode () => (0 != this.Id) ? this.Id.GetHashCode () : base.GetHashCode ();
  }

  /// <summary>
  /// Persistent class of table shiftsummarysection
  ///
  /// <see cref="IShiftSummarySection"/>
  /// </summary>
  public class ShiftSummarySection
    : IShiftSummarySection
    , IEquatable<IShiftSummarySection>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ShiftSummarySection).FullName);

    int m_id = 0;
    int m_version = 0;
    IShiftSummary m_shiftSummary;
    string m_sectionKey;

    /// <summary>
    /// <see cref="IShiftSummarySection"/>
    /// </summary>
    public virtual int Id => m_id;

    /// <summary>
    /// <see cref="IShiftSummarySection"/>
    /// </summary>
    public virtual int Version => m_version;

    /// <summary>
    /// <see cref="IShiftSummarySection"/>
    /// </summary>
    public virtual IShiftSummary ShiftSummary => m_shiftSummary;

    /// <summary>
    /// <see cref="IShiftSummarySection"/>
    /// </summary>
    public virtual string SectionKey => m_sectionKey;

    /// <summary>
    /// <see cref="IShiftSummarySection"/>
    /// </summary>
    public virtual string Items { get; set; } = "[]";

    /// <summary>
    /// <see cref="IShiftSummarySection"/>
    /// </summary>
    public virtual DateTime? DataUntil { get; set; }

    /// <summary>
    /// <see cref="IShiftSummarySection"/>
    /// </summary>
    public virtual bool UpdateRequired { get; set; } = true;

    /// <summary>
    /// <see cref="IShiftSummarySection"/>
    /// </summary>
    public virtual DateTime? DateTime { get; set; }

    /// <summary>
    /// Protected constructor with no arguments
    /// </summary>
    protected ShiftSummarySection ()
    {
    }

    /// <summary>
    /// Constructor of a section to compute
    /// </summary>
    /// <param name="shiftSummary">not null</param>
    /// <param name="sectionKey">not null and not empty</param>
    public ShiftSummarySection (IShiftSummary shiftSummary, string sectionKey)
    {
      if (shiftSummary is null) {
        log.Fatal ("ShiftSummarySection: shiftSummary is null");
        throw new ArgumentNullException (nameof (shiftSummary));
      }
      if (string.IsNullOrEmpty (sectionKey)) {
        log.Fatal ("ShiftSummarySection: sectionKey is null or empty");
        throw new ArgumentNullException (nameof (sectionKey));
      }

      m_shiftSummary = shiftSummary;
      m_sectionKey = sectionKey;
    }

    /// <summary>
    /// ToString
    /// </summary>
    /// <returns></returns>
    public override string ToString () => $"[ShiftSummarySection {this.Id} {this.SectionKey}]";

    /// <summary>
    /// <see cref="IEquatable{T}"/>
    /// </summary>
    public virtual bool Equals (IShiftSummarySection other) => this.Equals ((object)other);

    /// <summary>
    /// <see cref="object.Equals(object)"/>
    /// </summary>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }
      return (obj is IShiftSummarySection other) && (0 != this.Id) && (this.Id == other.Id);
    }

    /// <summary>
    /// <see cref="object.GetHashCode"/>
    /// </summary>
    public override int GetHashCode () => (0 != this.Id) ? this.Id.GetHashCode () : base.GetHashCode ();
  }

  /// <summary>
  /// Persistent class of table shiftsummarycontent
  ///
  /// <see cref="IShiftSummaryContent"/>
  /// </summary>
  public class ShiftSummaryContent
    : IShiftSummaryContent
    , IEquatable<IShiftSummaryContent>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ShiftSummaryContent).FullName);

    int m_id = 0;
    int m_version = 0;
    IShiftSummary m_shiftSummary;
    string m_generator;

    /// <summary>
    /// <see cref="IShiftSummaryContent"/>
    /// </summary>
    public virtual int Id => m_id;

    /// <summary>
    /// <see cref="IShiftSummaryContent"/>
    /// </summary>
    public virtual int Version => m_version;

    /// <summary>
    /// <see cref="IShiftSummaryContent"/>
    /// </summary>
    public virtual IShiftSummary ShiftSummary => m_shiftSummary;

    /// <summary>
    /// <see cref="IShiftSummaryContent"/>
    /// </summary>
    public virtual string Generator => m_generator;

    /// <summary>
    /// <see cref="IShiftSummaryContent"/>
    /// </summary>
    public virtual string Content { get; set; } = "{}";

    /// <summary>
    /// <see cref="IShiftSummaryContent"/>
    /// </summary>
    public virtual string Text { get; set; }

    /// <summary>
    /// <see cref="IShiftSummaryContent"/>
    /// </summary>
    public virtual string SourceVersion { get; set; } = "";

    /// <summary>
    /// <see cref="IShiftSummaryContent"/>
    /// </summary>
    public virtual DateTime DateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Protected constructor with no arguments
    /// </summary>
    protected ShiftSummaryContent ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="shiftSummary">not null</param>
    /// <param name="generator">not null and not empty</param>
    public ShiftSummaryContent (IShiftSummary shiftSummary, string generator)
    {
      if (shiftSummary is null) {
        log.Fatal ("ShiftSummaryContent: shiftSummary is null");
        throw new ArgumentNullException (nameof (shiftSummary));
      }
      if (string.IsNullOrEmpty (generator)) {
        log.Fatal ("ShiftSummaryContent: generator is null or empty");
        throw new ArgumentNullException (nameof (generator));
      }

      m_shiftSummary = shiftSummary;
      m_generator = generator;
    }

    /// <summary>
    /// ToString
    /// </summary>
    /// <returns></returns>
    public override string ToString () => $"[ShiftSummaryContent {this.Id} {this.Generator}]";

    /// <summary>
    /// <see cref="IEquatable{T}"/>
    /// </summary>
    public virtual bool Equals (IShiftSummaryContent other) => this.Equals ((object)other);

    /// <summary>
    /// <see cref="object.Equals(object)"/>
    /// </summary>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }
      return (obj is IShiftSummaryContent other) && (0 != this.Id) && (this.Id == other.Id);
    }

    /// <summary>
    /// <see cref="object.GetHashCode"/>
    /// </summary>
    public override int GetHashCode () => (0 != this.Id) ? this.Id.GetHashCode () : base.GetHashCode ();
  }
}
