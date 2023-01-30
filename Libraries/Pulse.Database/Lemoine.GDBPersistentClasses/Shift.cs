// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table Shift
  /// </summary>
  [Serializable]
  public class Shift
    : DataWithDisplayFunction
    , IShift
    , IVersionable
    , IComparable, IDisplayPriorityCodeNameComparerItem
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name; // Can't be empty but can be null
    string m_code;
    string m_externalCode;
    string m_color = "#808080";
    int? m_displayPriority;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (Shift).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] { "Id", "ExternalCode", "Code", "Name" }; }
    }

    /// <summary>
    /// Id
    /// </summary>
    [XmlAttribute ("Id")]
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
    /// Name
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    [XmlAttribute ("Name")]
    public virtual string Name
    {
      get { return m_name; }
      set { m_name = value; }
    }

    /// <summary>
    /// Code of the shift
    /// 
    /// Note an empty code is converted to null
    /// </summary>
    [XmlAttribute ("Code")]
    public virtual string Code
    {
      get { return m_code; }
      set { m_code = value; }
    }

    /// <summary>
    /// External Code of the reason
    /// 
    /// Note an empty external code is converted to null
    /// </summary>
    [XmlAttribute ("ExternalCode")]
    public virtual string ExternalCode
    {
      get { return m_externalCode; }
      set { m_externalCode = value; }
    }

    /// <summary>
    /// Color that is associated to the machine mode
    /// 
    /// Not null
    /// </summary>
    [XmlIgnore]
    public virtual string Color
    {
      get { return m_color; }
      set { m_color = value; }
    }

    /// <summary>
    /// Priority to use to display it in the reports or in the applications
    /// </summary>
    [XmlIgnore]
    public virtual int? DisplayPriority
    {
      get { return m_displayPriority; }
      set { m_displayPriority = value; }
    }

    /// <summary>
    /// Text to use in a selection dialog
    /// </summary>
    [XmlIgnore]
    public virtual string SelectionText
    {
      get {
        if (string.IsNullOrEmpty (this.Code)) {
          return string.Format ("{0}: {1}",
                                this.Id, this.Name);
        }
        else {
          if (string.IsNullOrEmpty (this.Name)) {
            return string.Format ("{0}: {1}",
                                  this.Id, this.Code);
          }
          else {
            return string.Format ("{0}: {1}/{2}",
                                  this.Id, this.Code, this.Name);
          }
        }
      }
    }
    #endregion // Getters / Setters

    #region Members
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (IShift other)
    {
      return this.Equals ((object)other);
    }

    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      Shift other = obj as Shift;
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
    public override int GetHashCode ()
    {
      if (0 != Id) {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * Id.GetHashCode ();
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
        return $"[Shift {this.Id} {this.Code}/{this.Name}]";
      }
      else {
        return $"[Shift {this.Id}]";
      }
    }
    #endregion // Members

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      // Nothing to do here for the moment
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

      var other = obj as IShift;
      if (other != null) {
        return new DisplayPriorityCodeNameComparer<IShift, Shift> ()
          .Compare (this, other);
      }
      else {
        throw new ArgumentException ("other is not a Shift", "obj");
      }
    }
    #endregion // IComparable implementation
    #region IComparable<IShift> implementation
    /// <summary>
    /// <see cref="IComparable{T}"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo (IShift other)
    {
      if (other is null) {
        return int.MinValue;
      }
      return new DisplayPriorityCodeNameComparer<IShift, Shift> ()
        .Compare (this, other);
    }
    /// <summary>
    /// <see cref="IComparer{T}"/>
    /// </summary>
    public class DisplayComparer : IComparer<IShift>
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
      int IComparer<IShift>.Compare (IShift x, IShift y)
      {
        return (new DisplayPriorityCodeNameComparer<IShift, Shift> ())
          .Compare (x, y);
      }
    }
    #endregion // IComparable<IShift> implementation
  }
}
