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
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineModeDefaultReasonDAO">IMachineModeDefaultReasonDAO</see>
  /// </summary>
  public class MachineModeDefaultReasonDAO
    : VersionableNHibernateDAO<MachineModeDefaultReason, IMachineModeDefaultReason, int>
    , IMachineModeDefaultReasonDAO
  {
    static readonly int DEFAULT_INACTIVE_SCORE = 10;
    static readonly int DEFAULT_ACTIVE_SCORE = 90;
    static readonly int DEFAULT_UNKNOWN_SCORE = 10;
    static readonly int DEFAULT_OFF_SCORE = 10;
    static readonly int DEFAULT_SCORE = 10;

    static readonly ILog log = LogManager.GetLogger (typeof (MachineModeDefaultReasonDAO).FullName);

    #region DefaultValues
    /// <summary>
    /// Insert the default values
    /// </summary>
    internal void InsertDefaultValues ()
    {
      IReason reasonUndefined = new ReasonDAO ().FindById ((int)ReasonId.Undefined);
      IReason reasonMotion = new ReasonDAO ().FindById ((int)ReasonId.Motion);
      IReason reasonOff = new ReasonDAO ().FindById ((int)ReasonId.Off);
      IReason reasonUnknown = new ReasonDAO ().FindById ((int)ReasonId.Unknown);

      var machineModes = new MachineModeDAO ()
        .FindAll ();
      var machineObservationStates = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
        .FindAll ();
      foreach (var machineMode in machineModes) {
        foreach (var machineObservationState in machineObservationStates) {
          var defaultReasons =
            ModelDAOHelper.DAOFactory.MachineModeDefaultReasonDAO
            .FindWithForConfig (machineMode, machineObservationState);
          if (defaultReasons.Any (x => !x.MaximumDuration.HasValue)) {
            continue;
          }
          // There is no default reason with unlimited duration, add one in some conditions
          IMachineModeDefaultReason defaultReason;
          if ((int)MachineModeId.Off == machineMode.Id) {
            defaultReason =
              ModelDAOHelper.ModelFactory.CreateMachineModeDefaultReason (machineMode,
                                                                          machineObservationState);
            defaultReason.Reason = reasonOff;
            defaultReason.Score = DEFAULT_OFF_SCORE;
            defaultReason.Auto = true;
            if (null == reasonOff) {
              log.WarnFormat ("InsertDefaultValues: " +
                              "please add manually a default reason for machine mode Off {0} " +
                              "because no reasonOff Id=6 exists",
                              machineMode);
            }
            else {
              log.InfoFormat ("InsertDefaultValues: " +
                              "about to add a new MachineModeDefaultReason Off for " +
                              "machineMode={0} machineObservationState={1}",
                              machineMode, machineObservationState);
              ModelDAOHelper.DAOFactory.MachineModeDefaultReasonDAO.MakePersistent (defaultReason);
            }
          }
          
          if (machineMode.Parent is null) { // Top machine mode only here
            if ((int)MachineModeId.Active == machineMode.Id) {
              defaultReason =
                ModelDAOHelper.ModelFactory.CreateMachineModeDefaultReason (machineMode,
                                                                            machineObservationState);
              defaultReason.Reason = reasonMotion;
              defaultReason.Score = DEFAULT_ACTIVE_SCORE;
              defaultReason.Auto = true;
              if (null == reasonMotion) {
                log.WarnFormat ("InsertDefaultValues: " +
                                "please add manually a default reason for machine mode Motion {0} " +
                                "because no reasonMotion Id=2 exists",
                                machineMode);
              }
              else {
                log.InfoFormat ("InsertDefaultValues: " +
                                "about to add a new MachineModeDefaultReason Motion for " +
                                "machineMode={0} machineObservationState={1}",
                                machineMode, machineObservationState);
                ModelDAOHelper.DAOFactory.MachineModeDefaultReasonDAO.MakePersistent (defaultReason);
              }
            }
            else if ((int)MachineModeId.Inactive == machineMode.Id) { // Inactive => undefined
              defaultReason =
                ModelDAOHelper.ModelFactory.CreateMachineModeDefaultReason (machineMode,
                                                                            machineObservationState);
              defaultReason.Reason = reasonUndefined;
              defaultReason.Score = DEFAULT_INACTIVE_SCORE;
              if (null == reasonUndefined) {
                log.WarnFormat ("InsertDefaultValues: " +
                                "please add manually a default reason for machine mode {0} " +
                                "because no reasonUndefined Id=1 exists",
                                machineMode);
              }
              else {
                log.WarnFormat ("InsertDefaultValues: " +
                                "please set manually a default reason for " +
                                "machineMode={0} machineObservationState={1} " +
                                "=> for the moment the undefined reason is set",
                                machineMode, machineObservationState);
                ModelDAOHelper.DAOFactory.MachineModeDefaultReasonDAO.MakePersistent (defaultReason);
              }
            }
            else if ((int)MachineModeId.Unknown == machineMode.Id) { // Unknown => Unknown
              defaultReason =
                ModelDAOHelper.ModelFactory.CreateMachineModeDefaultReason (machineMode,
                                                                            machineObservationState);
              defaultReason.Reason = reasonUnknown;
              defaultReason.Score = DEFAULT_UNKNOWN_SCORE;
              if (null == reasonUnknown) {
                log.WarnFormat ("InsertDefaultValues: " +
                                "please add manually a default reason for machine mode {0} " +
                                "because no reasonUnknown Id=7 exists",
                                machineMode);
              }
              else {
                log.InfoFormat ("InsertDefaultValues: " +
                                "about to add a new MachineModeDefaultReason Unknown for " +
                                "machineMode={0} machineObservationState={1}",
                                machineMode, machineObservationState);
                ModelDAOHelper.DAOFactory.MachineModeDefaultReasonDAO.MakePersistent (defaultReason);
              }
            }
            else { // Another top machine mode => Undefined
              defaultReason = ModelDAOHelper.ModelFactory
                .CreateMachineModeDefaultReason (machineMode, machineObservationState);
              defaultReason.Reason = reasonUndefined;
              defaultReason.Score = DEFAULT_SCORE;
              if (null == reasonUndefined) {
                log.WarnFormat ("InsertDefaultValues: " +
                                "please add manually a default reason for machine mode {0} " +
                                "because no reasonUndefined Id=1 exists",
                                machineMode);
              }
              else {
                log.WarnFormat ("InsertDefaultValues: " +
                                "please set manually a default reason for " +
                                "machineMode={0} machineObservationState={1} " +
                                "=> for the moment the undefined reason is set",
                                machineMode, machineObservationState);
                ModelDAOHelper.DAOFactory.MachineModeDefaultReasonDAO.MakePersistent (defaultReason);
              }

            }
          }
        }
      }
    }
    #endregion // DefaultValues

    /// <summary>
    /// Get all the items with an early fetch of the reason and of the reason group
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IMachineModeDefaultReason> FindWithReasonGroup ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModeDefaultReason> ()
        .Fetch (SelectMode.Fetch, "Reason")
        .Fetch (SelectMode.Fetch, "Reason.ReasonGroup")
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .List<IMachineModeDefaultReason> ();
    }

    /// <summary>
    /// Get all the items sorted for a given MachineMode and a given MachineObservationState
    /// 
    /// Note: there are some eager fetches and this is not registered to be cacheable
    /// </summary>
    /// <param name="machineMode"></param>
    /// <param name="machineObservationState"></param>
    /// <returns></returns>
    public IList<IMachineModeDefaultReason> FindWithForConfig (IMachineMode machineMode,
                                                               IMachineObservationState machineObservationState)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModeDefaultReason> ()
        .Add (Restrictions.Eq ("MachineMode", machineMode))
        .Add (Restrictions.Eq ("MachineObservationState",
                               machineObservationState))
        .Fetch (SelectMode.Fetch, "Reason")
        .Fetch (SelectMode.Fetch, "MachineMode")
        .Fetch (SelectMode.Fetch, "IncludeMachineFilter")
        .Fetch (SelectMode.Fetch, "ExcludeMachineFilter")
        .AddOrder (Order.Asc ("Id"))
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .List<IMachineModeDefaultReason> ();
    }

    /// <summary>
    /// Find all the entities given one or more MachineMode and one or more MachineObservationState
    /// Entities are grouped with config. sameness
    /// with some children that are eager fetched
    /// </summary>
    /// <param name="machineModes"></param>
    /// <param name="machineObservationStates"></param>
    /// <returns></returns>
    public IList<IMachineModeDefaultReason> FindWithForConfig (IList<IMachineMode> machineModes,
                                                              IList<IMachineObservationState> machineObservationStates)
    {
      IList<IMachineModeDefaultReason> machineModeDefaultReasons;

      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModeDefaultReason> ()
        .Fetch (SelectMode.Fetch, "Reason")
        .Fetch (SelectMode.Fetch, "MachineMode")
        .Fetch (SelectMode.Fetch, "IncludeMachineFilter")
        .Fetch (SelectMode.Fetch, "ExcludeMachineFilter")
        .Fetch (SelectMode.Fetch, "MachineMode")
        .Fetch (SelectMode.Fetch, "MachineObservationState")
        .AddOrder (Order.Asc ("Id"));

      //Criteria Build
      if (machineModes.Count > 1 | machineObservationStates.Count > 1) {
        Junction mModeDisjunction = Restrictions.Disjunction ();
        foreach (IMachineMode mMode in machineModes) {
          mModeDisjunction.Add (Restrictions.Eq ("MachineMode", mMode));
        }
        criteria.Add (mModeDisjunction);

        Junction mObserStateDisjunction = Restrictions.Disjunction ();
        foreach (IMachineObservationState mObservationState in machineObservationStates) {
          mObserStateDisjunction.Add (Restrictions.Eq ("MachineObservationState", mObservationState));
        }
        criteria.Add (mObserStateDisjunction);

        machineModeDefaultReasons = criteria.List<IMachineModeDefaultReason> ();
      }
      else {
        criteria.Add (Restrictions.Eq ("MachineMode", machineModes[0]));
        criteria.Add (Restrictions.Eq ("MachineObservationState",
                                       machineObservationStates[0]));
        machineModeDefaultReasons = criteria.List<IMachineModeDefaultReason> ();
      }

      return machineModeDefaultReasons;
    }

    /// <summary>
    /// Find all the entities given a machine mode
    /// </summary>
    /// <param name="machineMode"></param>
    /// <returns></returns>
    public IList<IMachineModeDefaultReason> FindByMachineMode (IMachineMode machineMode)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModeDefaultReason> ()
        .Add (Restrictions.Eq ("MachineMode", machineMode))
        .List<IMachineModeDefaultReason> ();
    }

    /// <summary>
    /// Get all the items sorted for a given MachineMode and a given MachineObservationState
    /// 
    /// The result is ordered by ascending duration
    /// 
    /// The request is recursive. If no configuration was found for the specified machine mode,
    /// another try is made with the parent
    ///
    /// Note: this is registered to be cacheable.
    ///       Do not consider here, the children are always automatically fetched because of the cacheable property.
    /// </summary>
    /// <param name="machineMode">not null</param>
    /// <param name="machineObservationState">not null</param>
    /// <returns></returns>
    IList<IMachineModeDefaultReason> FindWith (IMachineMode machineMode,
                                               IMachineObservationState machineObservationState)
    {
      Debug.Assert (null != machineMode);
      Debug.Assert (null != machineObservationState);

      // In the request below, Id is used to get profit of the query cache
      // without having to unproxy some entities (which may cause some LazyInitialization problems)
      IList<IMachineModeDefaultReason> list = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModeDefaultReason> ()
        .Add (Restrictions.Eq ("MachineMode.Id", machineMode.Id))
        .Add (Restrictions.Eq ("MachineObservationState.Id",
                               machineObservationState.Id))
        .Fetch (SelectMode.Fetch, "Reason")
        .Fetch (SelectMode.Fetch, "MachineMode")
        .Fetch (SelectMode.Fetch, "IncludeMachineFilter")
        .Fetch (SelectMode.Fetch, "ExcludeMachineFilter")
        .AddOrder (Order.Asc ("MaximumDuration")) // implies null values are greater than not null values, ok in PostgreSQL
        .SetCacheable (true)
        // Note: Although SetCacheable may not behave so well with the FetchMode, consider here
        //       this is used only by the Analysis service, and inside a session,
        //       so there will be no problem of LazyInitialization even if the cache is used
        .List<IMachineModeDefaultReason> ();
      if (0 == list.Count) {
        // Because of some Lazy initialization problems,
        // get the corresponding machineMode that is associated to the session
        IMachineMode attachedMachineMode;
        if (0 == machineMode.Id) {
          attachedMachineMode = machineMode;
        }
        else {
          attachedMachineMode = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById (machineMode.Id);
          Debug.Assert (null != attachedMachineMode);
        }
        if (null == attachedMachineMode.Parent) {
          return list;
        }
        else { // Else try with the parent
          log.DebugFormat ("FindWith: " +
                           "try with the parent {0}",
                           attachedMachineMode.Parent.Id);
          return FindWith (attachedMachineMode.Parent,
                           machineObservationState);
        }
      }
      else {
        return list;
      }
    }

    /// <summary>
    /// Get all the items sorted for a specified MonitoredMachine
    /// 
    /// Note: this is registered to be cacheable.
    ///       Do not consider here, the children are always automatically fetched because of the cacheable property.
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public IEnumerable<IMachineModeDefaultReason> FindWith (IMachine machine)
    {
      Debug.Assert (null != machine);

      var allDefaultReasons = this.FindAll ();
      var filteredDefaultReasons = allDefaultReasons
        .Where (d => d.IsApplicableToMachine (machine))
        .ToList (); // Use ToList to remove an optimization and force IsApplicableToMachine to be executed right now. Else the Where () may not be executed correctly (not sure why yet, probably because else a lazy run is used and IsApplicableToMachine is not run inside a transaction)
      return filteredDefaultReasons;
    }

    /// <summary>
    /// Get all the items sorted for a specified MonitoredMachine, MachineMode and MachineObservationState
    /// 
    /// The result is ordered by ascending duration
    ///
    /// The request is recursive. If no configuration was found for the specified machine mode,
    /// another try is made with the parent
    ///
    /// Note: this is registered to be cacheable.
    ///       Do not consider here, the children are always automatically fetched because of the cacheable property.
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="machineMode">not null</param>
    /// <param name="machineObservationState">not null</param>
    /// <returns></returns>
    public IEnumerable<IMachineModeDefaultReason> FindWith (IMachine machine,
                                                            IMachineMode machineMode,
                                                            IMachineObservationState machineObservationState)
    {
      Debug.Assert (null != machine);

      var allDefaultReasons = FindWith (machineMode,
                                     machineObservationState);
      var filteredDefaultReasons = allDefaultReasons
        .Where (d => d.IsApplicableToMachine (machine))
        .ToList (); // Use ToList to remove an optimization and force IsApplicableToMachine to be executed right now. Else the Where () may not be executed correctly (not sure why yet, probably because else a lazy run is used and IsApplicableToMachine is not run inside a transaction)
      if (!filteredDefaultReasons.Any ()) {
        // Because of some Lazy initialization problems,
        // get the corresponding machineMode that is associated to the session
        IMachineMode attachedMachineMode;
        if (0 == machineMode.Id) {
          attachedMachineMode = machineMode;
        }
        else {
          attachedMachineMode = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById (machineMode.Id);
          Debug.Assert (null != attachedMachineMode);
        }
        if (null == attachedMachineMode.Parent) {
          return filteredDefaultReasons;
        }
        else { // Else try with the parent
          log.DebugFormat ("FindWith: " +
                           "try with the parent {0}",
                           attachedMachineMode.Parent.Id);
          return FindWith (machine,
                           attachedMachineMode.Parent,
                           machineObservationState);
        }
      }
      return filteredDefaultReasons;
    }

    /// <summary>
    /// Find the first entity that matches a Machine, a MachineMode, a MachineObservationState and a duration
    /// 
    /// The request is recursive. If no configuration was found for the specified machine mode,
    /// another try is made with the parent
    /// 
    /// null is returned in case no matching entity could be found, which corresponds to an unexpected configuration
    ///
    /// Note: this is registered to be cacheable.
    ///       Do not consider here, the children are always automatically fetched because of the cacheable property.
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="machineMode">not null</param>
    /// <param name="machineObservationState">not null</param>
    /// <param name="duration"></param>
    /// <returns></returns>
    public IMachineModeDefaultReason FindWith (IMachine machine,
                                               IMachineMode machineMode,
                                               IMachineObservationState machineObservationState,
                                               TimeSpan duration)
    {
      var entities = FindWith (machine,
                               machineMode,
                               machineObservationState);
      foreach (var entity in entities) {
        if (!entity.MaximumDuration.HasValue || (duration <= entity.MaximumDuration.Value)) {
          return entity;
        }
      }

      log.ErrorFormat ("FindWith: " +
                       "no default reason matches machine {0} machineMode {1} machineObservationState {2}",
                       machine, machineMode, machineObservationState);
      return null;
    }
  }
}
