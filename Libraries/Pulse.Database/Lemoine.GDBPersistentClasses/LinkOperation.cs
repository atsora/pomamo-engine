// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business.Config;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table LinkOperation
  /// 
  /// This new table stores each time an operation
  /// must be automatically assigned between a given date/time
  /// and an identified operation.
  /// 
  /// It does not represent the effective operations that assigned to a machine,
  /// but all the manual or automatic link operation processes
  /// that must be processed.
  /// 
  /// To know the current operation state of a machine,
  /// the table Operation Slot that is filled in by the Analyzer must be used.
  /// </summary>
  [Serializable]
  public class LinkOperation: MachineModification, ILinkOperation
  {
    static readonly string MAX_AGE_LAST_OPERATION_SLOT_KEY = "Analysis.LinkOperation.MaxAgeLastOperationSlot";
    static readonly TimeSpan MAX_AGE_LAST_OPERATION_SLOT_DEFAULT = TimeSpan.FromDays (90); // About 3 months
    
    #region Members
    LowerBound<DateTime> m_begin;
    UpperBound<DateTime> m_end;
    LinkDirection m_direction;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "LinkOperation"; }
    }

    /// <summary>
    /// Begin date/time of the period that must be linked to an operation
    /// </summary>
    [XmlIgnore]
    public virtual LowerBound<DateTime> Begin {
      get { return m_begin; }
      set { m_begin = value; }
    }
    
    /// <summary>
    /// UTC begin date/time in SQL string for XML serialization
    /// </summary>
    [XmlAttribute("Begin")]
    public virtual string SqlBegin {
      get
      {
        if (!this.Begin.HasValue) {
          return null;
        }
        else {
          return this.Begin.Value.ToString("yyyy-MM-dd HH:mm:ss");
        }
      }
      set
      {
        if (string.IsNullOrEmpty (value)) {
          this.Begin = new LowerBound<DateTime> (null);
        }
        else {
          this.Begin = System.DateTime.Parse (value);
        }
      }
    }
    
    /// <summary>
    /// End date/time of the period that must be linked to an operation. If null, the end of the period is still unknown
    /// </summary>
    [XmlIgnore]
    public virtual UpperBound<DateTime> End {
      get { return m_end; }
      set { m_end = value; }
    }

    /// <summary>
    /// UTC end date/time in SQL string for XML serialization
    /// </summary>
    [XmlAttribute("End")]
    public virtual string SqlEnd {
      get
      {
        if (!this.End.HasValue) {
          return null;
        }
        else {
          return (this.End.Value).ToString ("yyyy-MM-dd HH:mm:ss");
        }
      }
      set
      {
        if (null == value) {
          this.End = new UpperBound<DateTime> (null);
        }
        else {
          this.End = System.DateTime.Parse (value);
        }
      }
    }
    
    /// <summary>
    /// Should the period be linked to an operation in the left / past (-1)
    /// or in the right / future (+1) ?
    /// </summary>
    public virtual LinkDirection Direction {
      get { return m_direction; }
      set { m_direction = value; }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    protected LinkOperation ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="direction"></param>
    /// <param name="range"></param>
    internal protected LinkOperation (IMachine machine,
                                      LinkDirection direction,
                                      UtcDateTimeRange range)
    {
      this.Machine = machine;
      this.Direction = direction;
      this.Begin = range.Lower;
      this.End = range.Upper;
    }
    #endregion // Constructors
    
    /// <summary>
    /// Make the analysis
    /// </summary>
    public override void MakeAnalysis ()
    {
      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());
      
      if (this.Direction.Equals (LinkDirection.Left) && this.Begin.HasValue) {
        // Search for the latest operation slot before this one
        IOperationSlot latestOperationSlot =
          (new OperationSlotDAO ()).GetLastBefore (this.Machine,
                                                   this.Begin.Value);
        // Note: the following assert may not work
        //       when Operation is not initialized
        /*Debug.Assert (null != latestOperationSlot.Operation);*/
        if ((null == latestOperationSlot)
            || (latestOperationSlot.EndDateTime
                < this.Begin.Value.Subtract (AnalysisConfigHelper.LinkOperationMaximumTime))) {
          // No previous operation slot or it is too old on this machine
          // => leave it pending if it is quite recent
          //    else flag it as obsolete
          ProcessNoDataYet ();
          return;
        }
        // If the time does not exceed the allowed time for LinkOperation
        // apply the operation
        UtcDateTimeRange range = new UtcDateTimeRange (latestOperationSlot.BeginDateTime, this.End);
        IOperationMachineAssociation subModification = ModelDAOHelper.ModelFactory
          .CreateOperationMachineAssociation (this.Machine,
                                              range);
        subModification.Operation = latestOperationSlot.Operation;
        ModelDAOHelper.DAOFactory.OperationMachineAssociationDAO.MakePersistent (subModification);
        subModification.Parent = this.MainModification ?? this;
        subModification.Priority = this.Priority;
        // Analysis is done
        MarkAsCompleted ("", latestOperationSlot.BeginDateTime.Value);
        return;
      }
      else if (this.Direction.Equals (LinkDirection.Right)) {
        // Search for the first operation slot after this one
        DateTime linkOperationDateTime;
        if (this.End.HasValue) {
          linkOperationDateTime = this.End.Value;
        }
        else {
          // No end value => try to get the corresponding fact
          IMonitoredMachine monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (this.Machine.Id);
          if (null == monitoredMachine) {
            log.ErrorFormat ("MakeAnalysis: " +
                             "{0} is not monitored",
                             this.Machine);
            MarkAsError ();
            return;
          }
          IFact fact = ModelDAOHelper.DAOFactory.FactDAO
            .FindFirstFactAfter (monitoredMachine,
                                 this.Begin);
          if (null == fact) {
            ProcessNoDataYet ();
            return;
          }
          if (fact.Begin.Equals (this.Begin)) {
            // Is there a fact after this one ?
            IFact nextFact = ModelDAOHelper.DAOFactory.FactDAO
              .FindFirstFactAfter (monitoredMachine,
                                   fact.End);
            if (null == nextFact) {
              GetLogger ().DebugFormat ("MakeAnalysis: " +
                                        "the retrieved fact is the latest " +
                                        "=> no data management");
              ProcessNoDataYet ();
              return;
            }
            else {
              linkOperationDateTime = fact.End;
            }
          }
          else {
            GetLogger ().WarnFormat ("MakeAnalysis: " +
                                     "no fact exists for LinkOperation " +
                                     "machine={0} begin={1} " +
                                     "=> take begin time as fallback",
                                     this.Machine, this.Begin);
            linkOperationDateTime = this.Begin.HasValue
              ? this.Begin.Value
              : new DateTime (1970, 01, 02, 00, 00, 00, DateTimeKind.Utc);
          }
        }
        IOperationSlot nextOperationSlot =
          (new OperationSlotDAO ()).GetNextNotNullOperation (this.Machine, linkOperationDateTime);
        if ((null == nextOperationSlot)
            || (Bound.Compare<DateTime> (linkOperationDateTime.Add (AnalysisConfigHelper.LinkOperationMaximumTime),
                                         nextOperationSlot.BeginDateTime) < 0)) {
          // If there is no previous operation slot, give up (there is probably no operation identification)
          IOperationSlot latestOperationSlot =
            (new OperationSlotDAO ()).GetLastOperationNotNull (this.Machine);
          TimeSpan maxAgeLastOperationSlot = ConfigSet.LoadAndGet<TimeSpan> (MAX_AGE_LAST_OPERATION_SLOT_KEY,
                                                                             MAX_AGE_LAST_OPERATION_SLOT_DEFAULT);
          if ((null == latestOperationSlot)
              || (latestOperationSlot.EndDateTime
                  < linkOperationDateTime.Subtract (maxAgeLastOperationSlot))) {
            string message = string.Format ("No operation slot {0} before {1}",
                                            maxAgeLastOperationSlot, linkOperationDateTime);
            GetLogger ().WarnFormat ("MakeAnalysis: " +
                                     "{0} " +
                                     "=> give up (probably no operation identification)",
                                     message);
            AddAnalysisLog (LogLevel.WARN, message);
            MarkAsCompleted ("");
            (new LinkOperationDAO ()).MakePersistent (this);
            return;
          }
          
          // No previous operation slot or it is too old on this machine
          // => leave it pending if it is quite recent
          //    else flag it as obsolete
          ProcessNoDataYet ();
          return;
        }
        Debug.Assert (null != nextOperationSlot);
        Debug.Assert (null != nextOperationSlot.Operation);
        // If the time does not exceed the allowed time for LinkOperation
        // apply the operation
        UtcDateTimeRange range = new UtcDateTimeRange (this.Begin, nextOperationSlot.EndDateTime);
        IOperationMachineAssociation subModification = ModelDAOHelper.ModelFactory
          .CreateOperationMachineAssociation (this.Machine,
                                              range);
        subModification.Operation = nextOperationSlot.Operation;
        ModelDAOHelper.DAOFactory.OperationMachineAssociationDAO.MakePersistent (subModification);
        subModification.Parent = this.MainModification ?? this;
        subModification.Priority = this.Priority;
        // Analysis is done
        MarkAsCompleted ("", (DateTime?)nextOperationSlot.BeginDateTime);
        return;
      }
      else {
        GetLogger ().FatalFormat ("MakeAnalysis: " +
                                  "link direction {0} is not managed",
                                  this.Direction);
        throw new Exception ("Unmanaged LinkDirection in MakeAnalysis");
      }
    }
    
    /// <summary>
    /// Apply the modification while keeping it transient
    /// 
    /// It should be never called, because there is no transient modification to process.
    /// Use a persistent entity instead and MakeAnalysis.
    /// </summary>
    public override void Apply ()
    {
      Debug.Assert (false);
      GetLogger ().FatalFormat ("Apply: not implemented/supported");
      throw new NotImplementedException ();
    }

    /// <summary>
    /// Manage the case where no data has been found to process the modification
    /// </summary>
    void ProcessNoDataYet () {
      if (DateTime.UtcNow.Subtract (this.DateTime)
          < AnalysisConfigHelper.ObsoleteTime) {
        MarkAsPending (DateTime.UtcNow);
        return;
      }
      else {
        GetLogger ().WarnFormat ("MakeAnalysis: " +
                                 "no data has been found to process the modification " +
                                 "and it is too old, " +
                                 "make it obsolete");
        AddAnalysisLog (LogLevel.WARN, "Obsolete");
        MarkAsObsolete ();
        (new LinkOperationDAO ()).MakePersistent (this);
        return;
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      base.Unproxy ();
    }
  }
}
