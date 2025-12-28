// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table MachineObservationState
  /// 
  /// This table lists the different machine observation states:
  /// <item>attended</item>
  /// <item>unattended</item>
  /// <item>on-site</item>
  /// <item>on-call</item>
  /// <item>machine OFF</item>
  /// </summary>
  [Serializable]
  public class MachineObservationState: DataWithTranslation, IMachineObservationState, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    bool m_userRequired;
    bool m_shiftRequired;
    bool? m_onSite;
    IMachineObservationState m_siteAttendanceChange;
    LinkDirection m_linkOperationDirection = LinkDirection.None;
    bool m_isProduction = false;
    bool m_isSetup = false;
    double? m_laborCost = null;
    IProductionState m_productionState = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineObservationState).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    internal protected MachineObservationState ()
    { }
    
    /// <summary>
    /// Constructor for default values
    /// </summary>
    /// <param name="id"></param>
    /// <param name="translationKey"></param>
    /// <param name="userRequired"></param>
    internal protected MachineObservationState (int id, string translationKey, bool userRequired)
    {
      m_id = id;
      this.TranslationKey = translationKey;
      m_userRequired = userRequired;
    }
    #endregion // Constructors
    
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
    /// MachineObservationState ID
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

    /// <summary>
    /// Is a user required for this observation state ?
    /// </summary>
    [XmlAttribute("UserRequired")]
    public virtual bool UserRequired {
      get { return m_userRequired; }
      set { m_userRequired = value; }
    }
    
    /// <summary>
    /// Is a shift required for this observation state ?
    /// </summary>
    [XmlAttribute("ShiftRequired")]
    public virtual bool ShiftRequired {
      get { return m_shiftRequired; }
      set { m_shiftRequired = value; }
    }
    
    /// <summary>
    /// Does this Machine Observation State mean the associated user
    /// is on site ?
    /// 
    /// null in case of not applicable
    /// </summary>
    [XmlIgnore]
    public virtual bool? OnSite {
      get { return m_onSite; }
      set { m_onSite = value; }
    }
    
    /// <summary>
    /// use for Xml serialization of Onsite
    /// </summary>
    [XmlAttribute("OnSite")]
    public virtual bool XmlSerializationOnSite {
      get { return m_onSite.Value; }
      set { m_onSite = value; }
    }
    
    /// <summary>
    /// used to serialize OnSite only when not null
    /// </summary>
    public virtual bool XmlSerializationOnSiteSpecified{ get { return m_onSite.HasValue; } }
    
    /// <summary>
    /// In which new MachineObservationState should this MachineObservationState
    /// be changed in case the site attendance change of the associated user
    /// changes ?
    /// </summary>
    [XmlIgnore]
    public virtual IMachineObservationState SiteAttendanceChange {
      get { return m_siteAttendanceChange; }
      set { m_siteAttendanceChange = value; }
    }

    /// <summary>
    /// Does this machine observation state imply an operation should be automatically set before or after it
    /// </summary>
    [XmlIgnore]
    public virtual LinkDirection LinkOperationDirection {
      get { return m_linkOperationDirection; }
      set { m_linkOperationDirection = value; }
    }
    
    /// <summary>
    /// Does it correspond to a production time ?
    /// </summary>
    [XmlAttribute("IsProduction")]
    public virtual bool IsProduction {
      get { return m_isProduction; }
      set { m_isProduction = value; }
    }

    /// <summary>
    /// Does it correspond to a setup time ?
    /// </summary>
    [XmlAttribute("IsSetup")]
    public virtual bool IsSetup {
      get { return m_isSetup; }
      set { m_isSetup = value; }
    }

    /// <summary>
    /// Labor cost associated with this machine observation state
    /// </summary>
    [XmlIgnore]
    public virtual double? LaborCost {
      get { return m_laborCost; }
      set { m_laborCost = value; }
    }

    /// <summary>
    /// use for Xml serialization of LaborCost
    /// </summary>
    [XmlAttribute ("LaborCost")]
    public virtual double XmlSerializationLaborCost
    {
      get { return m_laborCost.Value; }
      set { m_laborCost = value; }
    }

    /// <summary>
    /// used to serialize LaborCost only when not null
    /// </summary>
    public virtual bool XmlSerializationLaborCostSpecified => m_laborCost.HasValue;

    /// <summary>
    /// Associated production state (nullable)
    /// </summary>
    [XmlIgnore]
    public virtual IProductionState ProductionState {
      get { return m_productionState; }
      set { m_productionState = value; }
    }

    /// <summary>
    /// Text to use in a selection dialog
    /// </summary>
    [XmlIgnore]
    public virtual string SelectionText {
      get { return string.Format ("{0}: {1}{2}",
                                  this.Id, this.Name, this.TranslationKey); }
    }
    #endregion // Getters / Setters
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      // Nothing to do here for the moment
    }

    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals(object obj)
    {
      if (object.ReferenceEquals(this,obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      MachineObservationState other = obj as MachineObservationState;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (other.Id == this.Id);
      }
      return false;
    }
    
    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode()
    {
      if (0 != Id) {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * Id.GetHashCode();
        }
        return hashCode;
      }
      else {
        return base.GetHashCode ();
      }
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[MachineObservationState {this.Id} {this.TranslationKey ?? this.Name}]";
      }
      else {
        return $"[MachineObservationState {this.Id}]";
      }
    }
  }
}
