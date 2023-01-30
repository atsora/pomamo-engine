// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml.Serialization;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of the WorkOrderStatus table
  /// </summary>
  [Serializable]
  public class WorkOrderStatus: DataWithTranslation, IWorkOrderStatus
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    #endregion

    static readonly ILog log = LogManager.GetLogger(typeof (WorkOrderStatus).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"Id", "Name", "TranslationKey"}; }
    }
    
    /// <summary>
    /// Work order status ID
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
    }

    /// <summary>
    /// Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
    }
    #endregion
    
    #region IReferenceData implementation
    /// <summary>
    /// <see cref="IReferenceData.IsUndefined" />
    /// </summary>
    /// <returns></returns>
    public virtual bool IsUndefined()
    {
      return (1 == this.Id);
    }
    #endregion // IReferenceData implementation

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      // Nothing to do here for the moment
    }
  }
}
