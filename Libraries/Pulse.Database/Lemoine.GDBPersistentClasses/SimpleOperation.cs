// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Serialization;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Class that corresponds to a SimpleOperation
  /// that is a join between the Operation and the IntermediateWorkPiece tables
  /// in case the data structure option IntermediateWorkPieceOperationIsSimpleOperation is set
  /// </summary>
  [Serializable]
  public class SimpleOperation: DataWithPatternName, ISimpleOperation, IEquatable<ISimpleOperation>
  {
    #region Members
    IIntermediateWorkPiece m_intermediateWorkPiece;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (SimpleOperation).FullName);

    #region Constructors
    /// <summary>
    /// Constructor for a new SimpleOperation
    /// </summary>
    internal protected SimpleOperation (IOperationType operationType)
    {
      Operation operation = new Operation (operationType);
      m_intermediateWorkPiece = new IntermediateWorkPiece (operation);
    }
    
    /// <summary>
    /// Constructor from an operation
    /// </summary>
    /// <param name="operation"></param>
    internal protected SimpleOperation (IOperation operation)
    {
      m_intermediateWorkPiece =
        GetFirst<IIntermediateWorkPiece> (operation.IntermediateWorkPieces);
      Debug.Assert (null != m_intermediateWorkPiece);
    }
    
    /// <summary>
    /// Constructor from an intermediate work piece
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    internal protected SimpleOperation (IIntermediateWorkPiece intermediateWorkPiece)
    {
      m_intermediateWorkPiece = intermediateWorkPiece;
      Debug.Assert (null != m_intermediateWorkPiece);
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Id
    /// </summary>
    [XmlIgnore]
    public virtual int Id
    {
      get { return IntermediateWorkPieceId; }
    }
    
    /// <summary>
    /// Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return m_intermediateWorkPiece.Version; }
    }

    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"IntermediateWorkPieceId", "ExternalCode", "Code", "Name"}; }
    }
    
    /// <summary>
    /// Reference to the associated operation
    /// </summary>
    [XmlIgnore]
    public virtual IOperation Operation
    {
      get { return m_intermediateWorkPiece.Operation; }
    }
    
    /// <summary>
    /// Operation ID
    /// </summary>
    [XmlAttribute("OperationId")]
    public virtual int OperationId
    {
      get { return ((Lemoine.Collections.IDataWithId)m_intermediateWorkPiece.Operation).Id; }
    }
    
    /// <summary>
    /// Reference to the associated intermediate work piece
    /// </summary>
    [XmlIgnore]
    public virtual IIntermediateWorkPiece IntermediateWorkPiece
    {
      get { return m_intermediateWorkPiece; }
    }
    
    /// <summary>
    /// Intermediate work piece ID
    /// </summary>
    [XmlAttribute("IntermediateWorkPieceId")]
    public virtual int IntermediateWorkPieceId {
      get { return ((Lemoine.Collections.IDataWithId)m_intermediateWorkPiece).Id; }
    }
    
    /// <summary>
    /// Full name of the operation as used in the shop (written in the planning)
    /// 
    /// It corresponds to the operation name and the intermediate work piece name.
    /// </summary>
    [XmlAttribute("Name")]
    public virtual string Name
    {
      get { return m_intermediateWorkPiece.Operation.Name; }
      set
      {
        m_intermediateWorkPiece.Name = value;
        m_intermediateWorkPiece.Operation.Name = value;
      }
    }
    
    /// <summary>
    /// Operation code
    /// 
    /// It corresponds to the operation code and the intermediate work piece code
    /// </summary>
    [XmlAttribute("Code")]
    public virtual string Code {
      get { return m_intermediateWorkPiece.Operation.Code; }
      set
      {
        m_intermediateWorkPiece.Code = value;
        m_intermediateWorkPiece.Operation.Code = value;
      }
    }
    
    /// <summary>
    /// External code
    /// 
    /// It may help synchronizing data with en external database
    /// 
    /// It corresponds to the operation external code and the intermediate work piece external code
    /// </summary>
    [XmlAttribute("ExternalCode")]
    public virtual string ExternalCode {
      get { return m_intermediateWorkPiece.Operation.ExternalCode; }
      set
      {
        m_intermediateWorkPiece.ExternalCode = value;
        m_intermediateWorkPiece.Operation.ExternalCode = value;
      }
    }
    
    /// <summary>
    /// Link to the documentation in the network
    /// 
    /// It corresponds to the operation document link and the intermediate work piece document link.
    /// </summary>
    [XmlAttribute("DocumentLink")]
    public virtual string DocumentLink {
      get { return m_intermediateWorkPiece.Operation.DocumentLink; }
      set
      {
        m_intermediateWorkPiece.DocumentLink = value;
        m_intermediateWorkPiece.Operation.DocumentLink = value;
      }
    }
    
    /// <summary>
    /// Associated operation type
    /// </summary>
    [XmlIgnore]
    public virtual IOperationType Type {
      get { return m_intermediateWorkPiece.Operation.Type; }
      set { m_intermediateWorkPiece.Operation.Type = value; }
    }
    
    /// <summary>
    /// Associated operation type for Xml Serialization
    /// </summary>
    [XmlElement("Type")]
    public virtual OperationType XmlSerializationType {
      get { return this.Type as OperationType; }
      set { this.Type = value; }
    }

    /// <summary>
    /// Number of intermediate work pieces the operation makes.
    /// </summary>
    [XmlAttribute("Quantity"), MergeAuto]
    public virtual int Quantity {
      get { return m_intermediateWorkPiece.OperationQuantity; }
      set { m_intermediateWorkPiece.OperationQuantity = value; }
    }
    
    /// <summary>
    /// Estimated machining duration
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan? MachiningDuration {
      get { return m_intermediateWorkPiece.Operation.MachiningDuration; }
      set { m_intermediateWorkPiece.Operation.MachiningDuration = value; }
    }

    /// <summary>
    /// Estimated machining duration as string
    /// </summary>
    [XmlAttribute("MachiningDuration")]
    public virtual string MachiningDurationAsString {
      get
      {
        return (this.MachiningDuration.HasValue)
          ? this.MachiningDuration.Value.ToString ()
          : null;
      }
      set
      {
        this.MachiningDuration = ConvertToNullableTimeSpan (value);
      }
    }

    /// <summary>
    /// Creation date/time
    /// </summary>
    [XmlIgnore]
    public virtual DateTime CreationDateTime => m_intermediateWorkPiece.Operation.CreationDateTime;

    /// <summary>
    /// Return a value if the operation has been archived
    /// </summary>
    [XmlIgnore]
    public virtual DateTime? ArchiveDateTime
    {
      get { return m_intermediateWorkPiece.Operation.ArchiveDateTime; }
      set { m_intermediateWorkPiece.Operation.ArchiveDateTime = value; }
    }

    /// <summary>
    /// Archive DateTime as string
    /// </summary>
    [XmlAttribute ("ArchiveDateTime")]
    public virtual string ArchiveDateTimeAsString
    {
      get {
        return (this.ArchiveDateTime.HasValue)
          ? this.ArchiveDateTime.Value.ToString ()
          : null;
      }
      set {
        this.ArchiveDateTime = ConvertToNullableDateTime (value);
      }
    }

    /// <summary>
    /// Estimated setup duration
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan? SetUpDuration {
      get { return m_intermediateWorkPiece.Operation.SetUpDuration; }
      set { m_intermediateWorkPiece.Operation.SetUpDuration = value; }
    }
    
    /// <summary>
    /// Estimated setup duration as string for XML serialization
    /// </summary>
    [XmlAttribute("SetUpDuration")]
    public virtual string SetUpDurationAsString {
      get
      {
        return (this.SetUpDuration.HasValue)
          ? this.SetUpDuration.Value.ToString ()
          : null;
      }
      set
      {
        this.SetUpDuration = ConvertToNullableTimeSpan (value);
      }
    }

    /// <summary>
    /// Estimated tear down duration
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan? TearDownDuration {
      get { return m_intermediateWorkPiece.Operation.TearDownDuration; }
      set { m_intermediateWorkPiece.Operation.TearDownDuration = value; }
    }
    
    /// <summary>
    /// Estimated tear down duration as string
    /// </summary>
    [XmlAttribute("TearDownDuration")]
    public virtual string TearDownDurationAsString {
      get
      {
        return (this.TearDownDuration.HasValue)
          ? this.TearDownDuration.Value.ToString ()
          : null;
      }
      set
      {
        this.TearDownDuration = ConvertToNullableTimeSpan (value);
      }
    }
    
    /// <summary>
    /// Estimated loading duration
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan? LoadingDuration {
      get { return m_intermediateWorkPiece.Operation.LoadingDuration; }
      set { m_intermediateWorkPiece.Operation.LoadingDuration = value; }
    }
    
    /// <summary>
    /// Estimated loading duration as string for XML serialization
    /// </summary>
    [XmlAttribute("LoadingDuration")]
    public virtual string LoadingDurationAsString {
      get
      {
        return (this.LoadingDuration.HasValue)
          ? this.LoadingDuration.Value.ToString ()
          : null;
      }
      set
      {
        this.LoadingDuration = ConvertToNullableTimeSpan (value);
      }
    }

    /// <summary>
    /// Estimated unloading duration
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan? UnloadingDuration {
      get { return m_intermediateWorkPiece.Operation.UnloadingDuration; }
      set { m_intermediateWorkPiece.Operation.UnloadingDuration = value; }
    }
    
    /// <summary>
    /// Estimated unloading duration as string for XML serialization
    /// </summary>
    [XmlAttribute("UnloadingDuration")]
    public virtual string UnloadingDurationAsString {
      get
      {
        return (this.UnloadingDuration.HasValue)
          ? this.UnloadingDuration.Value.ToString ()
          : null;
      }
      set
      {
        this.UnloadingDuration = ConvertToNullableTimeSpan (value);
      }
    }

    /// <summary>
    /// Weight of the intermediate work piece when it is done
    /// 
    /// (this may help counting the number of made work pieces)
    /// </summary>
    [XmlIgnore]
    public virtual double? Weight {
      get { return m_intermediateWorkPiece.Weight; }
      set { m_intermediateWorkPiece.Weight = value; }
    }
    
    /// <summary>
    /// First component that is associated to this operation
    /// </summary>
    [XmlIgnore]
    public virtual IComponent Component {
      get { return GetFirst<IComponentIntermediateWorkPiece> (m_intermediateWorkPiece.ComponentIntermediateWorkPieces).Component; }
    }
    
    /// <summary>
    /// Set of components this intermediate work piece is known to be a part of
    /// </summary>
    [XmlIgnore]
    public virtual ICollection<IComponentIntermediateWorkPiece> ComponentIntermediateWorkPieces {
      get { return m_intermediateWorkPiece.ComponentIntermediateWorkPieces; }
    }
    #endregion // Getters / Setters
    
    #region Add methods
    /// <summary>
    /// Add a possible next operation
    /// </summary>
    /// <param name="operation"></param>
    public virtual void AddPossibleNextOperation (IOperation operation)
    {
      m_intermediateWorkPiece.AddPossibleNextOperation (operation);
    }

    /// <summary>
    /// Remove a possible next operation
    /// </summary>
    /// <param name="operation"></param>
    public virtual void RemovePossibleNextOperation (IOperation operation)
    {
      m_intermediateWorkPiece.RemovePossibleNextOperation (operation);
    }
    #endregion // Add methods
    
    #region Methods
    /// <summary>
    /// Check if the simple operation is undefined
    /// 
    /// An simple operation is considered as undefined if it has no name and no given type
    /// </summary>
    /// <returns></returns>
    public virtual bool IsUndefined ()
    {
      return ( (this.Name == null)
              || (0 == this.Name.Length))
        && (1 == this.Type.Id);
    }
    
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

    /// <summary>
    /// Convert a string to a nullable DateTime
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    DateTime? ConvertToNullableDateTime (string v)
    {
      return string.IsNullOrEmpty (v) ? default (DateTime?) : DateTime.Parse (v);
    }
    #endregion // Methods

    /// <summary>
    /// Indicates whether the current object
    /// is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals(ISimpleOperation other)
    {
      if (object.ReferenceEquals(this,other)) {
        return true;
      }

      if (other == null) {
        return false;
      }

      return (other.IntermediateWorkPieceId == this.IntermediateWorkPieceId && other.OperationId == this.OperationId);
    }
    
    /// <summary>
    /// Determines whether the specified Object is equal to the current Object
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
      ISimpleOperation other = obj as SimpleOperation;
      if (null == other) {
        return false;
      }
      return (other.IntermediateWorkPieceId == this.IntermediateWorkPieceId && other.OperationId == this.OperationId);
    }

    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode()
    {
      return m_intermediateWorkPiece.GetHashCode();
    }
  }
}
