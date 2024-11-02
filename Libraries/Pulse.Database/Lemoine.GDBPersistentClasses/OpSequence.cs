// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Globalization;

using Lemoine.Collections;
using Lemoine.Model;
using Lemoine.Core.Log;
using NHibernate.Type;
using System.ComponentModel;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table Sequence
  /// 
  /// It represents a specific machining sequence inside an operation.
  /// A sequence is characterized by a tool, some machining parameters
  /// like the tolerance, the stock...
  /// </summary>
  [Serializable]
  public class OpSequence : DataWithDisplayFunction, ISequence, Lemoine.Collections.IDataWithId
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    int m_order = 0;
    TimeSpan? m_estimatedTime;
    ICadModel m_cadModel;
    IOperation m_operation;
    IPath m_path;
    string m_name;
    string m_description;
    string m_toolNumber;
    ITool m_tool;
    bool m_autoOnly = true;
    int m_frequency = 1;
    int? m_operationStep;
    ICollection<IStamp> m_stamps = new InitialNullIdSet<IStamp, int> ();
    ICollection<IStampingValue> m_stampingValues = new InitialNullIdSet<IStampingValue, int> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (OpSequence).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] { "Id", "Name", "Path", "Operation" }; }
    }

    /// <summary>
    /// Sequence ID
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
    /// Order of sequence in operation
    /// </summary>
    [XmlAttribute ("Order")]
    public virtual int Order
    {
      get { return this.m_order; }
      set { this.m_order = value; }
    }

    /// <summary>
    /// Reference to the CAD Model table
    /// </summary>
    [XmlIgnore]
    public virtual ICadModel CadModel
    {
      get { return m_cadModel; }
      set {
        if (object.Equals (m_cadModel, value)) {
          // Nothing to do
          return;
        }
        m_cadModel?.Sequences.Remove (this);
        m_cadModel = value;
        m_cadModel?.Sequences.Add (this);
      }
    }

    /// <summary>
    /// Parent operation
    /// </summary>
    [XmlIgnore]
    public virtual IOperation Operation
    {
      get { return m_operation; }

      set {
        if (object.Equals (m_operation, value)) {
          // Nothing to do
          return;
        }
        if ((null != m_operation)
            && (m_operation is Operation)) {
          (m_operation as Operation).RemoveSequenceForInternalUse (this);
        }
        m_operation = value;

        if ((null != m_operation)
            && (m_operation is Operation)) {
          (m_operation as Operation).AddSequenceForInternalUse (this);
        }
      }
    }

    /// <summary>
    /// Parent path
    /// </summary>
    [XmlIgnore]
    public virtual IPath Path
    {
      get { return m_path; }
      set {
        if (object.Equals (m_path, value)) {
          // Nothing to do
          return;
        }
        if ((null != m_path)
            && (m_path is OpPath)) {
          (m_path as OpPath).RemoveSequenceForInternalUse (this);
        }
        m_path = value;
        if ((null != m_path)
            && (m_path is OpPath)) {
          (m_path as OpPath).AddSequenceForInternalUse (this);
          m_operation = m_path.Operation;
        }
      }
    }

    /// <summary>
    /// Parent Path
    /// </summary>
    [XmlElement ("Path")]
    public virtual OpPath XmlSerializationPath
    {
      get { return this.Path as OpPath; }
      set { this.Path = value; }
    }


    /* Operation serialization is removed: the path is used for this
    /// <summary>
    /// Parent operation for Xml Serialization
    /// </summary>

    [XmlElement("Operation")]
    public virtual Operation XmlSerializationOperation {
      get { return this.Operation as Operation; }
      set { this.Operation = value; }
    }
     */

    /// <summary>
    /// Name of the sequence
    /// </summary>
    [XmlAttribute ("Name")]
    public virtual string Name
    {
      get { return m_name; }
      set { m_name = value; }
    }

    /// <summary>
    /// Description of the sequence
    /// </summary>
    [XmlAttribute ("Description")]
    public virtual string Description
    {
      get { return m_description; }
      set { m_description = value; }
    }

    /// <summary>
    /// Tool number
    /// </summary>
    [XmlAttribute ("ToolNumber")]
    public virtual string ToolNumber
    {
      get { return m_toolNumber; }
      set { m_toolNumber = value; }
    }

    /// <summary>
    /// Estimated time (in seconds)
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual TimeSpan? EstimatedTime
    {
      get { return m_estimatedTime; }
      set { m_estimatedTime = value; }
    }

    /// <summary>
    /// Estimated time (in seconds)
    /// </summary>
    [XmlIgnore]
    public virtual double? EstimatedTimeSeconds
    {
      get {
        return m_estimatedTime.HasValue
          ? m_estimatedTime.Value.TotalSeconds
          : (double?)null;
      }
      set {
        m_estimatedTime = value.HasValue
          ? TimeSpan.FromSeconds (value.Value)
          : (TimeSpan?)null;
      }
    }

    /// <summary>
    /// Estimated time as string
    /// </summary>
    [XmlAttribute ("EstimatedTime")]
    public virtual string EstimatedTimeAsString
    {
      get {
        return (this.EstimatedTime.HasValue)
          ? this.EstimatedTime.Value.ToString ()
          : null;
      }
      set {
        this.EstimatedTime = ConvertToNullableTimeSpan (value);
      }
    }

    /// <summary>
    /// Reference to the used tool
    /// </summary>
    [XmlIgnore]
    public virtual ITool Tool
    {
      get { return m_tool; }
      set { m_tool = value; }
    }

    /// <summary>
    /// Xml Serialization of Tool
    /// </summary>
    [XmlElement ("Tool")]
    public virtual Tool XmlSerializationTool
    {
      get { return this.Tool as Tool; }
      set { this.Tool = value; }
    }

    /// <summary>
    /// Should the sequence be only considered if the machine mode is flagged this way ?
    /// </summary>
    [XmlAttribute ("AutoOnly")]
    public virtual bool AutoOnly
    {
      get { return m_autoOnly; }
      set { m_autoOnly = value; }
    }


    /// <summary>
    /// Sequence frequency in operation (default 1 = sequence appears every time operation is executed)
    /// </summary>
    [XmlAttribute ("Frequency")]
    public virtual int Frequency
    {
      get { return m_frequency; }
      set { m_frequency = value; }
    }

    /// <summary>
    /// Operation step if known
    /// </summary>
    [XmlIgnore]
    [DefaultValue(null)]
    public virtual int? OperationStep
    {
      get { return m_operationStep; }
      set { m_operationStep = value; }
    }

    /// <summary>
    /// Operation step if known
    /// </summary>
    [XmlAttribute ("OperationStep")]
    public virtual int XmlSerializationOperationStep
    {
      get { return this.OperationStep.Value; }
      set { this.OperationStep = value; }
    }

    /// <summary>
    /// used to serialize OperationStep only when not null
    /// </summary>
    public virtual bool XmlSerializationOperationStepSpecified => this.OperationStep.HasValue;

    /// <summary>
    /// Sequence kind
    /// </summary>
    [XmlAttribute("Kind")]
    public virtual SequenceKind Kind { get; set; }

    /// <summary>
    /// Sequence detail (nullable)
    /// </summary>
    [XmlIgnore]
    public virtual SequenceDetail Detail { get; set; } = null;

    /// <summary>
    /// Set of stamps (and then ISO files) that are associated to this sequence
    /// </summary>
    [XmlIgnore]
    public virtual ICollection<IStamp> Stamps => m_stamps;

    /// <summary>
    /// Set of stamping values that are associated to this sequence
    /// </summary>
    [XmlIgnore]
    public virtual ICollection<IStampingValue> StampingValues => m_stampingValues;

    /// <summary>
    /// Convert a string to a nullable TimeSpan
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    TimeSpan? ConvertToNullableTimeSpan (string v)
    {
      if (string.IsNullOrEmpty (v)) {
        return default (TimeSpan?);
      }
      else {
        TimeSpan result;
        if (TimeSpan.TryParse (v, out result)) {
          return result;
        }
        else { // Not a d.hh:mm:ss string, consider this is may be a number of seconds
          return TimeSpan.FromSeconds (double.Parse (v, CultureInfo.InvariantCulture));
        }
      }
    }
    #endregion // Getters / Setters

    /// <summary>
    /// Default constructor
    /// </summary>
    public OpSequence ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="operation">not null</param>
    /// <param name="path">not null</param>
    public OpSequence (IOperation operation, IPath path)
    {
      Debug.Assert (null != operation);
      Debug.Assert (null != path);

      this.Operation = operation;
      this.Path = path;
    }

    /// <summary>
    /// Add a stamp in the member directly
    /// 
    /// To be used by the Stamp class only
    /// </summary>
    /// <param name="stamp"></param>
    protected internal virtual void AddStampForInternalUse (IStamp stamp)
    {
      AddToProxyCollection<IStamp> (m_stamps, stamp);
    }

    /// <summary>
    /// Remove a stamp in the member directly
    /// 
    /// To be used by the Stamp class only
    /// </summary>
    /// <param name="stamp"></param>
    protected internal virtual void RemoveStampForInternalUse (IStamp stamp)
    {
      RemoveFromProxyCollection<IStamp> (m_stamps, stamp);
    }

    /// <summary>
    /// Unproxy all the properties
    /// </summary>
    public virtual void Unproxy ()
    {
      if (m_tool != null) {
        NHibernateHelper.Unproxy<ITool> (ref m_tool);
      }
      if (m_path != null) {
        NHibernateHelper.Unproxy<IPath> (ref m_path);
      }
    }

    /// <summary>
    /// Make a shallow copy
    /// <see cref="ICloneable.Clone" />
    /// </summary>
    /// <returns></returns>
    public virtual OpSequence CloneForXmlSerialization ()
    {
      var clone = new OpSequence ();
      clone.m_id = Id;
      clone.CadModel = CadModel;
      // Operation is not serialized
      clone.AutoOnly = AutoOnly;
      clone.Description = Description;
      // Detail is not serialized
      // StampingValues and Stamps are not serialized
      clone.EstimatedTime = EstimatedTime;
      clone.Frequency = Frequency;
      clone.Kind = Kind;
      clone.Name = Name;
      clone.OperationStep = OperationStep;
      clone.Order = Order;
      clone.Path = Path;
      clone.Tool = Tool;
      clone.ToolNumber = ToolNumber;
      clone.m_version = Version;
      return clone;
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[Sequence {this.Id} Name={this.Name} Order={this.Order}]";
      }
      else {
        return $"[Sequence {this.Id}]";
      }
    }

    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (ISequence other)
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
      OpSequence other = obj as OpSequence;
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
    /// Comparison of sequences based on their order
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public virtual int CompareTo (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return 0;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      if (obj == null) {
        throw new ArgumentException ("Comparison of a sequence with a null");
      }
      ISequence other = obj as OpSequence;
      if (null == other) {
        throw new ArgumentException ("Comparison of a sequence with another type");
      }
      return this.Order.CompareTo (other.Order);
    }
  }

  /// <summary>
  /// Convert a SequenceKind enum to a string in database
  /// </summary>
  [Serializable]
  public class SequenceKindType : EnumStringType
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public SequenceKindType () : base (typeof (SequenceKind))
    {
    }
  }
}

