// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of view SimpleOperation
  /// that is a join between the Operation and the IntermediateWorkPiece tables
  /// in case the data structure option IntermediateWorkPieceOperationIsSimpleOperation is set
  /// 
  /// Use this class very carefully. Each time it is possible, prefer to use
  /// the SimpleOperation class instead.
  /// Any modification made in SimpleOperationView is not propagated to the other classes,
  /// which may cause some problems.
  /// </summary>
  [Serializable]
  public class SimpleOperationView: BaseData
  {
    #region Members
    int m_operationId = 0;
    int m_intermediateWorkPieceId = 0;
    string m_name;
    string m_code;
    string m_externalCode;
    string m_documentLink;
    IOperationType m_type;
    TimeSpan? m_machiningDuration;
    TimeSpan? m_setUpDuration;
    TimeSpan? m_tearDownDuration;
    TimeSpan? m_loadingDuration;
    TimeSpan? m_unloadingDuration;
    double? m_weight;
    IComponent m_component;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (SimpleOperationView).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"ExternalCode", "Code", "Name"}; }
    }
    
    /// <summary>
    /// Operation ID
    /// </summary>
    [XmlAttribute("OperationId")]
    public virtual int OperationId
    {
      get { return this.m_operationId; }
    }
    
    /// <summary>
    /// Intermediate work piece ID
    /// </summary>
    [XmlAttribute("IntermediateWorkPieceId")]
    public virtual int IntermediateWorkPieceId {
      get { return m_intermediateWorkPieceId; }
    }
    
    /// <summary>
    /// IDisplay implementation
    /// </summary>
    [XmlIgnore]
    public virtual string Display
    {
      get; set;
    }
    
    /// <summary>
    /// Full name of the operation as used in the shop (written in the planning)
    /// 
    /// It corresponds to the operation name and the intermediate work piece name.
    /// </summary>
    [XmlAttribute("Name")]
    public virtual string Name
    {
      get { return this.m_name; }
      set { this.m_name = value; }
    }
    
    /// <summary>
    /// Operation code
    /// 
    /// It corresponds to the operation code and the intermediate work piece code
    /// </summary>
    [XmlAttribute("Code")]
    public virtual string Code {
      get { return m_code; }
      set { m_code = value; }
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
      get { return m_externalCode; }
      set { m_externalCode = value; }
    }
    
    /// <summary>
    /// Link to the documentation in the network
    /// 
    /// It corresponds to the operation document link and the intermediate work piece document link.
    /// </summary>
    [XmlAttribute("DocumentLink")]
    public virtual string DocumentLink {
      get { return m_documentLink; }
      set { m_documentLink = value; }
    }
    
    /// <summary>
    /// Associated operation type
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual IOperationType Type {
      get { return m_type; }
      set { m_type = value; }
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
    /// Estimated machining duration
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual TimeSpan? MachiningDuration {
      get { return m_machiningDuration; }
      set { m_machiningDuration = value; }
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
    /// Estimated setup duration
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual TimeSpan? SetUpDuration {
      get { return m_setUpDuration; }
      set { m_setUpDuration = value; }
    }
    
    /// <summary>
    /// Estimated setup duration as string for XML serialization
    /// </summary>
    [XmlAttribute("SetUpDuration")]
    public virtual string SetupDurationAsString {
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
    [XmlIgnore, MergeAuto]
    public virtual TimeSpan? TearDownDuration {
      get { return m_tearDownDuration; }
      set { m_tearDownDuration = value; }
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
    [XmlIgnore, MergeAuto]
    public virtual TimeSpan? LoadingDuration {
      get { return m_loadingDuration; }
      set { m_loadingDuration = value; }
    }
    
    /// <summary>
    /// Estimated loading duration as string
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
    [XmlIgnore, MergeAuto]
    public virtual TimeSpan? UnloadingDuration {
      get { return m_unloadingDuration; }
      set { m_unloadingDuration = value; }
    }
    
    /// <summary>
    /// Estimated unloading duration as string
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
      get { return m_weight; }
      set { m_weight = value; }
    }
    
    /// <summary>
    /// Component that is associated to this operation
    /// 
    /// Warning ! The setter works only locally
    /// The component is not updated accordingly to reflect the new operation
    /// </summary>
    [MergeParent, XmlIgnore]
    public virtual IComponent Component {
      get { return m_component; }
      set { m_component = value; }
    }
    
    /// <summary>
    /// Component that is associated to this operation for Xml Serialization
    /// 
    /// Warning ! The setter works only locally
    /// The component is not updated accordingly to reflect the new operation
    /// </summary>
    [MergeParent, XmlElement("Component")]
    public virtual Component XmlSerializationComponent {
      get { return this.Component as Component; }
      set { this.Component = value; }
    }
    #endregion // Getters / Setters
    
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
    #endregion // Methods
  }
}
