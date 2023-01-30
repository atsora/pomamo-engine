// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ComponentIntermediateWorkPiece
  /// 
  /// This lists the different association possibilities
  /// between a Component and an IntermediateWorkPiece.
  /// </summary>
  [Serializable]
  public class ComponentIntermediateWorkPiece : BaseData, IComponentIntermediateWorkPiece
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IComponent m_component = null;
    IIntermediateWorkPiece m_intermediateWorkPiece = null;
    string m_code;
    int? m_order;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (IComponentIntermediateWorkPiece).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected ComponentIntermediateWorkPiece ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="component"></param>
    /// <param name="intermediateWorkPiece"></param>
    public ComponentIntermediateWorkPiece (IComponent component,
                                           IIntermediateWorkPiece intermediateWorkPiece)
    {
      m_component = component;
      m_intermediateWorkPiece = intermediateWorkPiece;
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] { "Component", "Order", "Code", "IntermediateWorkPiece" }; }
    }

    /// <summary>
    /// ComponentIntermediateWorkPiece ID
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
    /// Reference to the related component
    /// 
    /// Be careful when set is used ! This is part of a secondary key
    /// </summary>
    [XmlIgnore]
    public virtual IComponent Component
    {
      get { return m_component; }
      set { m_component = value; }
    }

    /// <summary>
    /// Reference to the related component for Xml Serialization
    /// 
    /// Be careful when set is used ! This is part of a secondary key
    /// </summary>
    [XmlElement ("Component")]
    public virtual Component XmlSerializationComponent
    {
      get { return this.Component as Component; }
      set { this.Component = value; }
    }

    /// <summary>
    /// Reference to the related intermediate work piece
    /// 
    /// Be careful when set is used ! This is part of a secondary key
    /// </summary>
    [XmlIgnore]
    public virtual IIntermediateWorkPiece IntermediateWorkPiece
    {
      get { return m_intermediateWorkPiece; }
      set { m_intermediateWorkPiece = value; }
    }

    /// <summary>
    /// Reference to the related intermediate work piece for Xml Serialization
    /// 
    /// Be careful when set is used ! This is part of a secondary key
    /// </summary>
    [XmlElement ("IntermediateWorkPiece")]
    public virtual IntermediateWorkPiece XmlSerializationIntermediateWorkPiece
    {
      get { return this.IntermediateWorkPiece as IntermediateWorkPiece; }
      set { this.IntermediateWorkPiece = value; }
    }

    /// <summary>
    /// For a given component, code that is associated to an intermediate work piece
    /// </summary>
    [XmlAttribute ("Code")]
    public virtual string Code
    {
      get { return m_code; }
      set { m_code = value; }
    }

    /// <summary>
    /// For a given component, order in which the different intermediate work pieces must be made
    /// </summary>
    [XmlIgnore]
    public virtual int? Order
    {
      get { return m_order; }
      set { m_order = value; }
    }

    /// <summary>
    /// Order as string (for serialization)
    /// 
    /// Takes the value null in case Order is null
    /// </summary>
    [XmlAttribute ("Order")]
    public virtual string OrderAsString
    {
      get {
        return (this.Order.HasValue) ? this.Order.Value.ToString () : null;
      }
      set {
        this.Order =
          string.IsNullOrEmpty (value)
          ? default (int?)
          : int.Parse (value);
      }
    }
    #endregion // Getters / Setters

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[ComponentIntermediateWorkPiece {this.Id} {this.Component?.ToStringIfInitialized ()} {this.IntermediateWorkPiece?.ToStringIfInitialized ()} Order={this.Order} Code={this.Code}]";
      }
      else {
        return $"[ComponentIntermediateWorkPiece {this.Id}]";
      }
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
      IComponentIntermediateWorkPiece other = obj as ComponentIntermediateWorkPiece;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (other.Id == this.Id);
      }
      return false;
    }

    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="other">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public virtual bool Equals (IComponentIntermediateWorkPiece other)
    {
      return this.Equals ((object)other);
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
  }
}
