// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate.Criterion;
using System.Threading.Tasks;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine .ModelDAO.IModificationDAO">IModificationDAO</see>
  /// </summary>
  public sealed class ModificationDAO
    : IModificationDAO
  {
    ILog log = LogManager.GetLogger (typeof (ModificationDAO).FullName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modification"></param>
    public void MakePersistent (IModification modification)
    {
      if (modification is IGlobalModification) {
        ModelDAOHelper.DAOFactory.GlobalModificationDAO
          .MakePersistent ((IGlobalModification)modification);
      }
      else if (modification is IMachineModification) {
        ModelDAOHelper.DAOFactory.MachineModificationDAO
          .MakePersistent ((IMachineModification)modification);
      }
      else {
        Debug.Assert (false);
        log.FatalFormat ("MakePersistent: " +
                         "not supported sub-type for {0}",
                         modification);
        throw new Exception ("Not supported sub-type for the modification");
      }
    }

    /// <summary>
    /// Re-attach the object to the session
    /// </summary>
    /// <param name="modification"></param>
    public void Lock (IModification modification)
    {
      if (modification is IGlobalModification) {
        ModelDAOHelper.DAOFactory.GlobalModificationDAO
          .Lock ((IGlobalModification)modification);
      }
      else if (modification is IMachineModification) {
        ModelDAOHelper.DAOFactory.MachineModificationDAO
          .Lock ((IMachineModification)modification);
      }
      else {
        Debug.Assert (false);
        log.FatalFormat ("MakePersistent: " +
                         "not supported sub-type for {0}",
                         modification);
        throw new Exception ("Not supported sub-type for the modification");
      }
    }

    /// <summary>
    /// Re-attach the object to the session with an upgrade lock
    /// 
    /// Do not use it in a serializable transaction
    /// </summary>
    /// <param name="modification"></param>
    public void UpgradeLock (IModification modification)
    {
      if (modification is IGlobalModification) {
        ModelDAOHelper.DAOFactory.GlobalModificationDAO
          .UpgradeLock ((IGlobalModification)modification);
      }
      else if (modification is IMachineModification) {
        var machineModification = (IMachineModification)modification;
        ModelDAOHelper.DAOFactory.MachineModificationDAO
          .UpgradeLock (machineModification);
      }
      else {
        Debug.Assert (false);
        log.FatalFormat ("MakePersistent: " +
                         "not supported sub-type for {0}",
                         modification);
        throw new Exception ("Not supported sub-type for the modification");
      }
    }

    /// <summary>
    /// Find all the modifications (global and machine)
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IModification> FindAll ()
    {
      return ModelDAOHelper.DAOFactory.GlobalModificationDAO.FindAll ().Cast<IModification> ()
        .Concat (ModelDAOHelper.DAOFactory.MachineModificationDAO.FindAll ().Cast<IModification> ());
    }

    /// <summary>
    /// Find by ID
    /// 
    /// This method is inefficient. Please prefer the specific method of GlobalModificationDAO or MachineModificationDAO
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public IModification FindById (long id)
    {
      IGlobalModification globalModification = ModelDAOHelper.DAOFactory.GlobalModificationDAO
        .FindById (id);
      if (null != globalModification) {
        return globalModification;
      }
      IList<IMachine> machines = ModelDAOHelper.DAOFactory.MachineDAO
        .FindAll ();
      foreach (IMachine machine in machines) {
        IMachineModification machineModification = ModelDAOHelper.DAOFactory.MachineModificationDAO
          .FindById (id, machine);
        if (null != machineModification) {
          return machineModification;
        }
      }
      return null;
    }

    /// <summary>
    /// Returns the number of remaining modifications to process
    /// before a specified modification is completed
    /// </summary>
    /// <param name="modificationId"></param>
    /// <param name="createNewAnalysisStatusBefore"></param>
    /// <returns></returns>
    public double GetNumberOfRemainingModifications (long modificationId, bool createNewAnalysisStatusBefore = true)
    {
      IGlobalModification globalModification = ModelDAOHelper.DAOFactory.GlobalModificationDAO
        .FindById (modificationId);
      if (null != globalModification) {
        return ModelDAOHelper.DAOFactory.GlobalModificationDAO
          .GetNumberOfRemainingModifications (globalModification, createNewAnalysisStatusBefore);
      }

      IList<IMachine> machines = ModelDAOHelper.DAOFactory.MachineDAO
        .FindAll ();
      foreach (IMachine machine in machines) {
        IMachineModification machineModification = ModelDAOHelper.DAOFactory.MachineModificationDAO
          .FindById (modificationId, machine);
        if (null != machineModification) {
          return ModelDAOHelper.DAOFactory.MachineModificationDAO
            .GetNumberOfRemainingModifications (machineModification, createNewAnalysisStatusBefore);
        }
      }

      log.FatalFormat ("GetNumberOfRemainingModifications: " +
                       "no modification with Id {0}",
                       modificationId);
      throw new ArgumentException ("Invalid modification Id", "modificationId");
    }

    /// <summary>
    /// Get the completion of a modification
    /// 
    /// <item>0: not yet started</item>
    /// <item>1: completed</item>
    /// <item>1: in case the analysis status is pending (arbitrary value)</item>
    /// </summary>
    /// <param name="modification">not null</param>
    /// <returns></returns>
    public double GetCompletion (IModification modification)
    {
      Debug.Assert (null != modification);
      if (null == modification) {
        log.Fatal ("GetCompletion: " +
                   "null modification in argument");
        throw new ArgumentNullException ("modification");
      }

      if (modification.AnalysisStatus.Equals (AnalysisStatus.Pending)) {
        log.DebugFormat ("GetCompletion: " +
                         "pending modification {0} " +
                         "=> return 1 if there is no sub-modification in progress",
                         modification);
        int total = 1;
        double completed = 1.0;
        if (modification.AnalysisSubModifications) {
          foreach (IModification subModification in modification.SubModifications) {
            ++total;
            completed += GetCompletion (subModification);
          }
        }
        return completed / total;
      }
      else if (modification.AnalysisStatus.IsNew ()) {
        log.DebugFormat ("GetCompletion: " +
                         "new modification {0} " +
                         "=> return 0",
                         modification);
        return 0.0;
      }
      else if (modification.AnalysisStatus.IsInProgress ()) {
        if (modification.AnalysisStatus.Equals (AnalysisStatus.InProgress)
            || modification.AnalysisStatus.Equals (AnalysisStatus.StepTimeout)
            || modification.AnalysisStatus.Equals (AnalysisStatus.Timeout)
            || modification.AnalysisStatus.Equals (AnalysisStatus.DatabaseTimeout)) {
          log.DebugFormat ("GetCompletion: " +
                           "modification {0} InProgress or StepTimeout " +
                           "=> return arbitrary value 0.8");
          // This is a little bit too complex here to return a good value for now
          // and compare AppliedDateTime with the right range
          // => return an arbritary value 0.2
          int total = 1;
          if (modification.AnalysisSubModifications) {
            total += modification.SubGlobalModifications.Count
              + modification.SubMachineModifications.Count;
          }
          return 0.2 / total;
        }
        else if (modification.AnalysisStatus.Equals (AnalysisStatus.PendingSubModifications)) {
          IEnumerable<IModification> subModifications = modification.SubModifications;
          int total = 1;
          double completed = 1;
          // Note: this code is ok until 4 sub-modifications. If there are more that 4 sub-modifications coming
          //       the completion may decrease lightly in very specific cases
          foreach (IModification subModification in subModifications) {
            ++total;
            completed += GetCompletion (subModification);
          }
          log.DebugFormat ("GetCompletion: " +
                           "{0} sub-modifications with a total completion {1} " +
                           "=> return {2}",
                           total, completed, completed / total);
          return completed / total;
        }
        else {
          Debug.Assert (false);
          log.FatalFormat ("GetCompletion: " +
                           "unexpected analysis status {0} which is InProgress " +
                           "=> return arbritary 0.2 as a fallback value",
                           modification.AnalysisStatus);
          return 0.2;
        }
      }
      else { // Not new or in progress: return 1
        log.DebugFormat ("GetCompletion: " +
                         "completed modification {0} " +
                         "=> return 1",
                         modification);
        return 1.0;
      }
    }

    /// <summary>
    /// Criterion to get the modifications after a given id / priority
    /// </summary>
    /// <param name="id"></param>
    /// <param name="priority">status priority</param>
    /// <returns></returns>
    static internal ICriterion AfterWithStatusPriorityOnly (long id, int priority)
    {
      if (int.MaxValue == priority) {
        return Expression.Sql ("TRUE");
      }
      else {
        return Restrictions
          .Disjunction ()
          .Add (Restrictions.Lt ("StatusPriority", priority))
          .Add (Restrictions
            .Conjunction ()
            .Add (Restrictions.Eq ("StatusPriority", priority))
            .Add (Restrictions.Gt ("Id", id))
          );
      }
    }

    /// <summary>
    /// Criterion to get the modifications after a given date/time / id / priority
    /// </summary>
    /// <param name="id"></param>
    /// <param name="priority"></param>
    /// <returns></returns>
    static internal ICriterion After (long id,
                                      int priority)
    {
      if (int.MaxValue == priority) {
        return Expression.Sql ("TRUE");
      }
      else {
        return Restrictions
          .Disjunction ()
          .Add (PriorityLt (priority))
          .Add (Restrictions
            .Conjunction ()
            .Add (PriorityEq (priority))
            .Add (Restrictions.Gt ("Id", id))
          );
      }
    }

    static internal ICriterion PriorityLt (int priority)
    {
      if (int.MaxValue == priority) {
        return Expression.Sql ("TRUE");
      }
      else if (priority <= 0) {
        return Expression.Sql ("FALSE");
      }
      else {
        return Restrictions.Disjunction ()
          .Add (Restrictions.Conjunction ()
            .Add (Restrictions.IsNull ("StatusPriority"))
            .Add (Restrictions.Lt ("Priority", priority))
          )
          .Add (Restrictions.Conjunction ()
            .Add (Restrictions.IsNotNull ("StatusPriority"))
            .Add (Restrictions.Lt ("StatusPriority", priority))
          );
      }
    }

    static internal ICriterion PriorityGt (int priority)
    {
      if (int.MaxValue == priority) {
        return Expression.Sql ("FALSE");
      }
      else if (priority < 0) {
        return Expression.Sql ("TRUE");
      }
      else {
        return Restrictions.Disjunction ()
          .Add (Restrictions.Conjunction ()
            .Add (Restrictions.IsNull ("StatusPriority"))
            .Add (Restrictions.Gt ("Priority", priority))
          )
          .Add (Restrictions.Conjunction ()
            .Add (Restrictions.IsNotNull ("StatusPriority"))
            .Add (Restrictions.Gt ("StatusPriority", priority))
          );
      }
    }

    static internal ICriterion PriorityEq (int priority)
    {
      if (int.MaxValue == priority) {
        return Expression.Sql ("FALSE");
      }
      else {
        return Restrictions.Disjunction ()
          .Add (Restrictions.Conjunction ()
            .Add (Restrictions.IsNull ("StatusPriority"))
            .Add (Restrictions.Eq ("Priority", priority))
          )
          .Add (Restrictions.Conjunction ()
            .Add (Restrictions.IsNotNull ("StatusPriority"))
            .Add (Restrictions.Eq ("StatusPriority", priority))
          );
      }
    }

    /// <summary>
    /// Criterion to get the modifications strictly before a specified id / priority
    /// </summary>
    /// <param name="id"></param>
    /// <param name="priority"></param>
    /// <returns></returns>
    static internal ICriterion StrictlyBefore (long id,
                                               int priority)
    {
      // either equal date, equal priority, greater id
      // or equal date, lower priority
      // or greater date
      return Restrictions
        .Disjunction ()
        .Add (PriorityGt (priority))
        .Add (Restrictions
          .Conjunction ()
          .Add (PriorityEq (priority))
          .Add (Restrictions.Lt ("Id", id))
        );
    }

    /// <summary>
    /// Criterion to get the modifications strictly before a specified modification
    /// </summary>
    /// <param name="modification">modification</param>
    /// <returns></returns>
    static internal ICriterion StrictlyBefore (IModification modification)
    {
      return StrictlyBefore (((Lemoine.Collections.IDataWithId<long>)modification).Id, modification.StatusPriority);
    }

    /// <summary>
    /// Get the modiifcations in error
    /// </summary>
    /// <returns></returns>
    static internal ICriterion GetInErrorCriterion ()
    {
      return Restrictions.Disjunction ()
        .Add (Restrictions.Eq ("AnalysisStatus", AnalysisStatus.Error))
        .Add (Restrictions.Eq ("AnalysisStatus", AnalysisStatus.Timeout))
        .Add (Restrictions.Eq ("AnalysisStatus", AnalysisStatus.ConstraintIntegrityViolation))
        .Add (Restrictions.Eq ("AnalysisStatus", AnalysisStatus.AncestorError))
        .Add (Restrictions.Eq ("AnalysisStatus", AnalysisStatus.DatabaseTimeout));
    }

    /// <summary>
    /// Criterion to get the modifications that are not in error
    /// </summary>
    /// <returns></returns>
    static internal ICriterion GetNotInErrorCriterion ()
    {
      return Restrictions.Not (GetInErrorCriterion ());
    }

    /// <summary>
    /// Criterion to exclude the modifications at are not applicable
    /// </summary>
    /// <returns></returns>
    static internal ICriterion GetNotNotApplicableCriterion ()
    {
      return Restrictions.Not (Restrictions.Eq ("AnalysisStatus", AnalysisStatus.NotApplicable));
    }

    /// <summary>
    /// Criterion to get only the modifications that have not been completed yet
    /// </summary>
    /// <returns></returns>
    static internal ICriterion GetNotCompletedCriterion ()
    {
      return Restrictions.Disjunction ()
        .Add (Restrictions.IsNull ("AnalysisStatus"))
        .Add (Restrictions.Eq ("AnalysisStatus", AnalysisStatus.New))
        .Add (Restrictions.Eq ("AnalysisStatus", AnalysisStatus.Pending))
        .Add (Restrictions.Eq ("AnalysisStatus", AnalysisStatus.InProgress))
        .Add (Restrictions.Eq ("AnalysisStatus", AnalysisStatus.PendingSubModifications))
        .Add (Restrictions.Eq ("AnalysisStatus", AnalysisStatus.StepTimeout))
        .Add (Restrictions.Eq ("AnalysisStatus", AnalysisStatus.Timeout))
        .Add (Restrictions.Eq ("AnalysisStatus", AnalysisStatus.DatabaseTimeout));
    }

    /// <summary>
    /// Criterion to get only the new modifications
    /// </summary>
    /// <returns></returns>
    static internal ICriterion GetNewCriterion ()
    {
      return Restrictions.Disjunction ()
        .Add (Restrictions.IsNull ("AnalysisStatus"))
        .Add (Restrictions.Eq ("AnalysisStatus", AnalysisStatus.New));
    }

    /// <summary>
    /// Criterion to get the modifications that are not nuew
    /// </summary>
    /// <returns></returns>
    static internal ICriterion GetNotNewCriterion ()
    {
      return Restrictions.Conjunction ()
        .Add (Restrictions.IsNotNull ("AnalysisStatus"))
        .Add (Restrictions.Not (Restrictions.Eq ("AnalysisStatus", AnalysisStatus.New)));
    }

    /// <summary>
    /// Get the last completion order
    /// </summary>
    /// <returns></returns>
    public long GetNextCompletionOrder ()
    {
      try {
        using (var command = NHibernateHelper.GetCurrentSession ().Connection.CreateCommand ()) {
          command.CommandText = @"SELECT nextval('modification_completionorder_seq'::regclass)";
          return (long)command.ExecuteScalar ();
        }
      }
      catch (Exception ex) {
        log.Exception (ex, "GetNextCompletionOrder: query failed");
        throw;
      }
    }

    /// <summary>
    /// Get a number of modifications with a specified analysis status
    /// </summary>
    /// <param name="analysisStatus"></param>
    /// <returns></returns>
    public int GetNumber (AnalysisStatus analysisStatus)
    {
      return ModelDAOHelper.DAOFactory.MachineModificationDAO.GetNumber (analysisStatus)
        + ModelDAOHelper.DAOFactory.GlobalModificationDAO.GetNumber (analysisStatus);
    }

    /// <summary>
    /// Get all parent modifications not completed that have a revision
    /// </summary>
    /// <param name="application">Name of the application that created the revision (can be null or empty)</param>
    /// <param name="minId">Minimum id to browse (strict)</param>
    /// <returns></returns>
    public IEnumerable<IModification> FindNotCompletedWithRevision (string application, long minId)
    {
      return ModelDAOHelper.DAOFactory.GlobalModificationDAO.FindNotCompletedWithRevision (application, minId).Cast<IModification> ()
        .Concat (ModelDAOHelper.DAOFactory.MachineModificationDAO.FindNotCompletedWithRevision (application, minId).Cast<IModification> ());
    }

    /// <summary>
    /// Get all modifications related to a revision
    /// </summary>
    /// <param name="revision">Cannot be null</param>
    /// <param name="minId">Minimum ID of the modifications returned (strict)</param>
    /// <returns></returns>
    public IEnumerable<IModification> FindByRevision (IRevision revision, long minId)
    {
      return ModelDAOHelper.DAOFactory.GlobalModificationDAO.FindByRevision (revision, minId).Cast<IModification> ()
        .Concat (ModelDAOHelper.DAOFactory.MachineModificationDAO.FindByRevision (revision, minId).Cast<IModification> ());
    }

    /// <summary>
    /// Get the maximum modificationId
    /// </summary>
    /// <returns></returns>
    public long? GetMaxModificationId ()
    {
      long? result = ModelDAOHelper.DAOFactory.GlobalModificationDAO.GetMaxModificationId ();
      long? value2 = ModelDAOHelper.DAOFactory.MachineModificationDAO.GetMaxModificationId (null);
      if (result == null) {
        result = value2;
      }
      else if (value2 != null) {
        if (value2.Value > result.Value) {
          result = value2;
        }
      }

      return result;
    }

    /// <summary>
    /// Get the maximum modificationId asynchronously
    /// </summary>
    /// <returns></returns>
    public async Task<long?> GetMaxModificationIdAsync ()
    {
      long? result = await ModelDAOHelper.DAOFactory.GlobalModificationDAO.GetMaxModificationIdAsync ();
      long? value2 = await ModelDAOHelper.DAOFactory.MachineModificationDAO.GetMaxModificationIdAsync (null);
      if (result == null) {
        result = value2;
      }
      else if (value2 != null) {
        if (value2.Value > result.Value) {
          result = value2;
        }
      }

      return result;
    }
  }
}
