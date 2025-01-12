// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using System.Collections.Generic;
using NHibernate.Criterion;
using System.Diagnostics;
using Lemoine.Collections;
using Lemoine.Core.ExceptionManagement;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IReasonSlotDAO">IReasonMachineAssociationDAO</see>
  /// </summary>
  public sealed class ReasonMachineAssociationDAO
    // TODO: change SaveOnly...
    : SaveOnlyByMachineNHibernateDAO<ReasonMachineAssociation, IReasonMachineAssociation, long>
    , IReasonMachineAssociationDAO
  {
    static readonly string MAX_MANUAL_REASON_DURATION_KEY = "Reason.Manual.MaxDuration";
    static readonly TimeSpan MAX_MANUAL_REASON_DURATION_DEFAULT = TimeSpan.FromHours (12);

    static readonly string MAX_AUTO_REASON_DURATION_KEY = "Reason.Auto.MaxDuration";
    static readonly TimeSpan MAX_AUTO_REASON_DURATION_DEFAULT = TimeSpan.FromDays (3);

    static readonly string MANUAL_REASON_PRIORITY_KEY = "Reason.Manual.Priority";
    static readonly int MANUAL_REASON_PRIORITY_DEFAULT = 1000;

    static readonly string AUTO_REASON_PRIORITY_KEY = "Reason.Auto.Priority";
    static readonly int AUTO_REASON_PRIORITY_DEFAULT = 80;

    static readonly ILog log = LogManager.GetLogger (typeof (ReasonMachineAssociationDAO).FullName);

    /// <summary>
    /// Insert a new row in database with a new manual reason
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="reason"></param>
    /// <param name="reasonScore"></param>
    /// <param name="details"></param>
    /// <returns></returns>
    public long InsertManualReason (IMachine machine, UtcDateTimeRange range, IReason reason, double reasonScore, string details, string jsonData)
    {
      var association = ModelDAOHelper.ModelFactory.CreateReasonMachineAssociation (machine, range);
      association.SetManualReason (reason, reasonScore, details, jsonData);
      association.Option = AssociationOption.TrackSlotChanges;
      association.Priority = Lemoine.Info.ConfigSet
        .LoadAndGet (MANUAL_REASON_PRIORITY_KEY, MANUAL_REASON_PRIORITY_DEFAULT);
      return Insert (association, false, false);
    }

    /// <summary>
    /// Insert a new row in database to reset a reason
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public long InsertResetReason (IMachine machine, UtcDateTimeRange range)
    {
      var association = ModelDAOHelper.ModelFactory.CreateReasonMachineAssociation (machine, range);
      association.ResetManualReason ();
      association.Priority = Lemoine.Info.ConfigSet
        .LoadAndGet (MANUAL_REASON_PRIORITY_KEY, MANUAL_REASON_PRIORITY_DEFAULT);
      return Insert (association, false, false);
    }

    /// <summary>
    /// Insert a new row in database with an auto-reason
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="reason"></param>
    /// <param name="reasonScore"></param>
    /// <param name="details"></param>
    /// <param name="dynamic"></param>
    /// <param name="overwriteRequired"></param>
    /// <param name="option"></param>
    /// <returns></returns>
    public long InsertAutoReason (IMachine machine, UtcDateTimeRange range, IReason reason, double reasonScore, string details, string dynamic, bool overwriteRequired, AssociationOption? option, string jsonData = null)
    {
      var association = ModelDAOHelper.ModelFactory.CreateReasonMachineAssociation (machine, range);
      association.SetAutoReason (reason, reasonScore, overwriteRequired, details, jsonData);
      association.Dynamic = dynamic;
      association.Option = option;
      association.Priority = Lemoine.Info.ConfigSet
        .LoadAndGet (AUTO_REASON_PRIORITY_KEY, AUTO_REASON_PRIORITY_DEFAULT);
      return Insert (association, false, false);
    }

    /// <summary>
    /// Insert a row in database that corresponds to a sub-modification
    /// </summary>
    /// <param name="association"></param>
    /// <param name="range"></param>
    /// <param name="preChange"></param>
    /// <param name="parent">optional: parent</param>
    /// <returns>persistent association</returns>
    public IMachineModification InsertSub (IReasonMachineAssociation association, UtcDateTimeRange range, Action<IReasonMachineAssociation> preChange, IMachineModification parent)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"InsertSub: parent.Id={((IDataWithId<long>)association).Id},Priority={association.Priority},StatusPriority={association.StatusPriority}");
      }

      IReasonMachineAssociation subModification = association.Clone (range);
      subModification.Priority = association.StatusPriority;
      preChange (subModification);
      var modificationId = Insert (subModification, true, true);

      var persistentSubModification = ModelDAOHelper.DAOFactory.MachineModificationDAO
          .FindById (modificationId, association.Machine);
      Debug.Assert (null != persistentSubModification);
      var mainModification = ((ReasonMachineAssociation)association).MainModification;
      persistentSubModification.Parent = parent ?? (mainModification ?? association);

      return persistentSubModification;
    }

    long Insert (IReasonMachineAssociation association, bool subModification, bool fillStatus)
    {
      Debug.Assert (null != association);
      Debug.Assert (null != association.Machine);

      if (log.IsDebugEnabled) {
        log.Debug ($"Insert: Create assocation with priority {association.Priority}");
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        string partitionExistsRequest = string.Format (@"
SELECT EXISTS (
SELECT 1 FROM information_schema.tables
WHERE table_schema='pgfkpart'
  AND table_name='reasonmachineassociation_p{0}'
);
", association.Machine.Id);
        bool partitionExists = NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery (partitionExistsRequest)
          .AddScalar ("exists", NHibernate.NHibernateUtil.Boolean)
          .UniqueResult<bool> ();
        if (!partitionExists) {
          base.MakePersistent (association);
          return ((IDataWithId<long>)association).Id;
        }

        using (var transaction = session.BeginTransaction ("ReasonMachineAssociationDAO.MakePersistent.Partitioned", TransactionLevel.ReadCommitted, true)) {
          try {
            var insertIntoMachineModificationQuery = string.Format (@"
INSERT INTO pgfkpart.machinemodification_p{0}
  (modificationreferencedtable, machinemodificationmachineid, modificationpriority, parentglobalmodificationid, parentmachinemodificationid, modificationauto)
VALUES ('ReasonMachineAssociation', {0}, :Priority, :ParentGlobal, :ParentMachine, :Auto)
RETURNING modificationid;
",
              association.Machine.Id // 0
      );
            long? parentGlobalId = (null != association.ParentGlobal)
              ? ((IDataWithId<long>)(association.ParentGlobal)).Id
              : (long?)null;
            long? parentMachineId = (null != association.ParentMachine)
              ? ((IDataWithId<long>)(association.ParentMachine)).Id
              : (long?)null;
            var modificationId = NHibernateHelper.GetCurrentSession ()
              .CreateSQLQuery (insertIntoMachineModificationQuery)
              .AddScalar ("modificationid", NHibernate.NHibernateUtil.Int64)
              .SetInt32 ("Priority", association.Priority)
              .SetParameter ("ParentGlobal", parentGlobalId, NHibernate.NHibernateUtil.Int64)
              .SetParameter ("ParentMachine", parentMachineId, NHibernate.NHibernateUtil.Int64)
              .SetBoolean ("Auto", association.Auto)
              .UniqueResult<long> ();

            var insertIntoReasonMachineAssociationQuery = string.Format (@"
INSERT INTO pgfkpart.reasonmachineassociation_p{0} (modificationid, machineid, reasonid, 
  reasonmachineassociationbegin, reasonmachineassociationend, reasondetails,
  reasonmachineassociationoption, reasonmachineassociationreasonscore,
  reasonmachineassociationkind,
  reasonmachineassociationdynamic, reasondata)
VALUES (:Id, :Machine, :Reason, :Begin, :End, :Details, :Option, :ReasonScore, :Kind, :Dynamic, CAST(:Data AS jsonb));
",
              association.Machine.Id);
            var dynamic = (string.IsNullOrEmpty (association.Dynamic))
              ? null
              : association.Dynamic;
            var details = (string.IsNullOrEmpty (association.ReasonDetails))
              ? null
              : association.ReasonDetails;
            int? option = association.Option.HasValue
              ? (int?)association.Option.Value
              : null;
            NHibernateHelper.GetCurrentSession ()
              .CreateSQLQuery (insertIntoReasonMachineAssociationQuery)
              .SetInt64 ("Id", modificationId)
              .SetEntity ("Machine", association.Machine)
              .SetParameter ("Reason", association.Reason, NHibernateUtil.Entity (typeof (Reason)))
              .SetParameter ("Begin", association.Begin, (NHibernate.Type.IType)new Lemoine.NHibernateTypes.UtcLowerBoundDateTimeSecondsType ())
              .SetParameter ("End", association.End, (NHibernate.Type.IType)new Lemoine.NHibernateTypes.UtcUpperBoundDateTimeSecondsType ())
              .SetString ("Details", details)
              .SetParameter ("Option", option, NHibernateUtil.Int32)
              .SetDouble ("ReasonScore", association.ReasonScore)
              .SetInt32 ("Kind", (int)association.Kind)
              .SetString ("Dynamic", dynamic)
              .SetString ("Data", association.JsonData)
              .ExecuteUpdate ();

            if (fillStatus) {
              if (!subModification && transaction.TopTransaction) {
                transaction.CommitNew ();
              }

              var insertIntoStatusQuery = string.Format (@"
INSERT INTO pgfkpart.machinemodificationstatus_p{0} (modificationid, machinemodificationstatusmachineid, analysisiterations, analysistotalduration, modificationstatuspriority)
SELECT modificationid, machinemodificationmachineid, 0, 0, modificationpriority
FROM pgfkpart.machinemodification_p{0} m
WHERE machinemodificationmachineid={0}
  AND modificationid=:Id
;", association.Machine.Id);
              NHibernateHelper.GetCurrentSession ()
                .CreateSQLQuery (insertIntoStatusQuery)
                .SetInt64 ("Id", modificationId)
                .ExecuteUpdate ();
            }

            transaction.Commit ();
            return modificationId;
          }
          catch (Exception ex) {
            log.Exception (ex, "Insert");
            if (ExceptionTest.IsTransactionSerializationFailure (ex)) {
              transaction.FlagSerializationFailure ();
            }
            throw;
          }
        }
      }
    }

    /// <summary>
    /// Add in the request the machine id column
    /// </summary>
    /// <param name="machineId"></param>
    /// <returns></returns>
    protected internal override ICriterion GetMachineCriterion (int machineId)
    {
      return Restrictions.Conjunction ()
        .Add (Restrictions.Eq ("ModificationMachine.Id", machineId))
        .Add (Restrictions.Disjunction ()
          .Add (Restrictions.IsNull ("ModificationStatusMachine"))
          .Add (Restrictions.Eq ("ModificationStatusMachine.Id", machineId)))
        .Add (Restrictions.Eq ("Machine.Id", machineId));
    }

    /// <summary>
    /// <see cref="IReasonMachineAssociation"/>
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="modificationId"></param>
    /// <returns></returns>
    public IReasonMachineAssociation GetNextAncestorAuto (IMachine machine, long modificationId)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonMachineAssociation> ()
        .Add (Restrictions.Eq ("ModificationMachine", machine))
        .Add (Restrictions.Eq ("ModificationStatusMachine", machine))
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (Restrictions.IsNull ("ParentGlobal"))
        .Add (Restrictions.IsNull ("ParentMachine"))
        .Add (Restrictions.Gt ("Id", modificationId))
        .Add (Restrictions.Disjunction ()
          .Add (Restrictions.Eq ("Kind", ReasonMachineAssociationKind.Auto))
          .Add (Restrictions.Eq ("Kind", ReasonMachineAssociationKind.AutoWithOverwriteRequired))
          )
        .AddOrder (Order.Asc ("Id"))
        .SetMaxResults (1)
        .UniqueResult<IReasonMachineAssociation> ();
    }

    /// <summary>
    /// <see cref="IReasonMachineAssociationDAO"/>
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IReasonMachineAssociation> FindAppliedManualInRange (IMachine machine, UtcDateTimeRange range)
    {
      var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonMachineAssociation> ()
        .Add (Restrictions.Eq ("ModificationMachine", machine))
        .Add (Restrictions.Eq ("ModificationStatusMachine", machine))
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (Restrictions.IsNull ("ParentGlobal"))
        .Add (Restrictions.IsNull ("ParentMachine"))
        .Add (ModificationDAO.GetNotInErrorCriterion ())
        .Add (ModificationDAO.GetNotNewCriterion ())
        .Add (ModificationDAO.GetNotNotApplicableCriterion ())
        .Add (InUtcRange (range))
        .Add (Restrictions.Eq ("Kind", ReasonMachineAssociationKind.Manual));
      if (range.Lower.HasValue) {
        criteria = criteria
          .Add (Restrictions.Disjunction ()
            .Add (Restrictions.IsNull ("AnalysisAppliedDateTime"))
            .Add (Restrictions.Gt ("AnalysisAppliedDateTime", range.Lower.Value))
          );
        var maxManualReasonDuration = Lemoine.Info.ConfigSet
          .LoadAndGet<TimeSpan> (MAX_MANUAL_REASON_DURATION_KEY,
          MAX_MANUAL_REASON_DURATION_DEFAULT);
        // ReasonMachineAssociation.Range.Lower must be after range.Lower-maxDuration
        var minStart = range.Lower.Value
          .Subtract (maxManualReasonDuration);
        criteria = criteria
          .Add (Restrictions.Ge ("Begin", (LowerBound<DateTime>)minStart));
      }
      return criteria
        .AddOrder (Order.Asc ("Id"))
        .AddOrder (Order.Asc ("DateTime"))
        .AddOrder (Order.Desc ("Priority"))
        .List<IReasonMachineAssociation> ();
    }

    /// <summary>
    /// <see cref="IReasonMachineAssociationDAO"/>
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IReasonMachineAssociation> FindAppliedAutoInRange (IMachine machine, UtcDateTimeRange range)
    {
      var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonMachineAssociation> ()
        .Add (Restrictions.Eq ("ModificationMachine", machine))
        .Add (Restrictions.Eq ("ModificationStatusMachine", machine))
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (Restrictions.IsNull ("ParentGlobal"))
        .Add (Restrictions.IsNull ("ParentMachine"))
        .Add (ModificationDAO.GetNotInErrorCriterion ())
        .Add (ModificationDAO.GetNotNewCriterion ())
        .Add (ModificationDAO.GetNotNotApplicableCriterion ())
        .Add (InUtcRange (range))
        .Add (Restrictions.In ("Kind", new ReasonMachineAssociationKind[] { ReasonMachineAssociationKind.Auto, ReasonMachineAssociationKind.AutoWithOverwriteRequired }));
      if (range.Lower.HasValue) {
        criteria = criteria
          .Add (Restrictions.Disjunction ()
            .Add (Restrictions.IsNull ("AnalysisAppliedDateTime"))
            .Add (Restrictions.Gt ("AnalysisAppliedDateTime", range.Lower.Value))
          );
        var maxAutoReasonDuration = Lemoine.Info.ConfigSet
          .LoadAndGet<TimeSpan> (MAX_AUTO_REASON_DURATION_KEY,
          MAX_AUTO_REASON_DURATION_DEFAULT);
        // ReasonMachineAssociation.Range.Lower must be after range.Lower-maxDuration
        var minStart = range.Lower.Value
          .Subtract (maxAutoReasonDuration);
        criteria = criteria
          .Add (Restrictions.Ge ("Begin", (LowerBound<DateTime>)minStart));
      }
      return criteria
        .AddOrder (Order.Asc ("Id"))
        .AddOrder (Order.Asc ("DateTime"))
        .AddOrder (Order.Desc ("Priority"))
        .List<IReasonMachineAssociation> ();
    }

    /// <summary>
    /// Range criterion with UTC date/times
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    internal static AbstractCriterion InUtcRange (UtcDateTimeRange range)
    {
      if ((null == range) || (range.IsEmpty ())) {
        return Expression.Sql ("FALSE");
      }

      if (!range.Lower.HasValue && !range.Upper.HasValue) {
        return Restrictions.IsNotNull ("Begin");
      }

      // From constraint
      Junction result = Restrictions.Conjunction ();
      if (range.Lower.HasValue) {
        result = result
          .Add (Restrictions.Disjunction ()
                .Add (Restrictions.IsNull ("End"))
                .Add (Restrictions.Gt ("End",
                                       (UpperBound<DateTime>)range.Lower.Value)));
      }

      // To constraint
      if (range.Upper.HasValue) {
        result = result
          .Add (Restrictions.Lt ("Begin",
                                 (LowerBound<DateTime>)range.Upper.Value));
      }

      return result;
    }
  }
}
