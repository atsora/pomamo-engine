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
  /// Persistent class of table GlobalModification
  /// </summary>
  [Serializable,
   XmlInclude(typeof(ComponentIntermediateWorkPieceUpdate)),
   XmlInclude(typeof(IntermediateWorkPieceOperationUpdate)),
   XmlInclude(typeof(PeriodAssociation)),
   XmlInclude(typeof(ProjectComponentUpdate)),
   XmlInclude(typeof(SerialNumberModification)),
   XmlInclude(typeof(UserAttendance)),
   XmlInclude(typeof(WorkOrderProjectUpdate))]
  public abstract class GlobalModification: Modification, IGlobalModification
  {
    #region Members
    /// <summary>
    /// Associated revision
    /// 
    /// Nullable
    /// </summary>
    protected IRevision m_revision;
    /// <summary>
    /// Associated global parent (if any)
    /// 
    /// Nullable
    /// </summary>
    protected IGlobalModification m_parentGlobal = null;
    /// <summary>
    /// Associated machine parent (if any)
    /// 
    /// Nullable
    /// </summary>
    protected IMachineModification m_parentMachine = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (GlobalModification).FullName);

    #region Getters / Setters
    /// <summary>
    /// Associated revision
    /// 
    /// Note: if the modification is not persistent yet and the revision is, you may get problems
    /// </summary>
    [XmlIgnore]
    public override IRevision Revision {
      get { return m_revision; }
      set
      {
        if (object.Equals (m_revision, value)) {
          // Nothing to do
          return;
        }
        // Remote the modification from the previous revision
        m_revision?.GlobalModifications.Remove (this);
        m_revision = value;
        // Add the modification to the new revision
        m_revision?.GlobalModifications.Add (this);
      }
    }

    /// <summary>
    /// Parent global modification when applicable
    /// </summary>
    [XmlIgnore]
    public override IGlobalModification ParentGlobal {
      get { return m_parentGlobal; }
      set
      {
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
    public override IMachineModification ParentMachine {
      get { return m_parentMachine; }
      set
      {
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
    protected GlobalModification ()
    {
    }

    /// <summary>
    /// Constructor for abstract modifications, that are kept transient
    /// 
    /// You may add a mainModification reference to be used in the analysis logs
    /// </summary>
    /// <param name="mainModification"></param>
    protected GlobalModification (IModification mainModification)
      : base (mainModification)
    {
    }
    #endregion // Constructors


    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    /// <returns></returns>
    public override string GetTransactionNameSuffix ()
    {
      return ".Global." + this.Id;
    }
  }
}
