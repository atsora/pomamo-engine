// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table MachineCellUpdate
  /// 
  /// This table tracks the modifications that are made
  /// in the relations between a Machine and a Cell
  /// in table Machine.
  /// 
  /// It is necessary to allow the Analyzer service to update correctly
  /// all the Analysis tables.
  /// </summary>
  [Serializable]
  public class MachineCellUpdate: MachineModification, IMachineCellUpdate
  {
    #region Members
    ICell m_oldCell;
    ICell m_newCell;
    #endregion // Members

    #region constructor
    /// <summary>
    /// Default Constructor
    /// </summary>
    protected MachineCellUpdate ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="oldCell"></param>
    /// <param name="newCell"></param>
    public MachineCellUpdate (IMachine machine, ICell oldCell, ICell newCell)
      : base (machine)
    {
      this.m_oldCell = oldCell;
      this.m_newCell = newCell;
    }
    #endregion

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "MachineCellUpdate"; }
    }

    /// <summary>
    /// Old cell
    /// 
    /// null in case a new machine / cell relation is set
    /// </summary>
    [XmlIgnore]
    public virtual ICell OldCell {
      get { return m_oldCell; }
    }

    /// <summary>
    /// Old cell for XML serialization
    /// </summary>
    [XmlElement("OldCell")]
    public virtual Cell XmlSerializationOldCell {
      get { return this.OldCell as Cell; }
      set { m_oldCell = value; }
    }
    
    /// <summary>
    /// New cell
    /// 
    /// null in case the machine / cell relation is deleted
    /// </summary>
    [XmlIgnore]
    public virtual ICell NewCell {
      get { return m_newCell; }
    }

    /// <summary>
    /// New cell for XML serialization
    /// </summary>
    [XmlElement("NewCell")]
    public virtual Cell XmlSerializationNewCell {
      get { return this.NewCell as Cell; }
      set { m_newCell = value; }
    }
    #endregion // Getters / Setters
    
    #region Methods
    /// <summary>
    /// Make the analysis
    /// </summary>
    public override void MakeAnalysis ()
    {
      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());

      Debug.Assert (null != this.Machine);
      
      // Do nothing for the moment.
      // If one day, a ShiftChange modification references a cell, then there will be some changes
      
      // Analysis is done
      MarkAsCompleted ("");
    }
    
    /// <summary>
    /// Apply the modification while keeping it transient
    /// 
    /// It should be never called, because there is no transient modification to process.
    /// Use a persistent entity instead and MakeAnalysis.
    /// </summary>
    public override void Apply ()
    {
      Debug.Assert (false);
      log.FatalFormat ("Apply: not implemented/supported");
      throw new NotImplementedException ();
    }    
    #endregion // Methods
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      base.Unproxy ();
      NHibernateHelper.Unproxy<ICell> (ref m_oldCell);
      NHibernateHelper.Unproxy<ICell> (ref m_newCell);
    }
  }
}
