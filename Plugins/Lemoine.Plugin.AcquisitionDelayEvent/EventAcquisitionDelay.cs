// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Model;

using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.AcquisitionDelayEvent
{
  /// <summary>
  /// Persistent class EventAcquisitionDelay
  /// </summary>
  [Serializable]
  public class EventAcquisitionDelay
    : Lemoine.GDBPersistentClasses.EventMachineGeneric
  {
    static readonly string RANGE = "range";

    static readonly ILog log = LogManager.GetLogger (typeof (EventAcquisitionDelay).FullName);

    #region Getters / Setters
    [XmlIgnore]
    public virtual UtcDateTimeRange Range => new UtcDateTimeRange (this.SqlRange);

    [XmlIgnore]
    public virtual LocalDateTimeRange LocalRange
    {
      get { return this.Range.ToLocalTime (); }
    }

    /// <summary>
    /// Range for Xml serialization
    /// </summary>
    [XmlAttribute ("Range")]
    public virtual string SqlRange
    {
      get
      {
        return (string)this.Data[RANGE];
      }
      set
      {
        SetData (RANGE, value);
      }
    }

    [XmlAttribute ("LocalRange")]
    public virtual string SqlLocalRange
    {
      get { return this.LocalRange.ToString (dt => dt.ToString ("yyyy-MM-dd HH:mm:ss")); }
      // disable once ValueParameterNotUsed
      set { }
    }

    [XmlAttribute ("LocalBegin")]
    public virtual string SqlLocalBegin
    {
      get
      {
        var lower = this.LocalRange.Lower;
        if (lower.HasValue) {
          return lower.Value.ToString ("yyyy-MM-dd HH:mm:ss");
        }
        else {
          return "";
        }
      }
      // disable once ValueParameterNotUsed
      set { }
    }

    [XmlAttribute ("LocalEnd")]
    public virtual string SqlLocalEnd
    {
      get
      {
        var upper = this.LocalRange.Upper;
        if (upper.HasValue) {
          return upper.Value.ToString ("yyyy-MM-dd HH:mm:ss");
        }
        else {
          return "";
        }
      }
      // disable once ValueParameterNotUsed
      set { }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="level"></param>
    /// <param name="dateTime"></param>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    internal protected EventAcquisitionDelay (IEventLevel level, DateTime dateTime, IMachine machine,
      UtcDateTimeRange range)
      : base (level, dateTime, machine)
    {
      SetData ("range", range.ToString (dt => dt.ToString ("yyyy-MM-dd HH:mm:ss")));
    }

    protected EventAcquisitionDelay ()
    { }
    #endregion // Constructors
  }
}
