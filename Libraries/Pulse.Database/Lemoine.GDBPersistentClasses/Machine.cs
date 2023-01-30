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

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table Machine
  /// 
  /// This table replaces with the Monitored Machine
  /// and Machine Path tables the old sfkmach table.
  /// 
  /// This new table extends the concept of machine
  /// to include also the outsource resources and the not monitored machines.
  /// </summary>
  [Serializable, XmlInclude(typeof(MonitoredMachine))]
  public class Machine
    : DataWithDisplayFunction
    , IMachine, IVersionable, Lemoine.Collections.IDataWithId
    , IDisplayPriorityCodeNameComparerItem, IComparable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name;
    string m_code;
    string m_externalCode;
    IMachineMonitoringType m_monitoringType;
    int? m_displayPriority = null;
    ICompany m_company;
    IDepartment m_department;
    ICell m_cell;
    IMachineCategory m_category;
    IMachineSubCategory m_subCategory;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (Machine).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"Id", "ExternalCode", "Code", "Name"}; }
    }
    
    /// <summary>
    /// Machine ID
    /// 
    /// In case it is a monitored machine,
    /// it is the same ID than used in the Monitored Machine table.
    /// </summary>
    [XmlIgnore]
    public virtual int Id
    {
      get { return this.m_id; }
      internal protected set { m_id = value; }
    }
    
    /// <summary>
    /// ID of the machine module for XML serialization
    /// </summary>
    [XmlAttribute("Id")]
    public virtual string XmlSerializationId {
      get { return this.Id.ToString (); }
      set {
        int v;
        if (int.TryParse (value, out v)) {
          m_id = v;
        } else {
          log.ErrorFormat ("XmlSerializationId.set: " +
                           "{0} is not an integer",
                           value);
        }
      }
    }
    
    /// <summary>
    /// Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
      internal protected set { m_version = value; }
    }

    /// <summary>
    /// Name of the machining resource.
    /// 
    /// In case it is a monitored machine,
    /// it is the same than the name of the corresponding Monitored Machine
    /// </summary>
    [XmlAttribute("Name")]
    public virtual string Name
    {
      get { return this.m_name; }
      set { this.m_name = value; }
    }
    
    /// <summary>
    /// Code used in some companies to identify a machining resource
    /// </summary>
    [XmlAttribute("Code")]
    public virtual string Code {
      get { return m_code; }
      set { m_code = value; }
    }
    
    /// <summary>
    /// Machine external code
    /// 
    /// It may help synchronizing our data with an external database
    /// </summary>
    [XmlAttribute("ExternalCode")]
    public virtual string ExternalCode {
      get { return m_externalCode; }
      set { m_externalCode = value; }
    }
    
    /// <summary>
    /// Reference to the Machine Monitoring Type table
    /// </summary>
    [XmlIgnore]
    public virtual IMachineMonitoringType MonitoringType {
      get { return m_monitoringType; }
      set { m_monitoringType = value; }
    }
    
    /// <summary>
    /// Reference to the Machine Monitoring Type table
    /// for Xml Serialization
    /// </summary>
    [XmlElement("MonitoringType")]
    public virtual MachineMonitoringType XmlSerializationMonitoringType {
      get { return this.MonitoringType as MachineMonitoringType; }
      set { this.MonitoringType = value; }
    }
    
    /// <summary>
    /// Priority to use to display it in the reports or in the applications
    /// </summary>
    [XmlIgnore]
    public virtual int? DisplayPriority {
      get { return m_displayPriority; }
      set { m_displayPriority = value; }
    }
    
    /// <summary>
    /// Associated company
    /// </summary>
    [XmlIgnore]
    public virtual ICompany Company {
      get { return m_company; }
      set { m_company = value; }
    }

    /// <summary>
    /// Associated department
    /// </summary>
    [XmlIgnore]
    public virtual IDepartment Department {
      get { return m_department; }
      set { m_department = value; }
    }
    
    /// <summary>
    /// Associated cell
    /// </summary>
    [XmlIgnore]
    public virtual ICell Cell {
      get { return m_cell; }
      set { m_cell = value; }
    }
    
    /// <summary>
    /// Associated category
    /// </summary>
    [XmlIgnore]
    public virtual IMachineCategory Category {
      get { return m_category; }
      set { m_category = value; }
    }
    
    /// <summary>
    /// Associated sub-category
    /// </summary>
    [XmlIgnore]
    public virtual IMachineSubCategory SubCategory {
      get { return m_subCategory; }
      set { m_subCategory = value; }
    }

    /// <summary>
    /// Text to use in a selection dialog
    /// </summary>
    [XmlIgnore]
    public virtual string SelectionText {
      get { return string.Format ("{0}: {1}",
                                  this.Id, this.Name); }
    }
    #endregion // Getters / Setters
    
    #region Methods
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IMachineMonitoringType> (ref m_monitoringType);
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[Machine {this.Id} Name={this.Name}]";
      }
      else {
        return $"[Machine {this.Id}]";
      }
    }
    
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals(IMachine other)
    {
      return this.Equals ((object) other);
    }
    
    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals(object obj)
    {
      if (object.ReferenceEquals(this, obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      Machine other = obj as Machine;
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
    /// Is the machine monitored ?
    /// </summary>
    /// <returns></returns>
    public virtual bool IsMonitored ()
    {
      Debug.Assert (null != this.MonitoringType);
      int monitoringTypeId = this.MonitoringType.Id;
      return ((int) MachineMonitoringTypeId.Monitored == monitoringTypeId);
    }
    
    /// <summary>
    /// Is the machine obsolete?
    /// </summary>
    /// <returns></returns>
    public virtual bool IsObsolete()
    {
      Debug.Assert(this.MonitoringType != null);
      return this.MonitoringType.Id == (int)MachineMonitoringTypeId.Obsolete;
    }
    #endregion // Methods

    #region IComparable implementation
    /// <summary>
    /// <see cref="IComparable"/>
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public virtual int CompareTo (object obj)
    {
      if (obj == null) {
        return int.MinValue;
      }

      var other = obj as IMachine;
      if (other != null) {
        return new DisplayPriorityCodeNameComparer<IMachine, Machine> ()
          .Compare (this, other);
      }
      else {
        throw new ArgumentException ("other is not a Machine", "obj");
      }
    }
    #endregion // IComparable implementation
    #region IComparable<IMachine> implementation
    /// <summary>
    /// <see cref="IComparable{T}"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo (IMachine other)
    {
      if (other is null) {
        return int.MinValue;
      }
      return new DisplayPriorityCodeNameComparer<IMachine, Machine> ()
        .Compare (this, other);
    }
    /// <summary>
    /// <see cref="IComparer{T}"/>
    /// </summary>
    public class DisplayComparer : IComparer<IMachine>
    {
      /// <summary>
      /// Implementation of DisplayComparer
      /// 
      /// Use the following properties to sort the machines:
      /// <item>Display priority</item>
      /// <item>Code</item>
      /// <item>Name</item>
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      int IComparer<IMachine>.Compare (IMachine x, IMachine y)
      {
        return (new DisplayPriorityCodeNameComparer<IMachine, Machine> ())
          .Compare (x, y);
      }
    }
    #endregion // IComparable<IMachine> implementation
  }
}
