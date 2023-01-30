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

namespace Lemoine.Plugin.AcquisitionErrorEvent
{
  /// <summary>
  /// Persistent class EventAcquisitionError
  /// </summary>
  [Serializable]
  public class EventAcquisitionError
    : Lemoine.GDBPersistentClasses.EventMachineGeneric
  {
    static readonly string MACHINE_MODE_ID = "machinemodeid";
    static readonly string RANGE = "range";

    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (EventAcquisitionError).FullName);

    #region Getters / Setters
    [XmlIgnore]
    public virtual IMachineMode MachineMode
    {
      get
      {
        object machineModeIdObject;
        if (!this.Data.TryGetValue (MACHINE_MODE_ID, out machineModeIdObject)) {
          if (log.IsWarnEnabled) {
            log.Warn ($"MachineMode.get: {MACHINE_MODE_ID} is not in data => return null");
          }
          return null;
        }
        int machineModeId;
        try {
          machineModeId = Convert.ToInt32 (machineModeIdObject);
        }
        catch (InvalidCastException ex) {
          log.Fatal ($"MachineMode.get: {machineModeIdObject} is not a valid machine mode id (not an int) => return null", ex);
          throw;
        }
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var machineMode = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById (machineModeId);
          return machineMode;
        }
      }
      set
      {
        this.Data[MACHINE_MODE_ID] = ((Lemoine.Collections.IDataWithId)value).Id;
      }
    }

    [XmlElement ("MachineMode")]
    public virtual Lemoine.GDBPersistentClasses.MachineMode XmlSerializationMachineMode
    {
      get { return this.MachineMode as Lemoine.GDBPersistentClasses.MachineMode; }
      set { this.MachineMode = value; }
    }

    [XmlIgnore]
    public virtual UtcDateTimeRange Range
    {
      get { return new UtcDateTimeRange (this.SqlRange); }
    }

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
    /// <param name="machineMode">not null</param>
    /// <param name="range"></param>
    internal protected EventAcquisitionError (IEventLevel level, DateTime dateTime, IMachine machine,
      IMachineMode machineMode,
      UtcDateTimeRange range)
      : base (level, dateTime, machine)
    {
      Debug.Assert (null != machineMode);

      SetData (MACHINE_MODE_ID, ((Lemoine.Collections.IDataWithId)machineMode).Id);
      SetData ("range", range.ToString (dt => dt.ToString ("yyyy-MM-dd HH:mm:ss")));
    }

    protected EventAcquisitionError ()
    { }
    #endregion // Constructors
  }
}
