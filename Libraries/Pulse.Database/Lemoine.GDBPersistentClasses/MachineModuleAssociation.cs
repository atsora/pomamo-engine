// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Abstract base class for all the machine module association modification tables
  /// </summary>
  [Serializable,
   XmlInclude(typeof(IsoFileMachineModuleAssociation))]
  public abstract class MachineModuleAssociation : MachineAssociation, IMachineModuleAssociation
  {
    #region Members
    IMachineModule m_machineModule;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Associated machine
    /// </summary>
    [XmlIgnore]
    public override IMachine Machine
    {
      get { return m_machineModule.MonitoredMachine; }
    }
    
    /// <summary>
    /// Reference to the MachineModule persistent class
    /// It can't be null
    /// </summary>
    [XmlIgnore]
    public IMachineModule MachineModule
    {
      get { return m_machineModule; }
      set
      {
        if (null == value)
        {
          log.Fatal ("MachineModuleAssociation: " +
                     "MachineModule can't be null");
          throw new ArgumentNullException ();
        }
        else
        {
          m_machineModule = value;
          log = LogManager.GetLogger(string.Format ("{0}.{1}.{2}",
                                                    this.GetType ().FullName,
                                                    value.MonitoredMachine.Id,
                                                    value.Id));
        }
      }
    }
    
    /// <summary>
    /// Reference to the MachineModule persistent class for Xml Serialization
    /// </summary>
    [XmlElement("MachineModule")]
    public virtual MachineModule XmlSerializationMachineModule {
      get { return m_machineModule as MachineModule; }
      set { m_machineModule = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    protected MachineModuleAssociation ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    protected MachineModuleAssociation (IMachineModule machineModule, UtcDateTimeRange range)
      : base (machineModule.MonitoredMachine, range)
    {
      this.MachineModule = machineModule;
    }

    /// <summary>
    /// Constructor for abstract modifications, that are kept transient
    /// 
    /// You may add a mainModification reference to be used in the analysis logs
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="range"></param>
    /// <param name="mainModification"></param>
    internal protected MachineModuleAssociation (IMachineModule machineModule, UtcDateTimeRange range, IModification mainModification)
      : base (machineModule.MonitoredMachine, range, mainModification)
    {
      this.MachineModule = machineModule;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get the impacted slots without considering any pre-fetched slot
    /// </summary>
    /// <param name="slotDAO">DAO to use to update the slot</param>
    /// <param name="pastOnly">Apply the association on the existing past slots only</param>
    public override IList<I> GetImpactedSlots<TSlot, I, TSlotDAO> (TSlotDAO slotDAO,
                                                                   bool pastOnly)
    {
      bool leftMerge = !this.Option.HasValue
        || !this.Option.Value.HasFlag (AssociationOption.NoLeftMerge);
      bool rightMerge = !this.Option.HasValue
        || !this.Option.Value.HasFlag (AssociationOption.NoRightMerge);
      
      IList<I> impactedSlots = slotDAO
        .GetImpactedMachineModuleSlotsForAnalysis (this.MachineModule,
                                                   this.Range,
                                                   this.DateTime,
                                                   pastOnly,
                                                   leftMerge,
                                                   rightMerge);
      return impactedSlots;
    }
    
    /// <summary>
    /// Get the impacted activity analysis
    /// so that the activity analysis makes a pause
    /// </summary>
    /// <returns></returns>
    public override IList<IMachine> GetImpactedActivityAnalysis ()
    {
      IList<IMachine> list = new List<IMachine> ();
      list.Add (this.MachineModule.MonitoredMachine);
      return list;
    }
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      string modificationType = this.ModificationType;
      NHibernateHelper.Unproxy<IMachineModule> (ref m_machineModule);
    }
    #endregion // Methods
  }
}
