// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Collections;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table MachineMode
  /// 
  /// List of possible Machine Modes (or CNC Modes) (Auto, Jog, Manual, MDI, ...)
  /// </summary>
  [Serializable]
  public class MachineMode: DataWithTranslation, IMachineMode, IDataWithId
  {
    static readonly string DEFAULT_MACHINE_MODE_COLOR = "#808080"; // Grey

    #region Members
    int m_id = 0;
    int m_version = 0;
    bool? m_running;
    bool? m_auto;
    bool? m_manual;
    bool m_autoSequence = false;
    string m_color = DEFAULT_MACHINE_MODE_COLOR;
    MachineModeCategoryId m_machineModeCategory = MachineModeCategoryId.Inactive;
    IMachineMode m_parent = null;
    double? m_machineCost = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineMode).FullName);

    #region Getters / Setters
    /// <summary>
    /// MachineMode ID
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
    /// Indicates if the machine is considered running in this mode
    /// </summary>
    [XmlIgnore]
    public virtual bool? Running {
      get { return m_running; }
      set { m_running = value; }
    }
    
    /// <summary>
    /// Running attribute for Xml serialization
    /// </summary>
    [XmlAttribute("Running")]
    public virtual bool XmlSerializationRunning {
      get { return this.Running.Value; }
      set { this.Running = value; }
    }
    
    /// <summary>
    /// used to serialize Running only when not null
    /// </summary>
    public virtual bool XmlSerializationRunningSpecified{ get { return this.Running.HasValue; } }
    
    /// <summary>
    /// If not null, precise if the machine is running automatically
    /// </summary>
    [XmlIgnore]
    public virtual bool? Auto {
      get { return m_auto; }
      set { m_auto = value; }
    }
    
    /// <summary>
    /// Auto attribute for Xml serialization
    /// </summary>
    [XmlAttribute("Auto")]
    public virtual bool XmlSerializationAuto {
      get { return this.Auto.Value; }
      set { this.Auto = value; }
    }
    
    /// <summary>
    /// used to serialize Auto only when not null
    /// </summary>
    public virtual bool XmlSerializationAutoSpecified{ get { return this.Auto.HasValue; } }
    
    /// <summary>
    /// If not null, precise if the machine is running manually
    /// </summary>
    [XmlIgnore]
    public virtual bool? Manual {
      get { return m_manual; }
      set { m_manual = value; }
    }
    
    /// <summary>
    /// Manual attribute for Xml serialization
    /// </summary>
    [XmlAttribute("Manual")]
    public virtual bool XmlSerializationManual {
      get { return this.Manual.Value; }
      set { this.Manual = value; }
    }
    
    /// <summary>
    /// used to serialize Manual only when not null
    /// </summary>
    public virtual bool XmlSerializationManualSpecified{ get { return this.Manual.HasValue; } }
    
    /// <summary>
    /// Precise whether a sequence should be associated to the activity period
    /// in case it is detected by the CNC
    /// </summary>
    [XmlAttribute("AutoSequence")]
    public virtual bool AutoSequence {
      get { return m_autoSequence; }
      set { m_autoSequence = value; }
    }
    
    /// <summary>
    /// Color that is associated to the machine mode
    /// </summary>
    [XmlIgnore]
    public virtual string Color {
      get { return m_color; }
      set { m_color = value; }
    }

    /// <summary>
    /// Machine mode category to the machine mode
    /// </summary>
    [XmlIgnore]
    public virtual MachineModeCategoryId MachineModeCategory {
      get { return m_machineModeCategory; }
      set { m_machineModeCategory = value; }
    }
    
    /// <summary>
    /// Parent
    /// 
    /// Nullable
    /// </summary>
    [XmlIgnore]
    public virtual IMachineMode Parent {
      get { return m_parent; }
      set { m_parent = value; }
    }

    /// <summary>
    /// Machine cost associated with this machine mode
    /// 
    /// Nullable
    /// </summary>
    [XmlIgnore]
    public virtual double? MachineCost {
      get { return m_machineCost; }
      set { m_machineCost = value; }
    }

    /// <summary>
    /// MachineCost attribute for Xml serialization
    /// </summary>
    [XmlAttribute("MachineCost")]
    public virtual double XmlSerializationMachineCost {
      get { return this.MachineCost.Value; }
      set { this.MachineCost = value; }
    }

    /// <summary>
    /// used to serialize MachineCost only when not null
    /// </summary>
    public virtual bool XmlSerializationMachineCostSpecified{ get { return this.MachineCost.HasValue; } }

    /// <summary>
    /// Text to use in a selection dialog
    /// </summary>
    [XmlIgnore]
    public virtual string SelectionText {
      get { return string.Format ("{0}: {1}{2}",
                                  this.Id, this.Name, this.TranslationKey); }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    internal protected MachineMode ()
    { }
    
    /// <summary>
    /// Constructor for default values
    /// </summary>
    /// <param name="id"></param>
    /// <param name="translationKey"></param>
    /// <param name="running"></param>
    /// <param name="category"></param>
    /// <param name="parent"></param>
    internal protected MachineMode (int id, string translationKey, bool? running, MachineModeCategoryId category,
                                    IMachineMode parent)
    {
      m_id = id;
      this.TranslationKey = translationKey;
      m_running = running;
      m_machineModeCategory = category;
      m_parent = parent;
    }
    #endregion // Constructors
    
    /// <summary>
    /// Check if it is the descendant of the specified machine mode
    /// </summary>
    /// <param name="ancestor">not null</param>
    /// <returns></returns>
    public virtual bool IsDescendantOrSelfOf (IMachineMode ancestor)
    {
      if (ancestor is null) {
        log.Debug ("IsDescendantOrSelf: null => return false");
        return false;
      }
      if (this.Equals (ancestor)) { // self
        log.Debug ("IsDescendantOrSelf: self => return true");
        return true;
      }
      if (this.Parent is null) {
        log.Debug ("IsDescendantOrSelf: no parent => return false");
        return false;
      }
      else { // null != this.Parent
        if (log.IsDebugEnabled) {
          log.Debug ($"IsDescendantOrSelf: run recursively with {this.Parent?.Id} {ancestor?.Id}");
        }
        return this.Parent.IsDescendantOrSelfOf (ancestor);
      }
    }
    
    /// <summary>
    /// Get a common ancestor
    /// 
    /// Use a recursive method
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual IMachineMode GetCommonAncestor (IMachineMode other)
    {
      if (null == other) {
        return null;
      }
      if (object.Equals (this, other)) {
        return this;
      }
      if (null != this.Parent) {
        IMachineMode common = this.Parent.GetCommonAncestor (other);
        if (null != common) {
          return common;
        }
      }
      if (null != other.Parent) {
        IMachineMode common = this.GetCommonAncestor (other.Parent);
        if (null != common) {
          return common;
        }
      }
      return null;
    }

    #region Methods
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals(IMachineMode other)
    {
      return this.Equals ((object) other);
    }
    #endregion // Methods
    
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
      MachineMode other = obj as MachineMode;
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
        return $"[MachineMode {this.Id} {this.TranslationKey ?? this.Name}]";
      }
      else {
        return $"[MachineMode {this.Id}]";
      }
    }
  }
}
