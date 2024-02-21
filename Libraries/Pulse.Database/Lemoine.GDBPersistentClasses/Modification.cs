// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;

using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using Lemoine.Business.Config;
using Lemoine.Collections;
using Pulse.Extensions.Database;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table Modification
  /// </summary>
  [Serializable,
   XmlInclude (typeof (MachineModification)),
   XmlInclude (typeof (GlobalModification))]
  public abstract class Modification : IModification, Lemoine.Threading.IChecked, Lemoine.Collections.IDataWithId<long>
  {
    /// <summary>
    /// Percentage of the step timeout with which is compared the last modification analysis duration
    /// to determine if the step span should be increased
    /// </summary>
    const string INCREASE_STEP_SPAN_THRESHOLD_KEY = "Analysis.StepSpan.Increase.Threshold";
    const double INCREASE_STEP_SPAN_THRESHOLD_DEFAULT = 0.70; // 70 %

    /// <summary>
    /// Rate to use to increase the step span in case of a good analysis duration
    /// </summary>
    const string INCREASE_STEP_SPAN_RATE_KEY = "Analysis.StepSpan.Increase.Rate";
    const double INCREASE_STEP_SPAN_RATE_DEFAULT = 1.2; // +20 %

    /// <summary>
    /// Pending priority. If negative or 0, it is considered as an offset from the main Priority
    /// </summary>
    const string PENDING_PRIORITY_KEY = "Analysis.PendingPriority";
    static readonly int PENDING_PRIORITY_DEFAULT = -1;

    /// <summary>
    /// In progress priority. If negative or 0, it is considered as an offset from the main Priority
    /// </summary>
    const string IN_PROGRESS_PRIORITY_KEY = "Analysis.InProgressPriority";
    static readonly int IN_PROGRESS_PRIORITY_DEFAULT = -1;

    #region Members
    // disable once ConvertToConstant.Local
    long m_id = 0;
    /// <summary>
    /// Should this modification be run by the activity analysis thread ?
    /// By default, it is run by the modification thread
    /// </summary>
    protected bool m_activityAnalysis = false;
    /// <summary>
    /// Set of sub global modifications
    /// </summary>
    protected ICollection<IGlobalModification> m_subGlobalModifications = new List<IGlobalModification> ();
    /// <summary>
    /// Set of sub machine modifications
    /// </summary>
    protected ICollection<IMachineModification> m_subMachineModifications = new List<IMachineModification> ();
    DateTime m_dateTime = System.DateTime.UtcNow;
    int m_priority = 100;
    protected int? m_statusPriority = null;
    /// <summary>
    /// Analysis status
    /// </summary>
    protected Lemoine.Model.AnalysisStatus m_analysisStatus = AnalysisStatus.New;
    /// <summary>
    /// Next analysis status (nullable)
    /// </summary>
    protected AnalysisStatus? m_nextAnalysisStatus = null;
    DateTime? m_analysisAppliedDateTime;
    bool m_auto = false;
    IModification m_mainModification; // Reference to the main modification that is used e.g. in the analysis logs
    DateTime? m_analysisBegin = null;
    DateTime? m_analysisEnd = null;
    DateTime m_lastAnalysisBegin = DateTime.UtcNow;
    int m_analysisIterations = 0;
    TimeSpan m_analysisTotalDuration = TimeSpan.FromSeconds (0);
    TimeSpan? m_analysisLastDuration = null;
    long? m_analysisCompletionOrder = null;
    TimeSpan? m_analysisStepSpan = null;
    /// <summary>
    /// Is there any sub global modification ?
    /// </summary>
    protected bool m_analysisSubGlobalModifications = false;
    /// <summary>
    /// Is there any sub machine modification ?
    /// </summary>
    protected bool m_analysisSubMachineModifications = false;
    Lemoine.Threading.IChecked m_caller = null;

    IEnumerable<IModificationExtension> m_extensions = null;
    #endregion

    static readonly ILog log = LogManager.GetLogger (typeof (Modification).FullName);

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public abstract string ModificationType
    {
      get;
    }

    /// <summary>
    /// Modification ID
    /// </summary>
    [XmlIgnore]
    public virtual long Id
    {
      get { return this.m_id; }
      internal protected set { m_id = value; }
    }

    /// <summary>
    /// Associated revision
    /// 
    /// Note: if the modification is not persistent yet and the revision is, you may get problems
    /// </summary>
    [XmlIgnore]
    public abstract IRevision Revision { get; set; }

    /// <summary>
    /// Parent modification (machine or global) when applicable
    /// </summary>
    [XmlIgnore]
    public virtual IModification Parent
    {
      get
      {
        if (null != ParentGlobal) {
          return ParentGlobal;
        }
        else if (null != ParentMachine) {
          return ParentMachine;
        }
        else {
          return null;
        }
      }
      set
      {
        if (null == value) {
          this.ParentMachine = null;
          this.ParentGlobal = null;
        }
        else if (value is IMachineModification) {
          this.ParentMachine = (IMachineModification)value;
          this.ParentGlobal = null;
        }
        else if (value is IGlobalModification) {
          this.ParentMachine = null;
          this.ParentGlobal = (IGlobalModification)value;
        }
        else {
          log.FatalFormat ("Parent.set: " +
                           "modification {0} is neither a global nor a machine modification " +
                           "=> fall",
                           value);
          Debug.Assert (false);
          this.ParentMachine = null;
          this.ParentGlobal = null;
        }
      }
    }

    /// <summary>
    /// Parent global modification when applicable
    /// </summary>
    [XmlIgnore]
    public abstract IGlobalModification ParentGlobal { get; set; }

    /// <summary>
    /// Parent machine modification when applicable
    /// </summary>
    [XmlIgnore]
    public abstract IMachineModification ParentMachine { get; set; }

    /// <summary>
    /// Should the modification run by the Activity analysis thread ?
    /// </summary>
    [XmlIgnore]
    public virtual bool ActivityAnalysis
    {
      get { return m_activityAnalysis; }
      set { m_activityAnalysis = value; }
    }

    /// <summary>
    /// Sub-globalmodifications when applicable
    /// </summary>
    [XmlIgnore]
    public virtual ICollection<IGlobalModification> SubGlobalModifications
    {
      get
      {
        return m_subGlobalModifications;
      }
    }

    /// <summary>
    /// Sub-machinemodifications when applicable
    /// </summary>
    [XmlIgnore]
    public virtual ICollection<IMachineModification> SubMachineModifications
    {
      get
      {
        return m_subMachineModifications;
      }
    }

    /// <summary>
    /// Sub-modifications when applicable (global or machine)
    /// </summary>
    [XmlIgnore]
    public virtual IEnumerable<IModification> SubModifications
    {
      get
      {
        return m_subGlobalModifications.Cast<IModification> ().Concat (m_subMachineModifications.Cast<IModification> ());
      }
    }

    /// <summary>
    /// Date/time of the modification
    /// </summary>
    [XmlIgnore]
    public virtual DateTime DateTime
    {
      get { return m_dateTime; }
      set { m_dateTime = value; }
    }

    /// <summary>
    /// Date/time of the modification in SQL string for XML serialization
    /// </summary>
    [XmlAttribute ("DateTime")]
    public virtual string SqlDateTime
    {
      get
      {
        return this.DateTime.ToString ("yyyy-MM-dd HH:mm:ss");
      }
      set
      {
        if ((null == value) || (0 == value.Length)) {
          this.DateTime = System.DateTime.UtcNow;
        }
        else {
          this.DateTime = System.DateTime.Parse (value);
        }
      }
    }

    /// <summary>
    /// Priority of the modification (w.r.t. other modifications)
    /// high value = high priority
    /// </summary>
    [XmlAttribute ("Priority")]
    public virtual int Priority
    {
      get { return m_priority; }
      set
      {
        m_priority = value;
        if (!m_statusPriority.HasValue) {
          this.StatusPriority = value;
        }
      }
    }

    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public virtual int StatusPriority
    {
      get { return m_statusPriority ?? this.Priority; }
      set { m_statusPriority = value; }
    }

    /// <summary>
    /// Status of the analysis
    /// </summary>
    [XmlIgnore]
    public virtual Lemoine.Model.AnalysisStatus AnalysisStatus
    {
      get { return m_analysisStatus; }
    }

    /// <summary>
    /// In case of a parent modification, status to take once all the children are completed
    /// </summary>
    [XmlIgnore]
    public virtual Lemoine.Model.AnalysisStatus? NextAnalysisStatus
    {
      get { return m_nextAnalysisStatus; }
    }

    /// <summary>
    /// Date/time
    /// <item>on which the analysis applies</item>
    /// <item>or until when the analysis is done</item>
    /// </summary>
    [XmlIgnore]
    public virtual DateTime? AnalysisAppliedDateTime
    {
      get { return m_analysisAppliedDateTime; }
      set
      {
        if (value.HasValue && this.AnalysisStatus.Equals (AnalysisStatus.New)) {
          log.FatalFormat ("AnalysisAppliedDateTime.set: AnalysisAppliedDateTime {0} set on a new modification id={1}. StackTrace={2}",
            value, this.Id, System.Environment.StackTrace);
          Debug.Assert (false);
        }
        m_analysisAppliedDateTime = value;
      }
    }

    /// <summary>
    /// Auto modification
    /// </summary>
    [XmlIgnore]
    public virtual bool Auto
    {
      get { return m_auto; }
      set { m_auto = value; }
    }

    /// <summary>
    /// Main modification to be used as a reference in e.g. the analysis logs
    /// </summary>
    [XmlIgnore]
    internal protected virtual IModification MainModification
    {
      get { return m_mainModification; }
    }

    /// <summary>
    /// Begin date/time of the first analysis
    /// </summary>
    [XmlIgnore]
    public virtual DateTime? AnalysisBegin
    {
      get { return m_analysisBegin; }
      set { m_analysisBegin = value; }
    }

    /// <summary>
    /// End date/time of the last processed analysis
    /// </summary>
    [XmlIgnore]
    public virtual DateTime? AnalysisEnd
    {
      get { return m_analysisEnd; }
      set { m_analysisEnd = value; }
    }

    /// <summary>
    /// Begin date/time of the last analysis
    /// </summary>
    [XmlIgnore]
    public virtual DateTime LastAnalysisBegin
    {
      get { return m_lastAnalysisBegin; }
    }

    /// <summary>
    /// Number of analysis iterations
    /// </summary>
    [XmlIgnore]
    public virtual int AnalysisIterations
    {
      get { return m_analysisIterations; }
      set { m_analysisIterations = value; }
    }

    /// <summary>
    /// Total process time for this modification
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan AnalysisTotalDuration
    {
      get { return m_analysisTotalDuration; }
      set { m_analysisTotalDuration = value; }
    }

    /// <summary>
    /// Process time of the last iteration
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan? AnalysisLastDuration
    {
      get { return m_analysisLastDuration; }
      set { m_analysisLastDuration = value; }
    }

    /// <summary>
    /// Analysis completion order
    /// </summary>
    [XmlIgnore]
    public virtual long? AnalysisCompletionOrder
    {
      get { return m_analysisCompletionOrder; }
      set { m_analysisCompletionOrder = value; }
    }

    /// <summary>
    /// Analysis step span
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan? AnalysisStepSpan
    {
      get { return m_analysisStepSpan; }
      set { m_analysisStepSpan = value; }
    }

    /// <summary>
    /// Is there any sub-modifications ? Global or Machine
    /// </summary>
    [XmlIgnore]
    public virtual bool AnalysisSubModifications
    {
      get { return this.AnalysisSubGlobalModifications || this.AnalysisSubMachineModifications; }
    }

    /// <summary>
    /// Is there any sub-globalmodifications ?
    /// </summary>
    [XmlIgnore]
    public virtual bool AnalysisSubGlobalModifications
    {
      get { return m_analysisSubGlobalModifications; }
      protected set { m_analysisSubGlobalModifications = value; }
    }

    /// <summary>
    /// Is there any sub-machinemodifications ?
    /// </summary>
    [XmlIgnore]
    public virtual bool AnalysisSubMachineModifications
    {
      get { return m_analysisSubMachineModifications; }
      protected set { m_analysisSubMachineModifications = value; }
    }

    /// <summary>
    /// Add a caller to this class to correctly redirect the SetActive calls
    /// </summary>
    [XmlIgnore]
    public virtual Lemoine.Threading.IChecked Caller
    {
      get { return m_caller; }
      set { m_caller = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    protected Modification ()
    {
      m_mainModification = this;
    }

    /// <summary>
    /// Constructor for abstract modifications, that are kept transient
    /// 
    /// You may add a mainModification reference to be used in the analysis logs
    /// 
    /// Not to log any analysis log, mainModification may be null
    /// </summary>
    /// <param name="mainModification"></param>
    protected Modification (IModification mainModification)
    {
      m_mainModification = mainModification;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add an item to a proxy collection:
    /// <item>Try to initialize it first</item>
    /// <item>Do nothing if it could not be initialized</item>
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="item"></param>
    protected static void AddToProxyCollection<T> (ICollection<T> collection, T item)
    {
      try {
        NHibernateUtil.Initialize (collection);
      }
      catch (Exception) {
      }

      if ((null != collection)
          && NHibernateUtil.IsInitialized (collection)) {
        collection.Add (item);
      }
    }

    /// <summary>
    /// Remove an item from a proxy collection
    /// <item>Try to initialize it first</item>
    /// <item>Do nothing if it could not be initialized</item>
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="item"></param>
    protected static void RemoveFromProxyCollection<T> (ICollection<T> collection, T item)
    {
      try {
        NHibernateUtil.Initialize (collection);
      }
      catch (Exception) {
      }

      if ((null != collection)
          && NHibernateUtil.IsInitialized (collection)) {
        collection.Remove (item);
      }
    }

    /// <summary>
    /// Add a sub-globalmodification in the member directly
    /// 
    /// To be used by the parent Modification class only
    /// </summary>
    /// <param name="subModification"></param>
    protected internal virtual void AddSubModificationForInternalUse (IGlobalModification subModification)
    {
      AddToProxyCollection<IGlobalModification> (m_subGlobalModifications, subModification);
      m_analysisSubGlobalModifications = true;
    }

    /// <summary>
    /// Add a sub-machinemodification in the member directly
    /// 
    /// To be used by the parent Modification class only
    /// </summary>
    /// <param name="subModification"></param>
    protected internal virtual void AddSubModificationForInternalUse (IMachineModification subModification)
    {
      AddToProxyCollection<IMachineModification> (m_subMachineModifications, subModification);
      m_analysisSubMachineModifications = true;
    }

    /// <summary>
    /// Remove a sub-globalmodification in the member directly
    /// 
    /// To be used by the parent Modification class only
    /// </summary>
    /// <param name="subModification"></param>
    protected internal virtual void RemoveSubModificationForInternalUse (IGlobalModification subModification)
    {
      RemoveFromProxyCollection<IGlobalModification> (m_subGlobalModifications, subModification);
      if (0 == m_subGlobalModifications.Count) {
        m_analysisSubGlobalModifications = false;
      }
    }

    /// <summary>
    /// Remove a sub-machinemodification in the member directly
    /// 
    /// To be used by the parent Modification class only
    /// </summary>
    /// <param name="subModification"></param>
    protected internal virtual void RemoveSubModificationForInternalUse (IMachineModification subModification)
    {
      RemoveFromProxyCollection<IMachineModification> (m_subMachineModifications, subModification);
      if (0 == m_subMachineModifications.Count) {
        m_analysisSubMachineModifications = false;
      }
    }

    /// <summary>
    /// Get the modification extensions
    /// </summary>
    /// <returns></returns>
    IEnumerable<IModificationExtension> GetExtensions ()
    {
      if (null == m_extensions) {
        var request = new Lemoine.Business.Extension
          .GlobalExtensions<IModificationExtension> ();
        m_extensions = Lemoine.Business.ServiceProvider
          .Get (request);
      }
      return m_extensions;
    }

    void UpdateStatus (AnalysisStatus newStatus, int? newPriority = null)
    {
      var oldStatus = this.AnalysisStatus;
      if (!object.Equals (newStatus, oldStatus)) {
        m_analysisStatus = newStatus;
        if (newPriority.HasValue) {
          this.StatusPriority = newPriority.Value;
        }

        foreach (var extension in GetExtensions ()) {
          extension.NotifyAnalysisStatusChange (this, oldStatus, newStatus);
        }
      }
      else if (newPriority.HasValue && (this.StatusPriority != newPriority.Value)) {
        this.StatusPriority = newPriority.Value;
      }
    }

    /// <summary>
    /// Restore the status after a transaction failure
    /// </summary>
    /// <param name="oldStatus"></param>
    /// <param name="oldPriority"></param>
    protected internal virtual void RestoreStatus (AnalysisStatus oldStatus, int oldPriority)
    {
      if (!oldStatus.Equals (m_analysisStatus)) {
        m_analysisStatus = oldStatus;
      }
      if (oldPriority != this.StatusPriority) {
        this.StatusPriority = oldPriority;
      }
    }

    /// <summary>
    /// Initialize some parameters and make the analysis of this modification
    /// </summary>
    public virtual void RunAnalysis ()
    {
      log.DebugFormat ("RunAnalysis");
      m_lastAnalysisBegin = DateTime.UtcNow;
      if (this.AnalysisStatus.Equals (AnalysisStatus.Timeout) && CancelAfterTimeout ()) { // Switch to TimeoutCanceled
        MarkAsTimeoutCanceled ();
        return;
      }
      if (this.AnalysisStatus.Equals (AnalysisStatus.DatabaseTimeout) && CancelAfterTimeout ()) { // Switch to DatabaseTimeoutCanceled
        MarkAsDatabaseTimeoutCanceled ();
        return;
      }
      MakeAnalysis ();
    }

    /// <summary>
    /// In case the analysis status is Timeout, should the modification switch to the status TimeoutCanceled ?
    /// 
    /// Default is true, but it can be overriden.
    /// </summary>
    /// <returns></returns>
    public virtual bool CancelAfterTimeout ()
    {
      return true;
    }

    /// <summary>
    /// Cancelling the modification is required.
    /// 
    /// After an error, cancel any data that could have been set by the modification or its parent.
    /// The modification will be in error afterwards.
    /// 
    /// By default, switch all the children to ParentInError
    /// </summary>
    public virtual void Cancel ()
    {
      foreach (var subModification in this.SubModifications.Where (m => m.AnalysisStatus.IsNotCompleted ())) {
        subModification.MarkAsParentInError (true);
      }
    }

    /// <summary>
    /// Make the analysis of this modification
    /// </summary>
    public abstract void MakeAnalysis ();

    /// <summary>
    /// Add an analysis log for this modification
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    protected virtual void AddAnalysisLog (LogLevel level,
                                           string message)
    {
      ModelDAOHelper.DAOFactory.AnalysisLogDAO.Add (this, level, message);
    }

    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (IModification other)
    {
      if (object.ReferenceEquals (this, other)) {
        return true;
      }

      if (other == null) {
        return false;
      }

      if (this.Id != 0) {
        return (((Lemoine.Collections.IDataWithId<long>)other).Id == ((Lemoine.Collections.IDataWithId<long>)this).Id);
      }

      return false;
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
      IModification other = obj as Modification;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (((Lemoine.Collections.IDataWithId<long>)other).Id == ((Lemoine.Collections.IDataWithId<long>)this).Id);
      }

      return false;
    }

    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode ()
    {
      return m_id.GetHashCode ();
    }

    /// <summary>
    /// Apply directly the modification,
    /// while keeping the transient status of the modification
    /// </summary>
    public abstract void Apply ();
    #endregion // Methods

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public abstract void Unproxy ();

    #region IModification implementation
    /// <summary>
    /// Get the impacted activity analysis
    /// so that the activity analysis makes a pause
    /// 
    /// null if there is no impacted activity analysis
    /// </summary>
    /// <returns></returns>
    public virtual IList<IMachine> GetImpactedActivityAnalysis ()
    {
      return null;
    }
    #endregion // IModification implementation

    #region IChecked implementation
    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked">IChecked implementation</see>
    /// </summary>
    public virtual void SetActive ()
    {
      m_caller?.SetActive ();

      if (null != this.MainModification) {
        this.MainModification.CheckStepTimeout ();
      }
      else {
        CheckStepTimeout ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public virtual void PauseCheck ()
    {
      m_caller?.PauseCheck ();
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public virtual void ResumeCheck ()
    {
      m_caller?.ResumeCheck ();
    }
    #endregion // IChecked implementation

    #region MarkXxx: set a new analysis status
    void SetCompletionOrder ()
    {
      if (0 != this.Id) {
        this.AnalysisCompletionOrder = ModelDAOHelper.DAOFactory.ModificationDAO.GetNextCompletionOrder ();
      }
    }

    /// <summary>
    /// Is the analysis completed for the analysis service?
    /// </summary>
    /// <returns></returns>
    protected bool IsAnalysisCompleted ()
    {
      switch (this.AnalysisStatus) {
      case AnalysisStatus.Done:
      case AnalysisStatus.Obsolete:
      case AnalysisStatus.PendingSubModifications:
      case AnalysisStatus.Error:
      case AnalysisStatus.ConstraintIntegrityViolation:
      case AnalysisStatus.Delete:
      case AnalysisStatus.Cancel:
      case AnalysisStatus.DonePurge:
      case AnalysisStatus.NotApplicable:
      case AnalysisStatus.AncestorNotApplicable:
      case AnalysisStatus.AncestorError:
      case AnalysisStatus.TimeoutCanceled:
      case AnalysisStatus.ParentInError:
      case AnalysisStatus.ChildInError:
      case AnalysisStatus.DatabaseTimeoutCanceled:
        return true;
      default:
        return false;
      }
    }

    void CheckIsAnalysisNotCompleted ()
    {
      if (IsAnalysisCompleted () && !this.AnalysisStatus.Equals (AnalysisStatus.PendingSubModifications)) {
        log.Fatal ($"CheckIsAnalysisNotCompleted: the modification analysis is already completed status={this.AnalysisStatus} and there is an attempt to update the analysis status. StackTrace={System.Environment.StackTrace}");
        Debug.Assert (!IsAnalysisCompleted ());
      }
    }

    /// <summary>
    /// Mark the modification as Done, even if there are some modifications
    /// </summary>
    /// <param name="message">Message to send to the web service</param>
    protected virtual void ForceAsDone (string message)
    {
      CheckIsAnalysisNotCompleted ();

      if (m_auto) {
        UpdateStatus (AnalysisStatus.DonePurge);
      }
      else {
        UpdateStatus (AnalysisStatus.Done);
      }
      this.AnalysisAppliedDateTime = null;
      SetCompletionOrder ();

      if (!string.IsNullOrEmpty (message)) {
        AnalysisAccumulator.PushMessage (message);
      }
    }

    /// <summary>
    /// Mark the modification as completed (Done or SubModifications)
    /// </summary>
    /// <param name="message">Message to send to the web service</param>
    protected virtual void MarkAsCompleted (string message)
    {
      CheckIsAnalysisNotCompleted ();

      if (this.AnalysisSubGlobalModifications || this.AnalysisSubMachineModifications) {
        if (m_auto) {
          m_nextAnalysisStatus = AnalysisStatus.DonePurge;
        }
        else {
          m_nextAnalysisStatus = AnalysisStatus.Done;
        }
        int newPriority = this.StatusPriority;
        if (this.AnalysisSubMachineModifications) {
          var notCompletedSubMachineModifications = this.SubMachineModifications
            .Where (m => m.AnalysisStatus.IsNotCompleted ());
          if (notCompletedSubMachineModifications.Any ()) {
            newPriority = notCompletedSubMachineModifications
              .Max (m => m.StatusPriority);
          }
        }
        UpdateStatus (AnalysisStatus.PendingSubModifications, newPriority);
      }
      else {
        if (m_auto) {
          UpdateStatus (AnalysisStatus.DonePurge);
        }
        else {
          UpdateStatus (AnalysisStatus.Done);
        }
      }
      this.AnalysisAppliedDateTime = null;
      SetCompletionOrder ();

      if (!string.IsNullOrEmpty (message)) {
        AnalysisAccumulator.PushMessage (message);
      }
    }

    /// <summary>
    /// Mark the modification as completed (Done) or partially completed (InProgress)
    /// </summary>
    /// <param name="message">Message to send to the web service (nullable)</param>
    /// <param name="effectiveEnd"></param>
    protected virtual void MarkAsCompleted (string message, DateTime? effectiveEnd)
    {
      MarkAsCompleted (message);
      this.AnalysisAppliedDateTime = effectiveEnd;
    }

    /// <summary>
    /// Mark the modification as not applicable
    /// </summary>
    protected virtual void MarkAsNotApplicable ()
    {
      CheckIsAnalysisNotCompleted ();

      Cancel ();

      UpdateStatus (AnalysisStatus.NotApplicable);
      SetCompletionOrder ();
    }

    /// <summary>
    /// Mark the modification as Ancestor not applicable
    /// </summary>
    protected virtual void MarkAsAncestorNotApplicable ()
    {
      CheckIsAnalysisNotCompleted ();

      Cancel ();

      UpdateStatus (AnalysisStatus.AncestorNotApplicable);
      SetCompletionOrder ();
    }

    /// <summary>
    /// Mark the modification as in Error
    /// </summary>
    protected virtual void MarkAsError ()
    {
      CheckIsAnalysisNotCompleted ();

      Cancel ();

      if (this.AnalysisSubGlobalModifications || this.AnalysisSubMachineModifications) {
        m_nextAnalysisStatus = AnalysisStatus.Error;
        UpdateStatus (AnalysisStatus.PendingSubModifications);
      }
      else {
        UpdateStatus (AnalysisStatus.Error);
      }
      this.AnalysisAppliedDateTime = DateTime.UtcNow;
      SetCompletionOrder ();
    }

    /// <summary>
    /// Mark the modification in TimeoutCanceled
    /// </summary>
    protected virtual void MarkAsTimeoutCanceled ()
    {
      CheckIsAnalysisNotCompleted ();

      Cancel ();

      if (this.AnalysisSubGlobalModifications || this.AnalysisSubMachineModifications) {
        m_nextAnalysisStatus = AnalysisStatus.TimeoutCanceled;
        UpdateStatus (AnalysisStatus.PendingSubModifications);
      }
      else {
        UpdateStatus (AnalysisStatus.TimeoutCanceled);
      }
      this.AnalysisAppliedDateTime = DateTime.UtcNow;
      SetCompletionOrder ();
    }

    /// <summary>
    /// Mark the modification in DatabaseTimeoutCanceled
    /// </summary>
    protected virtual void MarkAsDatabaseTimeoutCanceled ()
    {
      CheckIsAnalysisNotCompleted ();

      Cancel ();

      if (this.AnalysisSubGlobalModifications || this.AnalysisSubMachineModifications) {
        m_nextAnalysisStatus = AnalysisStatus.DatabaseTimeoutCanceled;
        UpdateStatus (AnalysisStatus.PendingSubModifications);
      }
      else {
        UpdateStatus (AnalysisStatus.DatabaseTimeoutCanceled);
      }
      this.AnalysisAppliedDateTime = DateTime.UtcNow;
      SetCompletionOrder ();
    }

    /// <summary>
    /// Mark the modification as in ParentInError
    /// </summary>
    /// <param name="cancel"></param>
    public virtual void MarkAsParentInError (bool cancel)
    {
      CheckIsAnalysisNotCompleted ();

      if (cancel) {
        Cancel ();
      }

      UpdateStatus (AnalysisStatus.ParentInError);
      this.AnalysisAppliedDateTime = DateTime.UtcNow;
      SetCompletionOrder ();
    }

    /// <summary>
    /// Mark the modification as Pending
    /// </summary>
    /// <param name="effectiveEnd"></param>
    protected virtual void MarkAsPending (DateTime? effectiveEnd)
    {
      CheckIsAnalysisNotCompleted ();

      var newPriority = GetPendingPriority ();
      UpdateStatus (AnalysisStatus.Pending, newPriority);
      this.AnalysisAppliedDateTime = effectiveEnd;
    }

    /// <summary>
    /// Get a new pending priority
    /// </summary>
    /// <returns></returns>
    protected virtual int GetPendingPriority ()
    {
      var pendingPriority = Lemoine.Info.ConfigSet
        .LoadAndGet (PENDING_PRIORITY_KEY, PENDING_PRIORITY_DEFAULT);
      if (pendingPriority <= 0) {
        return this.Priority + pendingPriority;
      }
      else {
        return pendingPriority;
      }
    }

    /// <summary>
    /// Mark the modification as Obsolete
    /// </summary>
    protected virtual void MarkAsObsolete ()
    {
      CheckIsAnalysisNotCompleted ();

      UpdateStatus (AnalysisStatus.Obsolete);
      SetCompletionOrder ();
    }

    /// <summary>
    /// Mark the modification as InProgress
    /// </summary>
    /// <param name="effectiveEnd"></param>
    protected virtual void MarkAsInProgress (DateTime? effectiveEnd)
    {
      CheckIsAnalysisNotCompleted ();

      var newPriority = GetInProgressPriority ();
      UpdateStatus (AnalysisStatus.InProgress, newPriority);
      this.AnalysisAppliedDateTime = effectiveEnd;

      // if the last time is much better than step timeout, increase stepspan
      if (this.AnalysisStepSpan.HasValue) {
        TimeSpan lastDuration = DateTime.UtcNow.Subtract (this.LastAnalysisBegin);
        TimeSpan stepTimeout = AnalysisConfigHelper.ModificationStepTimeout;
        double threshold = (double)ConfigSet.LoadAndGet (INCREASE_STEP_SPAN_THRESHOLD_KEY,
                                                          INCREASE_STEP_SPAN_THRESHOLD_DEFAULT);
        if (lastDuration.TotalSeconds < stepTimeout.TotalSeconds * threshold) { // If better than 30 % by default
          double rate = (double)ConfigSet.LoadAndGet (INCREASE_STEP_SPAN_RATE_KEY,
                                                       INCREASE_STEP_SPAN_RATE_DEFAULT);
          log.InfoFormat ("MarkAsInProgress: " +
                          "increase the step span from {0} with rate {1}",
                          this.AnalysisStepSpan.Value, rate);
          this.AnalysisStepSpan = TimeSpan.FromSeconds (this.AnalysisStepSpan.Value.TotalSeconds * rate);
          ModelDAOHelper.DAOFactory.ModificationDAO.MakePersistent (this);
        }
      }
    }

    /// <summary>
    /// Keep the same priority in case of in-progress modifications by default
    /// </summary>
    /// <returns></returns>
    protected virtual int GetInProgressPriority ()
    {
      var inProgressPriority = Lemoine.Info.ConfigSet
        .LoadAndGet (IN_PROGRESS_PRIORITY_KEY, IN_PROGRESS_PRIORITY_DEFAULT);
      if (inProgressPriority <= 0) {
        return this.Priority + inProgressPriority;
      }
      else {
        return inProgressPriority;
      }
    }

    /// <summary>
    /// Mark the modification as Timeout
    /// 
    /// This is done outside the main process, this is why TotalDuration,
    /// LastDuration and AnalysisIterations are updated as well
    /// </summary>
    public virtual void MarkAsTimeout (DateTime startDateTime)
    {
      CheckIsAnalysisNotCompleted ();

      m_analysisStatus = AnalysisStatus.Timeout;
      SetCompletionOrder ();

      if (!this.AnalysisBegin.HasValue) {
        this.AnalysisBegin = startDateTime;
      }
      DateTime end = DateTime.UtcNow;
      TimeSpan duration = end.Subtract (startDateTime);
      this.AnalysisTotalDuration = this.AnalysisTotalDuration.Add (duration);
      this.AnalysisLastDuration = duration;
      ++this.AnalysisIterations;

      this.AnalysisEnd = end;
    }

    /// <summary>
    /// Check if the step timeout is reached.
    /// 
    /// If it is, raise the StepTimeoutException
    /// 
    /// It must be called by the main modification
    /// </summary>
    /// <returns></returns>
    public virtual void CheckStepTimeout ()
    {
      // By default, do nothing
    }

    /// <summary>
    /// Compute a new step span after a step timeout
    /// 
    /// By default it returns null.
    /// It must be overriden in case the step timeout is managed
    /// </summary>
    /// <returns></returns>
    protected virtual TimeSpan? ComputeNewStepSpan ()
    {
      return null;
    }

    /// <summary>
    /// Mark the modification as StepTimeout
    /// 
    /// This is done outside the main process, this is why TotalDuration,
    /// LastDuration and AnalysisIterations are updated as well
    /// </summary>
    public virtual void MarkAsStepTimeout ()
    {
      CheckIsAnalysisNotCompleted ();

      log.DebugFormat ("MarkAsStepTimeout");

      if (object.Equals (AnalysisStatus.StepTimeout, m_analysisStatus)) {
        log.DebugFormat ("MarkAsStepTimeout: " +
                         "already in StepTimeout state " +
                         "=> compute a new step span");

        // This step timeout was not small enough
        TimeSpan? newStepSpan = ComputeNewStepSpan ();
        if (newStepSpan.HasValue) {
          // Check the new value is smaller
          Debug.Assert (!this.AnalysisStepSpan.HasValue || (newStepSpan.Value < this.AnalysisStepSpan.Value));

          log.DebugFormat ("MarkAsStepTimeout: " +
                           "adjust the analysis step span from {0} to {1}",
                           this.AnalysisStepSpan, newStepSpan);
          this.AnalysisStepSpan = newStepSpan;
        }
      }
      else {
        UpdateStatus (AnalysisStatus.StepTimeout);
      }

      DateTime end = DateTime.UtcNow;
      TimeSpan duration = end.Subtract (this.LastAnalysisBegin);
      this.AnalysisTotalDuration = this.AnalysisTotalDuration.Add (duration);
      this.AnalysisLastDuration = duration;
      ++this.AnalysisIterations;

      if (!this.AnalysisBegin.HasValue) {
        this.AnalysisBegin = this.LastAnalysisBegin;
      }
      this.AnalysisEnd = end;
    }

    /// <summary>
    /// Mark the modification as Timeout
    /// </summary>
    public virtual void MarkAsConstraintIntegrityViolation ()
    {
      CheckIsAnalysisNotCompleted ();

      UpdateStatus (AnalysisStatus.ConstraintIntegrityViolation);
      this.AnalysisAppliedDateTime = null;
      SetCompletionOrder ();
    }

    /// <summary>
    /// Mark the modification as DatabaseTimeout
    /// </summary>
    public virtual void MarkAsDatabaseTimeout ()
    {
      CheckIsAnalysisNotCompleted ();

      UpdateStatus (AnalysisStatus.DatabaseTimeout);
      SetCompletionOrder ();
    }

    /// <summary>
    /// Mark all the sub-modifications were completed
    /// </summary>
    public virtual void MarkAllSubModificationsCompleted ()
    {
      if (this.AnalysisStatus.Equals (AnalysisStatus.Pending)) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("MarkAllSubModificationsCompleted: current status is pending, keep this status");
        }
        return;
      }

      Debug.Assert (AnalysisStatus.PendingSubModifications == this.AnalysisStatus);
      if ((AnalysisStatus.PendingSubModifications != this.AnalysisStatus)
        && (AnalysisStatus.New != this.AnalysisStatus)) {
        log.FatalFormat ("MarkAllSubModificationsCompleted: " +
                         "bad current analysis status {0} for modification id={1}",
                         this.AnalysisStatus, this.Id);
        throw new InvalidOperationException ();
      }

      if (this.NextAnalysisStatus.HasValue
        && !this.NextAnalysisStatus.Value.Equals (AnalysisStatus.Done)) {
        UpdateStatus (this.NextAnalysisStatus.Value);
      }
      else if (this.SubModifications.Any (m => m.AnalysisStatus.IsInError ())) {
        UpdateStatus (AnalysisStatus.ChildInError);
      }
      else if (this.SubModifications.Any (m => m.AnalysisStatus == AnalysisStatus.AncestorError)) {
        UpdateStatus (AnalysisStatus.AncestorError);
      }
      else if (this.SubModifications.Any (m => m.AnalysisStatus == AnalysisStatus.AncestorNotApplicable)) {
        UpdateStatus (AnalysisStatus.AncestorNotApplicable);
      }
      else if (this.NextAnalysisStatus.HasValue) {
        UpdateStatus (this.NextAnalysisStatus.Value);
      }
      else {
        if (m_auto) {
          UpdateStatus (AnalysisStatus.DonePurge);
        }
        else {
          UpdateStatus (AnalysisStatus.Done);
        }
      }
    }
    #endregion

    /// <summary>
    /// It the main modification transient ?
    /// 
    /// If false, then the analysis status may be updated and sub-modifications created.
    /// If true, the modifications must be processed directly.
    /// </summary>
    public virtual bool IsMainModificationTransient ()
    {
      return (null == this.MainModification) || (0 == ((IDataWithId<long>)this.MainModification).Id)
        || ((this.MainModification != this) && this.MainModification.IsMainModificationTransient ());
    }

    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    /// <returns></returns>
    public abstract string GetTransactionNameSuffix ();
  }
}
