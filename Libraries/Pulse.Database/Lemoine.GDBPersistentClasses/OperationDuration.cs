// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Serialization;
using System.Linq;

using Lemoine.Collections;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table OperationDuration
  /// 
  /// Implements <see cref="IOperationDuration"/>
  /// </summary>
  [Serializable]
  public class OperationDuration : BaseData, IOperationDuration, IEquatable<IOperationDuration>, Lemoine.Collections.IDataWithId
  {
    #region Members
    IOperation m_operation;
    /* TODO: for later once the table exists
    DateTime m_dateTime = DateTime.UtcNow;
    */
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (OperationDuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] { "Id" }; }
    }

    /// <summary>
    /// Operation ID
    /// </summary>
    [XmlAttribute ("Id")]
    public virtual int Id => m_operation.Id;

    /// <summary>
    /// Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version => m_operation.Version;

    /// <summary>
    /// <see cref="IOperationDuration"/>
    /// </summary>
    [XmlIgnore]
    public virtual IOperation Operation => m_operation;

    /// <summary>
    /// <see cref="IOperationDuration"/>
    /// </summary>
    public virtual IOperationModel OperationModel { get; set; }

    /// <summary>
    /// <see cref="IOperationDuration"/>
    /// </summary>
    public virtual IMachineFilter MachineFilter { get; set; }

    /// <summary>
    /// <see cref="IOperationDuration"/>
    /// </summary>
    public virtual UtcDateTimeRange ApplicableRange { get; set; } = new UtcDateTimeRange (new LowerBound<DateTime> (null), new UpperBound<DateTime> (null));

    /// <summary>
    /// Estimated machining duration
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual TimeSpan? Machining { get; set; }

    /// <summary>
    /// Estimated machining duration as string
    /// </summary>
    [XmlAttribute ("MachiningDuration")]
    public virtual string MachiningDurationAsString
    {
      get {
        return (this.Machining.HasValue)
          ? this.Machining.Value.ToString ()
          : null;
      }
      set {
        this.Machining = ConvertToNullableTimeSpan (value);
      }
    }

    /// <summary>
    /// Estimated machining hours
    /// </summary>
    [XmlAttribute ("MachiningHours")]
    public virtual double MachiningHours
    {
      get {
        return (this.Machining.HasValue)
          ? this.Machining.Value.TotalHours
          : 0;
      }
      set {
        if (0 < value) {
          this.Machining = TimeSpan.FromHours (value);
        }
        else {
          this.Machining = null;
        }
      }
    }

    /// <summary>
    /// Estimated setup duration
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual TimeSpan? Setup { get; set; }

    /// <summary>
    /// Estimated setup duration as string
    /// </summary>
    [XmlAttribute ("SetUpDuration")]
    public virtual string SetUpDurationAsString
    {
      get {
        return (this.Setup.HasValue)
          ? this.Setup.Value.ToString ()
          : null;
      }
      set {
        this.Setup = ConvertToNullableTimeSpan (value);
      }
    }

    /// <summary>
    /// Estimated set up hours
    /// </summary>
    [XmlAttribute ("SetUpHours")]
    public virtual double SetUpHours
    {
      get {
        return (this.Setup.HasValue)
          ? this.Setup.Value.TotalHours
          : 0;
      }
      set {
        if (0 < value) {
          this.Setup = TimeSpan.FromHours (value);
        }
        else {
          this.Setup = null;
        }
      }
    }

    /// <summary>
    /// Estimated tear down duration
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual TimeSpan? Teardown { get; set; }

    /// <summary>
    /// Estimated tear down duration as string
    /// </summary>
    [XmlAttribute ("TearDownDuration")]
    public virtual string TearDownDurationAsString
    {
      get {
        return (this.Teardown.HasValue)
          ? this.Teardown.Value.ToString ()
          : null;
      }
      set {
        this.Teardown = ConvertToNullableTimeSpan (value);
      }
    }

    /// <summary>
    /// Estimated tear down hours
    /// </summary>
    [XmlAttribute ("TearDownHours")]
    public virtual double TearDownHours
    {
      get {
        return (this.Teardown.HasValue)
          ? this.Teardown.Value.TotalHours
          : 0;
      }
      set {
        if (0 < value) {
          this.Teardown = TimeSpan.FromHours (value);
        }
        else {
          this.Teardown = null;
        }
      }
    }

    /// <summary>
    /// Estimated loading duration
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual TimeSpan? Loading { get; set; }

    /// <summary>
    /// Estimated loading duration as string
    /// </summary>
    [XmlAttribute ("LoadingDuration")]
    public virtual string LoadingDurationAsString
    {
      get {
        return (this.Loading.HasValue)
          ? this.Loading.Value.ToString ()
          : null;
      }
      set {
        this.Loading = ConvertToNullableTimeSpan (value);
      }
    }

    /// <summary>
    /// Estimated loading hours
    /// </summary>
    [XmlAttribute ("LoadingHours")]
    public virtual double LoadingHours
    {
      get {
        return (this.Loading.HasValue)
          ? this.Loading.Value.TotalHours
          : 0;
      }
      set {
        if (0 < value) {
          this.Loading = TimeSpan.FromHours (value);
        }
        else {
          this.Loading = null;
        }
      }
    }

    /// <summary>
    /// Estimated unloading duration
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual TimeSpan? Unloading { get; set; }

    /// <summary>
    /// Estimated unloading duration as string
    /// </summary>
    [XmlAttribute ("UnloadingDuration")]
    public virtual string UnloadingDurationAsString
    {
      get {
        return (this.Unloading.HasValue)
          ? this.Unloading.Value.ToString ()
          : null;
      }
      set {
        this.Unloading = ConvertToNullableTimeSpan (value);
      }
    }

    /// <summary>
    /// Estimated unloading hours
    /// </summary>
    [XmlAttribute ("UnloadingHours")]
    public virtual double UnloadingHours
    {
      get {
        return (this.Unloading.HasValue)
          ? this.Unloading.Value.TotalHours
          : 0;
      }
      set {
        if (0 < value) {
          this.Unloading = TimeSpan.FromHours (value);
        }
        else {
          this.Unloading = null;
        }
      }
    }


    #endregion // Getters / Setters

    #region Contructors
    /// <summary>
    /// The default constructor is forbidden
    /// and is only used by NHibernate
    /// </summary>
    protected OperationDuration ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="operation">not null</param>
    internal protected OperationDuration (IOperation operation, DateTime? from = null)
    {
      Debug.Assert (null != operation);

      m_operation = operation;
      if (from.HasValue) {
        this.ApplicableRange = new UtcDateTimeRange (from.Value);
      }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="operationModel">not null</param>
    internal protected OperationDuration (IOperationModel operationModel, DateTime? from = null)
    {
      Debug.Assert (null != operationModel);

      m_operation = operationModel.Operation;
      this.OperationModel = operationModel;
      if (from.HasValue) {
        this.ApplicableRange = new UtcDateTimeRange (from.Value);
      }
    }
    #endregion // Constructors

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
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IOperation> (ref m_operation);
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[OperationDuration {this.Id}]";
      }
      else {
        return $"[OperationDuration {this.Id}]";
      }
    }

    /// <summary>
    /// Indicates whether the current object
    /// is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (IOperationDuration other)
    {
      return this.Equals ((object)other);
    }

    /// <summary>
    /// Determines whether the specified Object
    /// is equal to the current Object
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
      OperationDuration other = obj as OperationDuration;
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
