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

namespace Lemoine.Plugin.MachineStateTemplateChangeEvent
{
  /// <summary>
  /// Persistent class of table EventMachineStateTemplateChange
  /// </summary>
  [Serializable]
  public class EventMachineStateTemplateChange
    : Lemoine.GDBPersistentClasses.EventMachineGeneric
  {
    static readonly string NEW_MACHINE_STATE_TEMPLATE_ID = "newmachinestatetemplateid";
    static readonly string RANGE = "range";
    
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (EventMachineStateTemplateChange).FullName);

    #region Getters / Setters
    [XmlIgnore]
    public virtual IMachineStateTemplate NewMachineStateTemplate
    {
      get
      {
        object newMachineStateTemplateIdObject;
        if (!this.Data.TryGetValue (NEW_MACHINE_STATE_TEMPLATE_ID, out newMachineStateTemplateIdObject)) {
          if (log.IsWarnEnabled) {
            log.Warn ($"NewMachineStateTemplate.get: {NEW_MACHINE_STATE_TEMPLATE_ID} is not in data => return null");
          }
          return null;
        }
        int newMachineStateTemplateId = Convert.ToInt32 (newMachineStateTemplateIdObject);
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
        {
          var newMachineStateTemplate = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
            .FindById (newMachineStateTemplateId);
          return newMachineStateTemplate;
        }
      }
      set
      {
        this.Data[NEW_MACHINE_STATE_TEMPLATE_ID] = value.Id;
      }
    }
    
    [XmlElement("NewMachineStateTemplate")]
    public virtual Lemoine.GDBPersistentClasses.MachineStateTemplate XmlSerializationNewMachineStateTemplate {
      get { return this.NewMachineStateTemplate as Lemoine.GDBPersistentClasses.MachineStateTemplate; }
      set { this.NewMachineStateTemplate = value; }
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
    [XmlAttribute("Range")]
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
    
    [XmlAttribute("LocalRange")]
    public virtual string SqlLocalRange
    {
      get { return this.LocalRange.ToString (dt => dt.ToString ("yyyy-MM-dd HH:mm:ss")); }
      // disable once ValueParameterNotUsed
      set { }
    }
    
    [XmlAttribute("LocalBegin")]
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
    
    [XmlAttribute("LocalEnd")]
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
    /// <param name="newMachineStateTemplate">not null</param>
    /// <param name="range"></param>
    internal protected EventMachineStateTemplateChange (IEventLevel level, DateTime dateTime, IMachine machine,
                                                        IMachineStateTemplate newMachineStateTemplate,
                                                        UtcDateTimeRange range)
      : base (level, dateTime, machine)
    {
      Debug.Assert (null != newMachineStateTemplate);
      
      SetData (NEW_MACHINE_STATE_TEMPLATE_ID, newMachineStateTemplate.Id);
      SetData ("range", range.ToString (dt => dt.ToString ("yyyy-MM-dd HH:mm:ss")));
    }

    protected EventMachineStateTemplateChange ()
    { }
    #endregion // Constructors
  }
}
