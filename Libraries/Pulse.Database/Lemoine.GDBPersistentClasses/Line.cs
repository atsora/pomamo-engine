// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Lemoine.Collections;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// A line is made of several machines which are linked to an operation
  /// The result is a production of components, to answer a workorder
  /// </summary>
  [Serializable]
  public class Line: DataWithDisplayFunction, ILine, IComparable, IEquatable<ILine>, Lemoine.Collections.IDataWithId, ISerializableModel
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name;
    string m_code;
    ICollection<IComponent> m_components = new InitialNullIdSet<IComponent, int>();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (Line).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"Id", "Code", "Name"}; }
    }
    
    /// <summary>
    /// ID
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id
    {
      get { return m_id; }
    }
    
    /// <summary>
    /// Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return m_version; }
    }
    
    /// <summary>
    /// Name of the line
    /// </summary>
    [XmlAttribute("Name")]
    public virtual string Name
    {
      get { return m_name; }
      set { m_name = value; }
    }
    
    /// <summary>
    /// Code used in some companies to identify a line
    /// </summary>
    [XmlAttribute("Code")]
    public virtual string Code {
      get { return m_code; }
      set { m_code = value; }
    }
    
    /// <summary>
    /// Components produced by the line
    /// </summary>
    [XmlIgnore]
    public virtual ICollection<IComponent> Components {
      get { return m_components; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    protected internal Line ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add a component
    /// </summary>
    /// <param name="component"></param>
    public virtual void AddComponent (IComponent component)
    {
      if (Components.Contains(component)) {
        log.InfoFormat ("AddComponent: " +
                        "Component {0} is already in {1}",
                        component, this);
      }
      else {
        Components.Add(component);
        // Maybe used later:
        //(component as Component).AddLineForInternalUse(this);
      }
    }
    
    /// <summary>
    /// Remove a component
    /// </summary>
    /// <param name="component"></param>
    public virtual void RemoveComponent(IComponent component)
    {
      if (Components.Contains(component)) {
        Components.Remove(component);
        // Maybe used later:
        //(component as Component).RemoveLineForInternalUse(this);
      }
      else {
        log.WarnFormat ("RemoveComponent: " +
                        "Component {0} is not in {1}",
                        component, this);
      }
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[Line {this.Id} Name={this.Name}]";
      }
      else {
        return $"[Line {this.Id}]";
      }
    }
    
    /// <summary>
    /// Indicates whether the current object
    /// is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals(ILine other)
    {
      return this.Equals ((object) other);
    }
    
    /// <summary>
    /// Determines whether the specified Object
    /// is equal to the current Object
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
      Line other = obj as Line;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (other.Id == this.Id);
      }
      return false;
    }

    /// <summary>
    /// Serves as a hash function for a particular type
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
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      // Nothing here
    }
    #endregion // Methods
    
    #region IComparable implementation
    /// <summary>
    /// Comparison with another object
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public virtual int CompareTo(ILine line)
    {
      return this.CompareTo(line as object);
    }
    
    /// <summary>
    /// IComparable implementation
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public virtual int CompareTo(object obj)
    {
      if (obj == null) {
        return int.MinValue;
      }

      Line other = obj as Line;
      if (other != null) {
        return (this.Code + this.Name).CompareTo(other.Code + other.Name);
      }
      else {
        throw new ArgumentException ("other is not a Line");
      }
    }
    #endregion // IComparable implementation
  }
}
