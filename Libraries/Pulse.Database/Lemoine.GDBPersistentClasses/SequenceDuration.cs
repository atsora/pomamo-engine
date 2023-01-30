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
  /// Persistent class of table SequenceDuration
  /// 
  /// Implements <see cref="ISequenceDuration"/>
  /// </summary>
  [Serializable]
  public class SequenceDuration : BaseData, ISequenceDuration, IEquatable<ISequenceDuration>, Lemoine.Collections.IDataWithId
  {
    #region Members
    ISequenceOperationModel m_sequenceOperationModel;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (SequenceDuration).FullName);

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
    public virtual int Id => m_sequenceOperationModel.Sequence.Id;

    /// <summary>
    /// Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version => m_sequenceOperationModel.Sequence.Version;

    /// <summary>
    /// <see cref="ISequenceDuration"/>
    /// </summary>
    [XmlIgnore]
    public virtual ISequenceOperationModel SequenceOperationModel => m_sequenceOperationModel;

    /// <summary>
    /// <see cref="ISequenceDuration"/>
    /// </summary>
    public virtual IMachineFilter MachineFilter { get; set; }

    /// <summary>
    /// <see cref="ISequenceDuration"/>
    /// </summary>
    [XmlIgnore]
    public virtual UtcDateTimeRange ApplicableRange { get; set; } = new UtcDateTimeRange (new LowerBound<DateTime> (null), new UpperBound<DateTime> (null));

    /// <summary>
    /// Estimated duration
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan EstimatedDuration { get; set; }

    /// <summary>
    /// Estimated duration as string
    /// </summary>
    [XmlAttribute ("EstimatedDuration")]
    public virtual string EstimatedDurationAsString
    {
      get => this.EstimatedDuration.ToString ();
      set {
        this.EstimatedDuration = ConvertToTimeSpan (value);
      }
    }
    #endregion // Getters / Setters

    #region Contructors
    /// <summary>
    /// The default constructor is forbidden
    /// and is only used by NHibernate
    /// </summary>
    protected SequenceDuration ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="sequenceOperationModel">not null</param>
    /// <param name="estimatedDuration"></param>
    public SequenceDuration (ISequenceOperationModel sequenceOperationModel, TimeSpan estimatedDuration)
    {
      Debug.Assert (null != sequenceOperationModel);

      m_sequenceOperationModel = sequenceOperationModel;
      this.EstimatedDuration = estimatedDuration;
    }
    #endregion // Constructors

    /// <summary>
    /// Convert a string to a TimeSpan
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    TimeSpan ConvertToTimeSpan (string v)
    {
      TimeSpan result;
      if (TimeSpan.TryParse (v, out result)) {
        return result;
      }
      else { // Not a d.hh:mm:ss string, consider this is may be a number of seconds
        return TimeSpan.FromSeconds (double.Parse (v, CultureInfo.InvariantCulture));
      }
    }
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[SequenceDuration {this.Id} Duration={this.EstimatedDuration}]";
      }
      else {
        return $"[SequenceDuration {this.Id}]";
      }
    }

    /// <summary>
    /// Indicates whether the current object
    /// is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (ISequenceDuration other)
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
      SequenceDuration other = obj as SequenceDuration;
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
