// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Collections;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ReasonProposal
  /// </summary>
  public class ReasonProposal
    : IReasonProposal
    , IVersionable
    , IEquatable<IReasonProposal>
  {
    int m_id = 0;
    int m_version = 0;
    IMachine m_machine;
    long m_modificationId;
    UtcDateTimeRange m_dateTimeRange;
    IReason m_reason;
    double m_reasonScore;
    ReasonProposalKind m_kind;
    string m_reasonDetails;
    string m_jsonData;

    static readonly ILog log = LogManager.GetLogger (typeof (ReasonProposal).FullName);

    /// <summary>
    /// Id
    /// </summary>
    public virtual int Id => this.m_id;

    /// <summary>
    /// Version
    /// </summary>
    public virtual int Version => this.m_version;

    /// <summary>
    /// <see cref="IReasonProposal"/>
    /// </summary>
    public virtual IMachine Machine => m_machine;

    /// <summary>
    /// <see cref="IReasonProposal"/>
    /// </summary>
    public virtual long ModificationId => m_modificationId;

    /// <summary>
    /// <see cref="IReasonProposal"/>
    /// </summary>
    public virtual UtcDateTimeRange DateTimeRange
    {
      get { return m_dateTimeRange; }
      set { m_dateTimeRange = value; }
    }

    /// <summary>
    /// <see cref="IReasonProposal"/>
    /// </summary>
    public virtual IReason Reason => m_reason;

    /// <summary>
    /// <see cref="IReasonProposal"/>
    /// </summary>
    public virtual double ReasonScore => m_reasonScore;

    /// <summary>
    /// <see cref="IReasonProposal"/>
    /// </summary>
    public virtual ReasonProposalKind Kind => m_kind;

    /// <summary>
    /// <see cref="IReasonProposal"/>
    /// </summary>
    public virtual string ReasonDetails => m_reasonDetails;

    /// <summary>
    /// <see cref="IPossibleReason"/>
    /// </summary>
    public virtual bool OverwriteRequired => this.Kind.IsOverwriteRequired ();

    /// <summary>
    /// <see cref="IPossibleReason"/>
    /// </summary>
    public virtual ReasonSource ReasonSource => this.Kind.ConvertToReasonSource ();

    /// <summary>
    /// Reason data in Json format
    /// </summary>
    public virtual string JsonData => m_jsonData;

    #region IPossibleReason implementation
    /// <summary>
    /// <see cref="IPossibleReason"/>
    /// </summary>
    public virtual IDictionary<string, object> Data => Pulse.Business.Reason.ReasonData.Deserialize (m_jsonData);

    /// <summary>
    /// <see cref="IPossibleReason"/>
    /// </summary>
    public virtual UtcDateTimeRange RestrictedRange => this.DateTimeRange;

    /// <summary>
    /// <see cref="IPossibleReason"/>
    /// </summary>
    public virtual IMachineMode RestrictedMachineMode => null;

    /// <summary>
    /// <see cref="IPossibleReason"/>
    /// </summary>
    public virtual IMachineObservationState RestrictedMachineObservationState => null;
    #endregion // IPossibleReason implementation

    #region Constructors
    /// <summary>
    /// Protected constructor with no arguments
    /// </summary>
    protected ReasonProposal ()
    {
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="reasonMachineAssociation">not null with a not null reason</param>
    /// <param name="range"></param>
    internal protected ReasonProposal (IReasonMachineAssociation reasonMachineAssociation, UtcDateTimeRange range)
    {
      Debug.Assert (null != reasonMachineAssociation);
      Debug.Assert (null != reasonMachineAssociation.Reason);
      Debug.Assert (0 < (int)reasonMachineAssociation.Kind);
      Debug.Assert (!range.IsEmpty ());

      m_machine = reasonMachineAssociation.Machine;
      m_modificationId = ((IDataWithId<long>)reasonMachineAssociation).Id;
      m_dateTimeRange = range;
      m_reason = reasonMachineAssociation.Reason;
      m_jsonData = reasonMachineAssociation.JsonData;
      m_reasonScore = reasonMachineAssociation.ReasonScore;
      m_kind = reasonMachineAssociation.Kind.ConvertToReasonProposalKind ();
      m_reasonDetails = reasonMachineAssociation.ReasonDetails;
    }
    #endregion // Constructors

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[ReasonProposal {this.Id} {this.Reason?.ToStringIfInitialized ()} Range={this.DateTimeRange}]";
      }
      else {
        return $"[ReasonProposal {this.Id}]";
      }
    }
    
    /// <summary>
    /// Indicates whether the current object
    /// is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (IReasonProposal other)
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
      IReasonProposal other = obj as ReasonProposal;
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
        int hashCode = 0;
        unchecked {
          hashCode += ModificationId.GetHashCode ();
        }
        return hashCode;
      }
    }
  }
}
