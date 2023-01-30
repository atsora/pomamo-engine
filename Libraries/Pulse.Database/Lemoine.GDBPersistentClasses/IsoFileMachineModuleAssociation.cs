// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Modification class to easily process and insert the coming iso file
  /// from the machine module only in the analysis tables.
  /// 
  /// This is not a persistent class, no database table is associated to it!
  /// </summary>
  [Serializable]
  public class IsoFileMachineModuleAssociation: MachineModuleAssociation, IIsoFileMachineModuleAssociation
  {
    #region Members
    IIsoFile m_isoFile;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "IsoFileMachineModuleAssociation"; }
    }

    /// <summary>
    /// Reference to the IsoFile
    /// </summary>
    [XmlIgnore]
    public virtual IIsoFile IsoFile {
      get { return m_isoFile; }
      set { m_isoFile = value; }
    }
    
    /// <summary>
    /// Reference to the IsoFile for Xml Serialization
    /// </summary>
    [XmlElement("IsoFile")]
    public virtual IsoFile XmlSerializationIsoFile {
      get { return m_isoFile as IsoFile; }
      set { m_isoFile = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    protected IsoFileMachineModuleAssociation ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    public IsoFileMachineModuleAssociation (IMachineModule machineModule, UtcDateTimeRange range)
      : base (machineModule, range)
    {
    }

    /// <summary>
    /// Constructor for abstract modifications, that are kept transient
    /// 
    /// You may add a mainModification reference to be used in the analysis logs
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <param name="mainModification"></param>
    public IsoFileMachineModuleAssociation (IMachineModule machineModule, UtcDateTimeRange range, IModification mainModification)
      : base (machineModule, range, mainModification)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Make the analysis
    /// </summary>
    public override void MakeAnalysis()
    {
      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());

      if (Bound.Compare<DateTime> (this.End, this.Begin) < 0) { // Empty period: error
        string message = string.Format ("End={0} before Begin={1}",
                                        this.End, this.Begin);
        log.ErrorFormat ("MakeAnalysis: " +
                         "{0} " +
                         "=> finish in error",
                         message);
        AddAnalysisLog(LogLevel.ERROR, message);
        MarkAsError ();
        return;
      }

      // Process it
      ProcessAssociation ();
      
      // Analysis is done
      MarkAsCompleted ("Cache/ClearDomainByMachineModule/IsoFileAssociation/" + this.MachineModule.Id + "?Broadcast=true");
    }
    
    /// <summary>
    /// Apply the modification while keeping it transient
    /// </summary>
    public override void Apply ()
    {
      ProcessAssociation ();
    }
    
    /// <summary>
    /// Process the association itself,
    /// without updating any Modification property
    /// </summary>
    void ProcessAssociation ()
    {
      IsoFileSlotDAO isoFileSlotDAO = new IsoFileSlotDAO ();
      isoFileSlotDAO.Caller = this;
      Insert<IsoFileSlot, IIsoFileSlot, IsoFileSlotDAO> (isoFileSlotDAO, false);
    }
    
    /// <summary>
    /// <see cref="PeriodAssociation.MergeDataWithOldSlot" />
    /// </summary>
    /// <param name="oldSlot"></param>
    /// <param name="range">merge period range</param>
    /// <returns></returns>
    public override TSlot MergeDataWithOldSlot<TSlot>(TSlot oldSlot,
                                                      UtcDateTimeRange range)
    {
      if (m_isoFile == null) {
        return null;
      }
      else {
        var slot = GenericMachineModuleSlot.Create (typeof (IsoFileSlot),
                                                    this.MachineModule,
                                                    new UtcDateTimeRange (Begin, this.End)) as IsoFileSlot;
        slot.Consolidated = false;
        slot.IsoFile = m_isoFile;
        return slot as TSlot;
      }
    }
    
    /// <summary>
    /// <see cref="PeriodAssociation.ConvertToSlot" />
    /// </summary>
    /// <returns></returns>
    public override TSlot ConvertToSlot<TSlot> ()
    {
      if (m_isoFile == null) {
        return null;
      }
      else {
        var slot = GenericMachineModuleSlot.Create (typeof (TSlot),
                                                    this.MachineModule,
                                                    new UtcDateTimeRange (Begin, End)) as TSlot;
        slot.Consolidated = false;

        if (slot is IIsoFileSlot) {
          // Activity detection is responsible for the creation of the slot
          var isoFileSlot = slot as IIsoFileSlot;
          isoFileSlot.IsoFile = m_isoFile;
          return slot;
        }
        else {
          throw new NotImplementedException ("Slot type not implemented");
        }
      }
    }
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      base.Unproxy ();
    }
    #endregion // Methods
  }
}
