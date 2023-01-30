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
  /// Persistent class of table MachineDepartmentUpdate
  /// 
  /// This table tracks the modifications that are made
  /// in the relations between a Machine and a Department
  /// in table Machine.
  /// 
  /// It is necessary to allow the Analyzer service to update correctly
  /// all the Analysis tables.
  /// </summary>
  [Serializable]
  public class MachineDepartmentUpdate: MachineModification, IMachineDepartmentUpdate
  {
    #region Members
    IDepartment m_oldDepartment;
    IDepartment m_newDepartment;
    #endregion // Members

    #region constructor
    /// <summary>
    /// Default Constructor
    /// </summary>
    protected MachineDepartmentUpdate ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="oldDepartment"></param>
    /// <param name="newDepartment"></param>
    public MachineDepartmentUpdate (IMachine machine, IDepartment oldDepartment, IDepartment newDepartment)
      : base (machine)
    {
      this.m_oldDepartment = oldDepartment;
      this.m_newDepartment = newDepartment;
    }
    #endregion

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "MachineDepartmentUpdate"; }
    }

    /// <summary>
    /// Old department
    /// 
    /// null in case a new machine / department relation is set
    /// </summary>
    [XmlIgnore]
    public virtual IDepartment OldDepartment {
      get { return m_oldDepartment; }
    }

    /// <summary>
    /// Old department for XML serialization
    /// </summary>
    [XmlElement("OldDepartment")]
    public virtual Department XmlSerializationOldDepartment {
      get { return this.OldDepartment as Department; }
      set { m_oldDepartment = value; }
    }
    
    /// <summary>
    /// New department
    /// 
    /// null in case the machine / department relation is deleted
    /// </summary>
    [XmlIgnore]
    public virtual IDepartment NewDepartment {
      get { return m_newDepartment; }
    }

    /// <summary>
    /// New department for XML serialization
    /// </summary>
    [XmlElement("NewDepartment")]
    public virtual Department XmlSerializationNewDepartment {
      get { return this.NewDepartment as Department; }
      set { m_newDepartment = value; }
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
      
      // Nothing to do for the moment
            
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
      NHibernateHelper.Unproxy<IDepartment> (ref m_oldDepartment);
      NHibernateHelper.Unproxy<IDepartment> (ref m_newDepartment);
    }
  }
}
