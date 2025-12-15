// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.ExceptionManagement;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineStateTemplateAssociationDAO">IMachineStateTemplateAssociationDAO</see>
  /// </summary>
  public class MachineStateTemplateAssociationDAO
    : SaveOnlyByMachineNHibernateDAO<MachineStateTemplateAssociation, IMachineStateTemplateAssociation, long>
    , IMachineStateTemplateAssociationDAO
  {
    static readonly string PRIORITY_KEY = "MachineStateTemplateAssociation.Priority";
    static readonly int PRIORITY_DEFAULT = 1000;

    ILog log = LogManager.GetLogger<MachineStateTemplateAssociationDAO> ();

    /// <summary>
    /// Get all MachineStateTemplateAssociation for a specific machine within a period
    /// Valid segments have:
    /// - their beginning strictly inferior to the end of the period, AND
    /// - their end strictly superior to the beginning of the period
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public IList<IMachineStateTemplateAssociation> FindByMachineAndPeriod (IMachine machine, DateTime start, DateTime end)
    {
      Debug.Assert (null != machine);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineStateTemplateAssociation> ()
        .Add (GetMachineCriterion (machine.Id))
        .Add (Restrictions.IsNull ("ParentGlobal"))
        .Add (Restrictions.IsNull ("ParentMachine"))
        .Add (Restrictions.Lt ("Begin", end))
        .Add (Restrictions.Or (
          Restrictions.IsNull ("End"),
          Restrictions.Gt ("End", start)))
        .List<IMachineStateTemplateAssociation> ();
    }

    /// <summary>
    /// Insert a new row in database with a new manual reason
    /// </summary>
    public long Insert (IMachine machine, UtcDateTimeRange range, IMachineStateTemplate machineStateTemplate)
    {
      var association = ModelDAO.ModelDAOHelper.ModelFactory.CreateMachineStateTemplateAssociation (machine, machineStateTemplate, range);
      association.Option = AssociationOption.TrackSlotChanges;
      association.Priority = Lemoine.Info.ConfigSet
        .LoadAndGet (PRIORITY_KEY, PRIORITY_DEFAULT);
      return InsertModification (association, false, false);
    }

    /// <summary>
    /// Insert a row in database that corresponds to a sub-modification
    /// </summary>
    /// <param name="association"></param>
    /// <param name="range"></param>
    /// <param name="preChange"></param>
    /// <param name="parent">optional: parent</param>
    /// <returns>persistent association</returns>
    public IMachineModification InsertSub (IMachineStateTemplateAssociation association, UtcDateTimeRange range, Action<IMachineStateTemplateAssociation> preChange, IMachineModification parent)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"InsertSub: parent.Id={(association).Id},Priority={association.Priority},StatusPriority={association.StatusPriority}");
      }

      IMachineStateTemplateAssociation subModification = association.Clone (range);
      subModification.Priority = association.StatusPriority;
      preChange (subModification);
      var modificationId = InsertModification (subModification, true, true);

      var persistentSubModification = ModelDAOHelper.DAOFactory.MachineModificationDAO
          .FindById (modificationId, association.Machine);
      Debug.Assert (null != persistentSubModification);
      var mainModification = ((ReasonMachineAssociation)association).MainModification;
      persistentSubModification.Parent = parent ?? (mainModification ?? association);

      return persistentSubModification;
    }

    long InsertModification (IMachineStateTemplateAssociation association, bool subModification, bool fillStatus)
    {
      Debug.Assert (null != association);
      Debug.Assert (null != association.Machine);

      if (log.IsDebugEnabled) {
        log.Debug ($"InsertModification: Create assocation with priority {association.Priority}");
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        string partitionExistsRequest = string.Format (@"
SELECT EXISTS (
SELECT 1 FROM information_schema.tables
WHERE table_schema='pgfkpart'
  AND table_name='machinestatetemplateassociation_p{0}'
);
", association.Machine.Id);
        bool partitionExists = NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery (partitionExistsRequest)
          .AddScalar ("exists", NHibernate.NHibernateUtil.Boolean)
          .UniqueResult<bool> ();
        if (!partitionExists) {
          base.MakePersistent (association);
          return association.Id;
        }

        using (var transaction = session.BeginTransaction ("MachineStateTemplateAssociationDAO.MakePersistent.Partitioned", TransactionLevel.ReadCommitted, true)) {
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
              ? ((association.ParentGlobal)).Id
              : (long?)null;
            long? parentMachineId = (null != association.ParentMachine)
              ? ((association.ParentMachine)).Id
              : (long?)null;
            var modificationId = NHibernateHelper.GetCurrentSession ()
              .CreateSQLQuery (insertIntoMachineModificationQuery)
              .AddScalar ("modificationid", NHibernate.NHibernateUtil.Int64)
              .SetInt32 ("Priority", association.Priority)
              .SetParameter ("ParentGlobal", parentGlobalId, NHibernate.NHibernateUtil.Int64)
              .SetParameter ("ParentMachine", parentMachineId, NHibernate.NHibernateUtil.Int64)
              .SetBoolean ("Auto", association.Auto)
              .UniqueResult<long> ();

            var insertIntoReasonMachineAssociationQuery = $"""
INSERT INTO pgfkpart.machinestatetemplateassociation_p{association.Machine.Id} (modificationid, machineid, machinestatetemplateid,
  userid, shiftid,
  machinestatetemplateassociationbegin, machinestatetemplateassociationend,
  machinestatetemplateassociationforce, machinestatetemplateassociationoption)
VALUES (:Id, :Machine, :MachineStateTemplate, :User, :Shift, :Begin, :End, :Force, :Option);
""";
            int? option = association.Option.HasValue
              ? (int?)association.Option.Value
              : null;
            NHibernateHelper.GetCurrentSession ()
              .CreateSQLQuery (insertIntoReasonMachineAssociationQuery)
              .SetInt64 ("Id", modificationId)
              .SetEntity ("Machine", association.Machine)
              .SetParameter ("MachineStateTemplate", association.MachineStateTemplate, NHibernateUtil.Entity (typeof (MachineStateTemplate)))
              .SetParameter ("User", association.User, NHibernateUtil.Entity (typeof (User)))
              .SetParameter ("Shift", association.Shift, NHibernateUtil.Entity (typeof (Shift)))
              .SetParameter ("Begin", association.Begin, (NHibernate.Type.IType)new Lemoine.NHibernateTypes.UtcLowerBoundDateTimeSecondsType ())
              .SetParameter ("End", association.End, (NHibernate.Type.IType)new Lemoine.NHibernateTypes.UtcUpperBoundDateTimeSecondsType ())
              .SetBoolean ("Force", association.Force)
              .SetParameter ("Option", option, NHibernateUtil.Int32)
              .ExecuteUpdate ();

            if (fillStatus) {
              if (!subModification && transaction.TopTransaction) {
                transaction.CommitNew ();
              }

              var insertIntoStatusQuery = $"""
INSERT INTO pgfkpart.machinemodificationstatus_p{association.Machine.Id} (modificationid, machinemodificationstatusmachineid, analysisiterations, analysistotalduration, modificationstatuspriority)
SELECT modificationid, machinemodificationmachineid, 0, 0, modificationpriority
FROM pgfkpart.machinemodification_p{association.Machine.Id} m
WHERE machinemodificationmachineid={association.Machine.Id}
  AND modificationid=:Id;
""";
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
  }
}
