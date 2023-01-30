// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ShiftTemplateAssociation
  /// </summary>
  [Serializable]
  public class ShiftTemplateAssociation: PeriodAssociation, IShiftTemplateAssociation
  {
    #region Members
    IShiftTemplate m_ShiftTemplate;
    bool m_force = false;
    #endregion // Members
    
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    protected ShiftTemplateAssociation ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="ShiftTemplate">not null</param>
    /// <param name="begin"></param>
    internal protected ShiftTemplateAssociation (IShiftTemplate ShiftTemplate,
                                                 DateTime begin)
      : base (begin)
    {
      this.ShiftTemplate = ShiftTemplate;
    }
    
    /// <summary>
    /// Constructor for abstract modifications, that are kept transient
    /// 
    /// You may add a mainModification reference to be used in the analysis logs
    /// </summary>
    /// <param name="ShiftTemplate">not null</param>
    /// <param name="begin"></param>
    /// <param name="mainModification"></param>
    internal protected ShiftTemplateAssociation (IShiftTemplate ShiftTemplate,
                                                 DateTime begin,
                                                 IModification mainModification)
      : base (begin, mainModification)
    {
      this.ShiftTemplate = ShiftTemplate;
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "ShiftTemplateAssociation"; }
    }

    /// <summary>
    /// Reference to the Machine State Template
    /// 
    /// Not null
    /// </summary>
    [XmlIgnore]
    public virtual IShiftTemplate ShiftTemplate {
      get { return m_ShiftTemplate; }
      set
      {
        Debug.Assert (null != value);
        if (null == value) {
          log.ErrorFormat ("ShiftTemplate.set: " +
                           "null value");
          throw new ArgumentNullException ("ShiftTemplateAssociation.ShiftTemplate");
        }
        m_ShiftTemplate = value;
      }
    }
    
    /// <summary>
    /// Reference to the Machine Observation State for Xml Serialization
    /// </summary>
    [XmlElement("ShiftTemplate")]
    public virtual ShiftTemplate XmlSerializationShiftTemplate {
      get { return this.ShiftTemplate as ShiftTemplate; }
      set { this.ShiftTemplate = value; }
    }

    /// <summary>
    /// Force re-building the template
    /// 
    /// Default is False
    /// </summary>
    [XmlIgnore]
    public virtual bool Force {
      get { return m_force; }
      set { m_force = value; }
    }
    #endregion // Getters / Setters
    
    #region PeriodAssociation implementation
    /// <summary>
    /// <see cref="PeriodAssociation.ConvertToSlot" />
    /// </summary>
    /// <returns></returns>
    public override TSlot ConvertToSlot<TSlot>()
    {
      Debug.Assert (null != this.ShiftTemplate);
      
      var slot =
        GenericRangeSlot.Create (typeof (TSlot), this.Range) as TSlot;
      slot.Consolidated = false;

      if (typeof (TSlot).Equals (typeof (ShiftTemplateSlot))) {
        var shiftTemplateSlot = slot as IShiftTemplateSlot;
        shiftTemplateSlot.ShiftTemplate = this.ShiftTemplate;
        return slot;
      }
      else if (typeof (TSlot).Equals (typeof (ShiftSlot))) {
        var shiftSlot = slot as IShiftSlot;
        Debug.Assert (null == shiftSlot.Shift);
        shiftSlot.ShiftTemplate = this.ShiftTemplate;
        shiftSlot.TemplateProcessed = false;
        return slot;
      }
      else {
        System.Diagnostics.Debug.Assert (false);
        throw new NotImplementedException ("Slot type not implemented");
      }
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
      Debug.Assert (null != oldSlot);
      
      if (oldSlot is IShiftTemplateSlot) {
        var oldShiftTemplateSlot =
          oldSlot as IShiftTemplateSlot;
        var newShiftTemplateSlot =
          (IShiftTemplateSlot) oldShiftTemplateSlot.Clone ();

        newShiftTemplateSlot.ShiftTemplate = this.ShiftTemplate;
        
        return newShiftTemplateSlot as TSlot;
      }
      if (oldSlot is IShiftSlot) {
        var oldShiftSlot =
          oldSlot as IShiftSlot;
        var newShiftSlot =
          (IShiftSlot) oldShiftSlot.Clone ();

        if (!object.Equals (oldShiftSlot.ShiftTemplate, this.ShiftTemplate)) {
          // change of ShiftTemplate
          newShiftSlot.ShiftTemplate = this.ShiftTemplate;
          newShiftSlot.Shift = null;
          newShiftSlot.TemplateProcessed = false;
        }
        else if (this.Force) { // Same ShiftTemplate, but force re-building the shifts
          newShiftSlot.Shift = null;
          newShiftSlot.TemplateProcessed = false;
        }
        return newShiftSlot as TSlot;
      }
      else {
        System.Diagnostics.Debug.Assert (false);
        log.FatalFormat ("MergeData: " +
                         "trying to merge the association with a not supported slot {0}",
                         typeof (TSlot));
        throw new ArgumentException ("Not supported machine slot");
      }
    }
    #endregion // PeriodAssociation implementation
    
    #region Modification implementation
    /// <summary>
    /// Make the analysis
    /// </summary>
    public override void MakeAnalysis ()
    {
      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());

      Debug.Assert (null != this.ShiftTemplate);
      
      // Process it
      ProcessAssociation ();
      
      // Analysis is done
      MarkAsCompleted ("Cache/ClearDomain/ShiftTemplateAssociation?Broadcast=true");
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
      {
        var shiftTemplateSlotDAO = new ShiftTemplateSlotDAO ();
        Insert<ShiftTemplateSlot, IShiftTemplateSlot, ShiftTemplateSlotDAO> (shiftTemplateSlotDAO);
      }
      SetActive ();
      {
        var shiftSlotDAO = new ShiftSlotDAO ();
        Insert<ShiftSlot, IShiftSlot, ShiftSlotDAO> (shiftSlotDAO);
      }
    }
    
    /// <summary>
    /// Check if the step process should be active or not for the specified range
    /// 
    /// By default, it is not active in the future or when the main modification is transient
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    protected override bool IsStepActive (UtcDateTimeRange range)
    {
      // Just return false for the moment because there are two steps in the analysis
      return false;
    }
    #endregion // Modification implementation
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      NHibernateHelper.Unproxy<IShiftTemplate> (ref m_ShiftTemplate);
    }
  }
}
