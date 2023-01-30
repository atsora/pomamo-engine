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
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business.Config;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table Operation
  /// </summary>
  [Serializable]
  public class Operation : DataWithDisplayFunction, IOperation, IMergeable<IOperation>, IEquatable<IOperation>, Lemoine.Collections.IDataWithId
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
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
    DateTime m_creationDateTime = DateTime.UtcNow;
    bool m_lock = true;
    IMachineFilter m_machineFilter = null;
    ICollection<IIntermediateWorkPiece> m_intermediateWorkPieces = new InitialNullIdSet<IIntermediateWorkPiece, int> ();
    ICollection<IIntermediateWorkPiece> m_sources = new InitialNullIdSet<IIntermediateWorkPiece, int> ();
    ICollection<IPath> m_paths = new InitialNullIdSortedSet<IPath, int> ();
    ICollection<ISequence> m_sequences = new InitialNullIdSet<ISequence, int> ();
    ICollection<IStamp> m_stamps = new InitialNullIdSet<IStamp, int> ();
    DateTime? m_archiveDateTime = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (Operation).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] { "Id", "ExternalCode", "Code", "Name", "Type" }; }
    }

    /// <summary>
    /// Operation ID
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
    /// Associated SimpleOperation
    /// </summary>
    [XmlIgnore]
    public virtual ISimpleOperation SimpleOperation
    {
      get { return new SimpleOperation (this); }
    }

    /// <summary>
    /// Long display name that is retrieved with the display_long function
    /// </summary>
    [XmlIgnore]
    public virtual string LongDisplay
    {
      get; set;
    }

    /// <summary>
    /// Short display name that is retrieved with the display_short function
    /// </summary>
    [XmlIgnore]
    public virtual string ShortDisplay
    {
      get; set;
    }

    /// <summary>
    /// Full name of the operation as used in the shop (written in the planning)
    /// </summary>
    [XmlAttribute ("Name"), MergeAuto]
    public virtual string Name
    {
      get { return this.m_name; }
      set { this.m_name = value; }
    }

    /// <summary>
    /// Operation code
    /// </summary>
    [XmlAttribute ("Code"), MergeAuto]
    public virtual string Code
    {
      get { return m_code; }
      set { m_code = value; }
    }

    /// <summary>
    /// External code
    /// 
    /// It may help synchronizing data with en external database
    /// </summary>
    [XmlAttribute ("ExternalCode"), MergeAuto]
    public virtual string ExternalCode
    {
      get { return m_externalCode; }
      set { m_externalCode = value; }
    }

    /// <summary>
    /// Link to the documentation in the network
    /// </summary>
    [XmlAttribute ("DocumentLink"), MergeAuto]
    public virtual string DocumentLink
    {
      get { return m_documentLink; }
      set { m_documentLink = value; }
    }

    /// <summary>
    /// Associated operation type
    /// </summary>
    [MergeAuto, XmlIgnore]
    public virtual IOperationType Type
    {
      get { return m_type; }
      set { m_type = value; }
    }

    /// <summary>
    /// Associated operation type for Xml Serialization
    /// </summary>
    [XmlElement ("Type")]
    public virtual OperationType XmlSerializationType
    {
      get { return this.Type as OperationType; }
      set { this.Type = value; }
    }

    /// <summary>
    /// <see cref="IOperation"/>
    /// </summary>
    [XmlIgnore]
    public virtual IOperationRevision ActiveRevision => new OperationRevision (this);

    /// <summary>
    /// <see cref="IOperation"/>
    /// </summary>
    [XmlIgnore]
    public virtual IList<IOperationRevision> Revisions => new List<IOperationRevision> { ActiveRevision };

    /// <summary>
    /// <see cref="IOperation"/>
    /// </summary>
    [XmlIgnore]
    public virtual IOperationModel DefaultActiveModel => new OperationModel (this);

    /// <summary>
    /// Estimated machining duration
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual TimeSpan? MachiningDuration
    {
      get { return m_machiningDuration; }
      set { m_machiningDuration = value; }
    }

    /// <summary>
    /// Estimated machining duration as string
    /// </summary>
    [XmlAttribute ("MachiningDuration")]
    public virtual string MachiningDurationAsString
    {
      get {
        return (this.MachiningDuration.HasValue)
          ? this.MachiningDuration.Value.ToString ()
          : null;
      }
      set {
        this.MachiningDuration = ConvertToNullableTimeSpan (value);
      }
    }

    /// <summary>
    /// Estimated machining hours
    /// </summary>
    [XmlAttribute ("MachiningHours")]
    public virtual double MachiningHours
    {
      get {
        return (this.MachiningDuration.HasValue)
          ? this.MachiningDuration.Value.TotalHours
          : 0;
      }
      set {
        if (0 < value) {
          this.MachiningDuration = TimeSpan.FromHours (value);
        }
        else {
          this.MachiningDuration = null;
        }
      }
    }

    /// <summary>
    /// Estimated setup duration
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual TimeSpan? SetUpDuration
    {
      get { return m_setUpDuration; }
      set { m_setUpDuration = value; }
    }

    /// <summary>
    /// Estimated setup duration as string
    /// </summary>
    [XmlAttribute ("SetUpDuration")]
    public virtual string SetUpDurationAsString
    {
      get {
        return (this.SetUpDuration.HasValue)
          ? this.SetUpDuration.Value.ToString ()
          : null;
      }
      set {
        this.SetUpDuration = ConvertToNullableTimeSpan (value);
      }
    }

    /// <summary>
    /// Estimated set up hours
    /// </summary>
    [XmlAttribute ("SetUpHours")]
    public virtual double SetUpHours
    {
      get {
        return (this.SetUpDuration.HasValue)
          ? this.SetUpDuration.Value.TotalHours
          : 0;
      }
      set {
        if (0 < value) {
          this.SetUpDuration = TimeSpan.FromHours (value);
        }
        else {
          this.SetUpDuration = null;
        }
      }
    }

    /// <summary>
    /// Estimated tear down duration
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual TimeSpan? TearDownDuration
    {
      get { return m_tearDownDuration; }
      set { m_tearDownDuration = value; }
    }

    /// <summary>
    /// Estimated tear down duration as string
    /// </summary>
    [XmlAttribute ("TearDownDuration")]
    public virtual string TearDownDurationAsString
    {
      get {
        return (this.TearDownDuration.HasValue)
          ? this.TearDownDuration.Value.ToString ()
          : null;
      }
      set {
        this.TearDownDuration = ConvertToNullableTimeSpan (value);
      }
    }

    /// <summary>
    /// Estimated tear down hours
    /// </summary>
    [XmlAttribute ("TearDownHours")]
    public virtual double TearDownHours
    {
      get {
        return (this.TearDownDuration.HasValue)
          ? this.TearDownDuration.Value.TotalHours
          : 0;
      }
      set {
        if (0 < value) {
          this.TearDownDuration = TimeSpan.FromHours (value);
        }
        else {
          this.TearDownDuration = null;
        }
      }
    }

    /// <summary>
    /// Estimated loading duration
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual TimeSpan? LoadingDuration
    {
      get { return m_loadingDuration; }
      set { m_loadingDuration = value; }
    }

    /// <summary>
    /// Estimated loading duration as string
    /// </summary>
    [XmlAttribute ("LoadingDuration")]
    public virtual string LoadingDurationAsString
    {
      get {
        return (this.LoadingDuration.HasValue)
          ? this.LoadingDuration.Value.ToString ()
          : null;
      }
      set {
        this.LoadingDuration = ConvertToNullableTimeSpan (value);
      }
    }

    /// <summary>
    /// Estimated loading hours
    /// </summary>
    [XmlAttribute ("LoadingHours")]
    public virtual double LoadingHours
    {
      get {
        return (this.LoadingDuration.HasValue)
          ? this.LoadingDuration.Value.TotalHours
          : 0;
      }
      set {
        if (0 < value) {
          this.LoadingDuration = TimeSpan.FromHours (value);
        }
        else {
          this.LoadingDuration = null;
        }
      }
    }

    /// <summary>
    /// Estimated unloading duration
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual TimeSpan? UnloadingDuration
    {
      get { return m_unloadingDuration; }
      set { m_unloadingDuration = value; }
    }

    /// <summary>
    /// Estimated unloading duration as string
    /// </summary>
    [XmlAttribute ("UnloadingDuration")]
    public virtual string UnloadingDurationAsString
    {
      get {
        return (this.UnloadingDuration.HasValue)
          ? this.UnloadingDuration.Value.ToString ()
          : null;
      }
      set {
        this.UnloadingDuration = ConvertToNullableTimeSpan (value);
      }
    }

    /// <summary>
    /// Estimated unloading hours
    /// </summary>
    [XmlAttribute ("UnloadingHours")]
    public virtual double UnloadingHours
    {
      get {
        return (this.UnloadingDuration.HasValue)
          ? this.UnloadingDuration.Value.TotalHours
          : 0;
      }
      set {
        if (0 < value) {
          this.UnloadingDuration = TimeSpan.FromHours (value);
        }
        else {
          this.UnloadingDuration = null;
        }
      }
    }

    /// <summary>
    /// Creation date/time
    /// </summary>
    [XmlIgnore]
    public virtual DateTime CreationDateTime => m_creationDateTime;

    /// <summary>
    /// Lock the operation for auto-update
    /// </summary>
    [XmlIgnore]
    public virtual bool Lock
    {
      get { return m_lock; }
      set { m_lock = value; }
    }

    /// <summary>
    /// Associated machine filter
    /// </summary>
    [XmlIgnore]
    public virtual IMachineFilter MachineFilter
    {
      get { return m_machineFilter; }
      set { m_machineFilter = value; }
    }

    /// <summary>
    /// Set of intermediate work pieces this operation makes
    /// </summary>
    [XmlIgnore, MergeChildren ("Operation")]
    public virtual ICollection<IIntermediateWorkPiece> IntermediateWorkPieces
    {
      get {
        if (null == m_intermediateWorkPieces) {
          m_intermediateWorkPieces = new List<IIntermediateWorkPiece> ();
        }
        return m_intermediateWorkPieces;
      }
    }

    /// <summary>
    /// Set of intermediate work pieces that are need for this operation
    /// </summary>
    [XmlIgnore]
    public virtual ICollection<IIntermediateWorkPiece> Sources
    {
      get {
        return m_sources;
      }
    }

    /// <summary>
    /// Set of paths that are associated to this operation
    /// </summary>
    // do not merge automatically (collection implemented as a sorted set on path number
    // and there are issues if inserting a path with an already existing key)
    //[XmlIgnore, MergeChildren("Operation")]
    public virtual ICollection<IPath> Paths
    {
      get {
        return m_paths;
      }
    }


    /// <summary>
    /// Set of sequences that are associated to this operation
    /// </summary>
    [XmlIgnore, MergeChildren ("Operation")]
    //[XmlIgnore]
    public virtual ICollection<ISequence> Sequences
    {
      get {
        return m_sequences;
      }
    }

    /// <summary>
    /// List of stamps (and then ISO files) that are associated to this operation
    /// </summary>
    [XmlIgnore, MergeChildren ("Operation")]
    public virtual ICollection<IStamp> Stamps
    {
      get {
        return m_stamps;
      }
    }

    /// <summary>
    /// Return a value if the operation has been archived
    /// </summary>
    [XmlIgnore]
    public virtual DateTime? ArchiveDateTime
    {
      get { return m_archiveDateTime; }
      set { m_archiveDateTime = value; }
    }

    /// <summary>
    /// Durations
    /// </summary>
    public virtual ICollection<IOperationDuration> Durations => new List<IOperationDuration> { new OperationDuration (this) };
    #endregion // Getters / Setters

    #region Contructors
    /// <summary>
    /// The default constructor is forbidden
    /// and is only used by NHibernate
    /// </summary>
    protected Operation ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="operationType"></param>
    internal protected Operation (IOperationType operationType)
    {
      m_type = operationType;
    }
    #endregion // Constructors

    #region Add methods
    /// <summary>
    /// Add an intermediate work piece
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    public virtual void AddIntermediateWorkPiece (IIntermediateWorkPiece intermediateWorkPiece)
    {
      intermediateWorkPiece.Operation = this; // Everyting is done in the setter
    }

    /// <summary>
    /// Add a source work piece
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    public virtual void AddSource (IIntermediateWorkPiece intermediateWorkPiece)
    {
      Sources.Add (intermediateWorkPiece);
      intermediateWorkPiece.PossibleNextOperations.Add (this);
    }

    /// <summary>
    /// Remove a source work piece
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    public virtual void RemoveSource (IIntermediateWorkPiece intermediateWorkPiece)
    {
      Sources.Remove (intermediateWorkPiece);
      intermediateWorkPiece.PossibleNextOperations.Remove (this);
    }

    /// <summary>
    /// Remove a path
    /// </summary>
    /// <param name="path"></param>
    public virtual void RemovePath (IPath path)
    {
      if (!this.Paths.Contains (path)) {
        log.WarnFormat ("RemovePath: " +
                        "Path {0} is not in {1}",
                        path, this);
      }
      else {
        RemovePathForInternalUse (path);
      }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Get the total number of intermediate work pieces
    /// </summary>
    /// <returns></returns>
    public virtual int GetTotalNumberOfIntermediateWorkPieces ()
    {
      int total = 0;
      foreach (IIntermediateWorkPiece intermediateWorkPiece in this.IntermediateWorkPieces) {
        total += intermediateWorkPiece.OperationQuantity;
      }
      return total;
    }

    /// <summary>
    /// Check if the operation is undefined
    /// 
    /// An operation is considered as undefined if it has no name and no given type
    /// </summary>
    /// <returns></returns>
    public virtual bool IsUndefined ()
    {
      return ((this.Name == null)
              || (0 == this.Name.Length))
        && (1 == this.Type.Id);
    }

    /// <summary>
    /// <see cref="IMergeable&lt;T&gt;.Merge" />
    /// </summary>
    /// <param name="other"></param>
    /// <param name="conflictResolution"></param>
    public virtual void Merge (IOperation other,
                               ConflictResolution conflictResolution)
    {
      Mergeable.MergeAuto (this, other, conflictResolution);
      // There is no need to update the quantity here
      ModifyItems<IIntermediateWorkPiece>
        (other.Sources, new Modifier<IIntermediateWorkPiece>
         (delegate (IIntermediateWorkPiece item) {
           this.AddSource (item);
           other.RemoveSource (item);
         }));
      // UNDONE: source / quantity

      var singlePath = Lemoine.Info.ConfigSet
        .Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.SinglePath));
      if (singlePath) {
        // check if merged sequence has an associated path
        IPath otherPath = null;
        if (other.Paths.Count > 0) {
          // there  should be only one
          otherPath = other.Paths.ToList ()[0];
        }

        if (otherPath != null) {
          if (this.Paths.Count == 0) {
            // simply transfer path
            otherPath.Operation = this;
          }
          else {
            if (otherPath.Sequences != null) {
              // transfer sequences of otherPath to (end of) path:
              // may also need to reassign sequence order.

              IPath path = this.Paths.ToList ()[0];
              int maxSequenceOrder = 0;
              if ((path.Sequences != null) && (path.Sequences.Count > 0)) {
                maxSequenceOrder = 1 + path.Sequences.Last<ISequence> ().Order;
              }

              // clone collection to prevent iterator/collection change issue
              ICollection<ISequence> otherSequences = new List<ISequence> ();
              foreach (ISequence sequence in otherPath.Sequences) {
                otherSequences.Add (sequence);
              }

              foreach (ISequence sequence in otherSequences) {
                sequence.Order += maxSequenceOrder;
                sequence.Operation = this;
                sequence.Path = path;
              }
            } // else no sequence to transfer
          }
        } // else no path to transfer

      }
      else {
        // multiple paths

        if (other.Paths.Count > 0) {
          // note that order of "other" paths is preserved but that their number will change
          IList<IPath> otherPaths = new List<IPath> (other.Paths.Count);

          // merge paths (add those of "other" after those of "this")
          // awkward but sure way to get max path number
          int maxPathNumber = 0;
          foreach (IPath path in this.Paths) {
            maxPathNumber = System.Math.Max (maxPathNumber, path.Number);
          }


          // clone collection to prevent iterator/collection change issue
          foreach (IPath path in other.Paths) {
            otherPaths.Add (path);
          }

          // reassign number and parent operation for "other" paths
          foreach (IPath path in otherPaths) {
            path.Number = ++maxPathNumber;
            path.Operation = this;
          }

          // sequences were dealt with when reassigning paths: nothing more to do
        }
      }

    }

    /// <summary>
    /// Add an intermediate work piece in the member directly
    /// 
    /// To be used by the IntermediateWorkPiece class only
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    protected internal virtual void AddIntermediateWorkPieceForInternalUse (IIntermediateWorkPiece intermediateWorkPiece)
    {
      AddToProxyCollection<IIntermediateWorkPiece> (m_intermediateWorkPieces, intermediateWorkPiece);
    }

    /// <summary>
    /// Remove an intermediate work piece in the member directly
    /// 
    /// To be used by the IntermediateWorkPiece class only
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    protected internal virtual void RemoveIntermediateWorkPieceForInternalUse (IIntermediateWorkPiece intermediateWorkPiece)
    {
      RemoveFromProxyCollection<IIntermediateWorkPiece> (m_intermediateWorkPieces, intermediateWorkPiece);
    }

    /// <summary>
    /// Add a sequence in the member directly
    /// 
    /// To be used by the Sequence class only
    /// </summary>
    /// <param name="sequence"></param>
    protected internal virtual void AddSequenceForInternalUse (ISequence sequence)
    {
      AddToProxyCollection<ISequence> (m_sequences, sequence);
    }

    /// <summary>
    /// Add a path in the member directly
    /// 
    /// To be used by the Path class only
    /// </summary>
    /// <param name="path"></param>
    protected internal virtual void AddPathForInternalUse (IPath path)
    {
      AddToProxyCollection<IPath> (m_paths, path);
      foreach (ISequence sequence in path.Sequences) {
        this.AddSequenceForInternalUse (sequence);
      }
    }

    /// <summary>
    /// Remove a sequence in the member directly
    /// 
    /// To be used by the Sequence class only
    /// </summary>
    /// <param name="sequence"></param>
    protected internal virtual void RemoveSequenceForInternalUse (ISequence sequence)
    {
      RemoveFromProxyCollection<ISequence> (m_sequences, sequence);
    }

    /// <summary>
    /// Remove a path in the member directly
    /// 
    /// To be used by the Path class only
    /// </summary>
    /// <param name="path"></param>
    protected internal virtual void RemovePathForInternalUse (IPath path)
    {
      RemoveFromProxyCollection<IPath> (m_paths, path);
      foreach (ISequence sequence in path.Sequences) {
        this.RemoveSequenceForInternalUse (sequence);
      }
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

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IOperationType> (ref m_type);
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[Operation {this.Id} Name={this.Name}]";
      }
      else {
        return $"[Operation {this.Id}]";
      }
    }

    /// <summary>
    /// Indicates whether the current object
    /// is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (IOperation other)
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
      Operation other = obj as Operation;
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
