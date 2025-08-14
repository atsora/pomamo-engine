// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ManufOrderMachineAssociation
  /// </summary>
  [Serializable]
  public class ManufacturingOrderMachineAssociation
    : MachineModification
    , IManufacturingOrderMachineAssociation
    // Note: public else it is not serializable for the alert service
  {
    UtcDateTimeRange m_range;
    IManufacturingOrder m_manufacturingOrder;
    AssociationOption? m_associationOption;
    bool m_partOfDetectionAnalysis;

    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType => "ManufacturingOrderMachineAssociation";

    /// <summary>
    /// Range
    /// </summary>
    [XmlIgnore]
    public virtual UtcDateTimeRange Range => m_range;

    /// <summary>
    /// Range for Xml serialization
    /// </summary>
    [XmlAttribute("Range")]
    public virtual string XmlSerializationRange
    {
      get
      {
        if (m_range is null) {
          log.Warn ("XmlSerializationRange.get: range null");
          return "";
        }
        else {
          return m_range.ToString (dt => dt.ToString ("yyyy-MM-dd HH:mm:ss"));
        }
      }
      set
      {
        m_range = new UtcDateTimeRange (value);
      }
    }
    
    /// <summary>
    /// Association option
    /// </summary>
    public virtual AssociationOption? Option {
      get { return m_associationOption; }
      set { m_associationOption = value; }
    }

    /// <summary>
    /// Manufacturing order to associate to a machine
    /// </summary>
    [XmlIgnore]
    public virtual IManufacturingOrder ManufacturingOrder {
      get { return m_manufacturingOrder; }
      set { m_manufacturingOrder = value; }
    }

    /// <summary>
    /// Reference to the related ManufacturingOrder for Xml Serialization
    /// </summary>
    [XmlElement("ManufacturingOrder")]
    public virtual ManufacturingOrder XmlSerializationManufacturingOrder {
      get { return this.ManufacturingOrder as ManufacturingOrder; }
      set { this.ManufacturingOrder = value; }
    }

    /// <summary>
    /// Part of the detection analysis so that any analysis problem is logged
    /// in the detectionanalysislog table
    /// </summary>
    [XmlIgnore]
    public virtual bool PartOfDetectionAnalysis => m_partOfDetectionAnalysis;
    
    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected ManufacturingOrderMachineAssociation ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <returns></returns>
    internal protected ManufacturingOrderMachineAssociation (IMachine machine, UtcDateTimeRange range)
      : base (machine)
    {
      m_range = range;
    }
    
    /// <summary>
    /// Constructor for abstract modifications, that are kept transient
    /// 
    /// You may add a mainModification reference to be used in the analysis logs
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="mainModification"></param>
    /// <param name="partOfDetectionAnalysis"></param>
    internal protected ManufacturingOrderMachineAssociation (IMachine machine, UtcDateTimeRange  range, IModification mainModification,
      bool partOfDetectionAnalysis)
      : base (machine, mainModification)
    {
      m_range = range;
      m_partOfDetectionAnalysis = partOfDetectionAnalysis;
    }
    #endregion // Constructors
    
    /// <summary>
    /// Make the analysis
    /// </summary>
    public override void MakeAnalysis ()
    {
      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());
      
      Analyze ();

      // Analysis is done
      MarkAsCompleted ("Cache/ClearDomainByMachine/ManufacturingOrderAssociation/" + this.Machine.Id + "?Broadcast=true");
    }
    
    /// <summary>
    /// Apply the modifications
    /// </summary>
    public override void Apply ()
    {
      this.Analyze ();
    }
    
    /// <summary>
    /// Insert all the slots that may correspond to this modification
    /// taking into account the auto-sequence table
    /// 
    /// Note the DateTime property of the modification must be correctly
    /// set to use this
    /// </summary>
    public virtual void Analyze ()
    {
      var association = new WorkOrderMachineAssociation (this.Machine,
                                                         this.Range,
                                                         this.MainModification ?? this,
                                                         m_partOfDetectionAnalysis);
      association.DateTime = this.DateTime;
      association.ManufacturingOrder = this.ManufacturingOrder;
      association.Option = this.Option;
      association.Caller = this;
      association.Analyze ();
    }
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      base.Unproxy ();
      NHibernateHelper.Unproxy<IManufacturingOrder> (ref m_manufacturingOrder);
    }
  }
}
