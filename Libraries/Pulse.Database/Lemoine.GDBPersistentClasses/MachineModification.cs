// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;
using Lemoine.Database.Persistent;
using Lemoine.Model;

using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table MachineModification
  /// </summary>
  [Serializable,
   XmlInclude (typeof (MachineCellUpdate)),
   XmlInclude (typeof (MachineCompanyUpdate)),
   XmlInclude (typeof (MachineDepartmentUpdate)),
   XmlInclude (typeof (MachineStamp)),
   XmlInclude (typeof (LinkOperation)),
   XmlInclude (typeof (OperationInformation)),
   XmlInclude (typeof (MachineAssociation)),
   XmlInclude (typeof (NonConformanceReport)),
   XmlInclude (typeof (ProcessMachineStateTemplate)),
   XmlInclude (typeof (ProductionInformation)),
   XmlInclude (typeof (ProductionInformationShift)),
   XmlInclude (typeof (TaskMachineAssociation))]
  public abstract class MachineModification : Modification, IMachineModification
  {
    #region Members
    /// <summary>
    /// Associated machine (not null)
    /// </summary>
    protected IMachine m_machine;
    /// <summary>
    /// Associated revision (nullable)
    /// </summary>
    protected IRevision m_revision;
    /// <summary>
    /// Associated global parent (nullable)
    /// </summary>
    protected IGlobalModification m_parentGlobal = null;
    /// <summary>
    /// Associated machine parent (nullable)
    /// </summary>
    protected IMachineModification m_parentMachine = null;
    #endregion // Members

    /// <summary>
    /// Logger
    /// </summary>
    protected ILog log = LogManager.GetLogger (typeof (MachineModification).FullName);

    #region Getters / Setters
    /// <summary>
    /// Reference to the Machine (machine or outsource)
    /// 
    /// It can't be null
    /// </summary>
    [XmlIgnore]
    public virtual IMachine Machine
    {
      get { return m_machine; }
      set { SetMachine (value); }
    }

    /// <summary>
    /// Reference to the machine for the mapping
    /// </summary>
    [XmlIgnore]
    protected internal virtual IMachine ModificationMachine
    {
      get { return this.Machine; }
      set { this.Machine = value; }
    }

    /// <summary>
    /// Reference to the machine for the mapping
    /// </summary>
    [XmlIgnore]
    protected internal virtual IMachine ModificationStatusMachine
    {
      get { return this.Machine; }
      // disable once ValueParameterNotUsed
      set { }
    }

    /// <summary>
    /// Reference to the Machine (machine or outsource)
    /// for Xml Serialization
    /// 
    /// It can't be null
    /// </summary>
    [XmlElement ("Machine")]
    public virtual Machine XmlSerializationMachine
    {
      get { return this.Machine as Machine; }
      set { this.Machine = value; }
    }

    /// <summary>
    /// Associated revision
    /// 
    /// Note: if the modification is not persistent yet and the revision is, you may get problems
    /// </summary>
    [XmlIgnore]
    public override IRevision Revision
    {
      get { return m_revision; }
      set {
        if (object.Equals (m_revision, value)) {
          // Nothing to do
          return;
        }
        // Remote the modification from the previous revision
        m_revision?.MachineModifications.Remove (this);
        m_revision = value;
        // Add the modification to the new revision
        m_revision?.MachineModifications.Add (this);
      }
    }

    /// <summary>
    /// Parent global modification when applicable
    /// </summary>
    [XmlIgnore]
    public override IGlobalModification ParentGlobal
    {
      get { return m_parentGlobal; }
      set {
        if (object.Equals (m_parentGlobal, value)) {
          // nothing to do
          return;
        }
        // Remove the sub-modification from the previous parent
        if (m_parentGlobal != null) {
          GlobalModification previousParent = m_parentGlobal as GlobalModification;
          previousParent.RemoveSubModificationForInternalUse (this);
        }
        m_parentGlobal = value;
        if (m_parentGlobal != null) {
          // Add the sub-modification to the new parent
          (m_parentGlobal as GlobalModification).AddSubModificationForInternalUse (this);
        }

      }
    }

    /// <summary>
    /// Parent machine modification when applicable
    /// </summary>
    [XmlIgnore]
    public override IMachineModification ParentMachine
    {
      get { return m_parentMachine; }
      set {
        if (object.Equals (m_parentMachine, value)) {
          // nothing to do
          return;
        }
        // Remove the sub-modification from the previous parent
        if (m_parentMachine != null) {
          MachineModification previousParent = m_parentMachine as MachineModification;
          previousParent.RemoveSubModificationForInternalUse (this);
        }
        m_parentMachine = value;
        if (m_parentMachine != null) {
          // Add the sub-modification to the new parent
          (m_parentMachine as MachineModification).AddSubModificationForInternalUse (this);
        }

      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    protected MachineModification ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    protected MachineModification (IMachine machine)
    {
      SetMachine (machine);
    }

    /// <summary>
    /// Constructor for abstract modifications, that are kept transient
    /// 
    /// You may add a mainModification reference to be used in the analysis logs
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="mainModification"></param>
    protected MachineModification (IMachine machine, IModification mainModification)
      : base (mainModification)
    {
      SetMachine (machine);
    }

    void SetMachine (IMachine machine)
    {
      if (null == machine) {
        log.Fatal ($"MachineModification: Machine can't be null. StackTrace={System.Environment.StackTrace}");
        Debug.Assert (null != machine);
        throw new ArgumentNullException ("machine");
      }
      else {
        m_machine = machine;
        log = LogManager.GetLogger ($"{this.GetType ().FullName}.{machine.Id}");
      }
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get a logger
    /// </summary>
    /// <returns></returns>
    public virtual ILog GetLogger ()
    {
      return log;
    }

    /// <summary>
    /// Get the impacted activity analysis
    /// so that the activity analysis makes a pause
    /// </summary>
    /// <returns></returns>
    public override IList<IMachine> GetImpactedActivityAnalysis ()
    {
      IList<IMachine> list = new List<IMachine> ();
      list.Add (this.Machine);
      return list;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      string modificationType = this.ModificationType;
      NHibernateHelper.Unproxy<IMachine> (ref m_machine);
    }

    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    /// <returns></returns>
    public override string GetTransactionNameSuffix ()
    {
      return "." + m_machine.Id + "." + this.Id;
    }
    #endregion // Methods
  }
}
