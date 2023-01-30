// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table CurrentCncValue
  /// </summary>
  [Serializable]
  public class CurrentCncValue: ICurrentCncValue
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IMachineModule m_machineModule;
    IField m_field;
    DateTime m_dateTime = DateTime.UtcNow;
    string m_string = null;
    int? m_int = null;
    double? m_double = null;
    #endregion // Members

    static readonly ILog staticLog = LogManager.GetLogger (typeof (CurrentCncValue).FullName);
    ILog log = LogManager.GetLogger(typeof (CurrentCncValue).FullName);

    #region Getters / Setters
    /// <summary>
    /// Id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Associated machine module
    /// </summary>
    public virtual IMachineModule MachineModule {
      get { return m_machineModule; }
      protected set
      {
        m_machineModule = value;
        log = LogManager.GetLogger(string.Format ("{0}.{1}.{2}",
                                                  this.GetType ().FullName,
                                                  value.MonitoredMachine.Id, value.Id));
      }
    }
    
    /// <summary>
    /// Associated field
    /// </summary>
    public virtual IField Field {
      get { return m_field; }
    }
    
    /// <summary>
    /// UTC date/time stamp
    /// </summary>
    public virtual DateTime DateTime {
      get { return m_dateTime; }
      set
      {
        switch (value.Kind) {
          case DateTimeKind.Unspecified:
            log.WarnFormat ("DateTime.set: " +
                            "unspecified DateTimeKind => suppose it is a universal time");
            m_dateTime = new DateTime (value.Ticks, DateTimeKind.Utc);
            break;
          case DateTimeKind.Utc:
            m_dateTime = value;
            break;
          case DateTimeKind.Local:
            m_dateTime = value.ToUniversalTime ();
            break;
          default:
            throw new Exception("Invalid value for DateTimeKind");
        }
      }
    }
    
    /// <summary>
    /// String value in case the corresponding Field refers to a String
    /// </summary>
    public virtual string String {
      get { return m_string; }
      set
      {
        m_string = value;
        m_int = null;
        m_double = null;
      }
    }
    
    /// <summary>
    /// Int value in case the corresponding Field refers to an Int32
    /// </summary>
    public virtual Nullable<int> Int {
      get { return m_int; }
      set
      {
        m_int = value;
        m_string = value.ToString ();
        m_double = value;
      }
    }
    
    /// <summary>
    /// Double or average value in case the corresponding Field refers to a Double
    /// </summary>
    public virtual Nullable<double> Double {
      get { return m_double; }
      set
      {
        m_double = value;
        if (value.HasValue) {
          m_int = (int) value.Value;
        }
        else {
          m_int = null;
        }
        m_string = value.ToString ();
      }
    }
    
    /// <summary>
    /// String, Int or Double value according to the Type property of the Field
    /// </summary>
    public virtual object Value {
      get
      {
        switch (this.Field.Type) {
          case FieldType.Boolean:
            return (this.Int != 0);
          case FieldType.String:
            return this.String;
          case FieldType.Int32:
            return this.Int;
          case FieldType.Double:
            return this.Double;
          default:
            log.ErrorFormat ("Value.get: " +
                             "unknown field type {0}",
                             this.Field.Type);
            throw new Exception ("Unknown field type");
        }
      }
      set
      {
        try {
          switch (this.Field.Type) {
            case FieldType.Boolean:
              m_string = (Convert.ToBoolean (value)).ToString ();
              m_int = (Convert.ToBoolean (value))?1:0;
              m_double = (Convert.ToBoolean (value))?1.0:0.0;
              return;
            case FieldType.String:
              this.String = value.ToString ();
              return;
            case FieldType.Int32:
              this.Int = Convert.ToInt32 (value);
              return;
            case FieldType.Double:
              this.Double = Convert.ToDouble (value);
              return;
          }
        }
        catch (Exception ex) {
          log.ErrorFormat ("Value.set: " +
                           "error {0}",
                           ex);
          throw new InvalidCastException ("The value could not be converted to the right type", ex);
        }
      }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected CurrentCncValue ()
    {  }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    internal protected CurrentCncValue (IMachineModule machineModule, IField field)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);
      
      this.m_machineModule = machineModule;
      log = LogManager.GetLogger(string.Format ("{0}.{1}.{2}",
                                                this.GetType ().FullName,
                                                machineModule.MonitoredMachine.Id, machineModule.Id));

      if (null == field) {
        log.Fatal ("CurrentCncValue: " +
                   "null field argument");
        throw new ArgumentNullException ("field");
      }
      m_field = field;
    }
    #endregion // Constructors

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[CurrentCncValue {this.Id} {this.MachineModule?.ToStringIfInitialized ()} {this.Field?.ToStringIfInitialized ()}]";
      }
      else {
        return $"[CurrentCncValue {this.Id}]";
      }
    }
  }
}
