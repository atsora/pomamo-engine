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
  /// Persistent class of table MachineCompanyUpdate
  /// 
  /// This table tracks the modifications that are made
  /// in the relations between a Machine and a Company
  /// in table Machine.
  /// 
  /// It is necessary to allow the Analyzer service to update correctly
  /// all the Analysis tables.
  /// </summary>
  [Serializable]
  public class MachineCompanyUpdate: MachineModification, IMachineCompanyUpdate
  {
    #region Members
    ICompany m_oldCompany;
    ICompany m_newCompany;
    #endregion // Members

    #region constructor
    /// <summary>
    /// Default Constructor
    /// </summary>
    protected MachineCompanyUpdate ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="oldCompany"></param>
    /// <param name="newCompany"></param>
    public MachineCompanyUpdate (IMachine machine, ICompany oldCompany, ICompany newCompany)
      : base (machine)
    {
      this.m_oldCompany = oldCompany;
      this.m_newCompany = newCompany;
    }
    #endregion

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "MachineCompanyUpdate"; }
    }

    /// <summary>
    /// Old company
    /// 
    /// null in case a new machine / company relation is set
    /// </summary>
    [XmlIgnore]
    public virtual ICompany OldCompany {
      get { return m_oldCompany; }
    }
    
    /// <summary>
    /// Old company for XML serialization
    /// </summary>
    [XmlElement("OldCompany")]
    public virtual Company XmlSerializationOldCompany {
      get { return this.OldCompany as Company; }
      set { m_oldCompany = value; }
    }
    
    /// <summary>
    /// New company
    /// 
    /// null in case the machine / company relation is deleted
    /// </summary>
    [XmlIgnore]
    public virtual ICompany NewCompany {
      get { return m_newCompany; }
    }

    /// <summary>
    /// New company for XML serialization
    /// </summary>
    [XmlElement("NewCompany")]
    public virtual Company XmlSerializationNewCompany {
      get { return this.NewCompany as Company; }
      set { m_newCompany = value; }
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
      NHibernateHelper.Unproxy<ICompany> (ref m_oldCompany);
      NHibernateHelper.Unproxy<ICompany> (ref m_newCompany);
    }
  }
}
