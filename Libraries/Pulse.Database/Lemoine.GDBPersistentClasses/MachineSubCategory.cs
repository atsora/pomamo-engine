// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using Lemoine.Collections;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table MachineSubCategory
  /// </summary>
  [Serializable]
  public class MachineSubCategory
    : DataWithDisplayFunction
    , IMachineSubCategory, IVersionable
    , IComparable, IDisplayPriorityCodeNameComparerItem
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name;
    string m_code;
    string m_externalCode;
    int? m_displayPriority = null;
    ICollection<IMachine> m_machines = new InitialNullIdSet<IMachine, int> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineSubCategory).FullName);

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
    /// MachineSubCategory Id
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// MachineSubCategory Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// MachineSubCategory name
    /// </summary>
    [XmlAttribute("Name")]
    public virtual string Name
    {
      get { return this.m_name; }
      set { this.m_name = value; }
    }

    /// <summary>
    /// Code used in some companies to identify a machineSubCategory
    /// </summary>
    [XmlAttribute("Code")]
    public virtual string Code {
      get { return m_code; }
      set { m_code = value; }
    }
    
    /// <summary>
    /// MachineSubCategory external code
    /// 
    /// It may help synchronizing our data with an external database
    /// </summary>
    [XmlAttribute("ExternalCode")]
    public virtual string ExternalCode {
      get { return m_externalCode; }
      set { m_externalCode = value; }
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
    /// Text to use in a selection dialog
    /// </summary>
    [XmlIgnore]
    public virtual string SelectionText {
      get { return string.Format ("{0}: {1}",
                                  this.Id, this.Name); }
    }
    
    /// <summary>
    /// Set of machines sharing this subcategory
    /// </summary>
    [XmlIgnore]
    public virtual ICollection<IMachine> Machines {
      get { return m_machines; }
    }
    #endregion // Getters / Setters

    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals(IMachineSubCategory other)
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
      if (object.ReferenceEquals(this,obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      MachineSubCategory other = obj as MachineSubCategory;
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

      var other = obj as IMachineSubCategory;
      if (other != null) {
        return new DisplayPriorityCodeNameComparer<IMachineSubCategory, MachineSubCategory> ()
          .Compare (this, other);
      }
      else {
        throw new ArgumentException ("other is not a MachineSubCategory", "obj");
      }
    }
    #endregion // IComparable implementation
    #region IComparable<IMachineSubCategory> implementation
    /// <summary>
    /// <see cref="IComparable{T}"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo (IMachineSubCategory other)
    {
      if (other is null) {
        return int.MinValue;
      }
      return new DisplayPriorityCodeNameComparer<IMachineSubCategory, MachineSubCategory> ()
        .Compare (this, other);
    }
    /// <summary>
    /// <see cref="IComparer{T}"/>
    /// </summary>
    public class DisplayComparer : IComparer<IMachineSubCategory>
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
      int IComparer<IMachineSubCategory>.Compare (IMachineSubCategory x, IMachineSubCategory y)
      {
        return (new DisplayPriorityCodeNameComparer<IMachineSubCategory, MachineSubCategory> ())
          .Compare (x, y);
      }
    }
    #endregion // IComparable<IMachineSubCategory> implementation
  }
}
