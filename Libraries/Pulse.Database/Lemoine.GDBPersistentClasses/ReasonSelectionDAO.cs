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
  /// Implementation of <see cref="Lemoine.ModelDAO.IReasonSelectionDAO">IReasonSelectionDAO</see>
  /// </summary>
  public class ReasonSelectionDAO
    : VersionableNHibernateDAO<ReasonSelection, IReasonSelection, int>
    , IReasonSelectionDAO
  {
    readonly ILog log = LogManager.GetLogger(typeof (ReasonSelectionDAO).FullName);

    /// <summary>
    /// Insert the default values
    /// </summary>
    internal void InsertDefaultValues ()
    {
      IReason reasonUndefined = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Undefined);
      
      IList<IMachineMode> machineModes = ModelDAOHelper.DAOFactory
        .MachineModeDAO.FindAll ();
      IList<IMachineObservationState> machineObservationStates = ModelDAOHelper.DAOFactory
        .MachineObservationStateDAO.FindAll ();
      foreach (IMachineMode machineMode in machineModes.Where (mm => (null == mm.Parent))) {
        foreach (IMachineObservationState machineObservationState in machineObservationStates) {
          var reasonSelections =
            ModelDAOHelper.DAOFactory.ReasonSelectionDAO
            .FindWithForConfig (machineMode, machineObservationState);
          if (!reasonSelections.Any ()) {
            var defaultReasons = ModelDAOHelper.DAOFactory.MachineModeDefaultReasonDAO
              .FindWithForConfig (machineMode, machineObservationState);
            foreach (var reason in defaultReasons
              .Where (d => !object.Equals (d.Reason, reasonUndefined))
              .Select (d => d.Reason)
              .Distinct ()) {
              var reasonSelection = ModelDAOHelper.ModelFactory
                .CreateReasonSelection (machineMode, machineObservationState);
              reasonSelection.Selectable = false;
              reasonSelection.Reason = reason;
              ModelDAOHelper.DAOFactory.ReasonSelectionDAO
                .MakePersistent (reasonSelection);
            }
          }
        }
      }
    }
    
    /// <summary>
    /// Get all the items sorted for a given MachineMode and a given MachineObservationState
    /// 
    /// Note: there are some eager fetches and this is not registered to be cacheable
    /// </summary>
    /// <param name="machineMode"></param>
    /// <param name="machineObservationState"></param>
    /// <returns></returns>
    public IList<IReasonSelection> FindWithForConfig (IMachineMode machineMode,
                                                      IMachineObservationState machineObservationState)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonSelection> ()
        .Add (Restrictions.Eq ("MachineMode", machineMode))
        .Add (Restrictions.Eq ("MachineObservationState",
                               machineObservationState))
        .Fetch (SelectMode.Fetch, "Reason")
        .Fetch (SelectMode.Fetch, "Reason.ReasonGroup")
        .Fetch (SelectMode.Fetch, "MachineFilter")
        .AddOrder(Order.Asc("Id"))
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .List<IReasonSelection> ();
    }
    
    /// <summary>
    /// Find all the entities given one or more MachineMode and one or more MachineObservationState
    /// Entities are grouped with config. sameness
    /// with some children that are eager fetched
    /// </summary>
    /// <param name="machineModes">not null and not empty</param>
    /// <param name="machineObservationStates">not null and not empty</param>
    /// <returns></returns>
    public IList<IReasonSelection> FindWithForConfig(IList<IMachineMode> machineModes,
                                                     IList<IMachineObservationState> machineObservationStates)
    {
      Debug.Assert (null != machineModes);
      Debug.Assert (1 <= machineModes.Count);
      Debug.Assert (null != machineObservationStates);
      Debug.Assert (1 <= machineObservationStates.Count);

      IList<IReasonSelection> reasonSelections;
      
      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonSelection> ()
        .Fetch (SelectMode.Fetch, "Reason")
        .Fetch (SelectMode.Fetch, "MachineFilter")
        .Fetch (SelectMode.Fetch, "MachineMode")
        .Fetch (SelectMode.Fetch, "MachineObservationState")
        .AddOrder(Order.Asc("Id"));
      
      //Criteria Build
      if( machineModes.Count > 1 | machineObservationStates.Count > 1) {
        Junction mModeDisjunction = Restrictions.Disjunction();
        foreach(IMachineMode mMode in machineModes){
          mModeDisjunction.Add (Restrictions.Eq ("MachineMode", mMode));
        }
        criteria.Add(mModeDisjunction);
        
        Junction mObserStateDisjunction = Restrictions.Disjunction();
        foreach(IMachineObservationState mObservationState in machineObservationStates) {
          mObserStateDisjunction.Add (Restrictions.Eq ("MachineObservationState",mObservationState));
        }
        criteria.Add(mObserStateDisjunction);
        
        reasonSelections = criteria.List<IReasonSelection> ();
      }
      else {
        criteria.Add (Restrictions.Eq ("MachineMode", machineModes[0]));
        criteria.Add (Restrictions.Eq ("MachineObservationState",
                                       machineObservationStates[0]));
        reasonSelections = criteria.List<IReasonSelection> ();
      }
      
      return reasonSelections;
    }
    
    /// <summary>
    /// Find all the entities given a MachineMode
    /// </summary>
    /// <param name="machineMode"></param>
    /// <returns></returns>
    public IList<IReasonSelection> FindByMachineMode (IMachineMode machineMode)
    {
      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<ReasonSelection>()
        .Add (Restrictions.Eq("MachineMode", machineMode))
        .List<IReasonSelection>();
    }
    
    /// <summary>
    /// Get all the items for a given MachineMode and a given MachineObservationState
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
    public IList<IReasonSelection> FindWith (IMachineMode machineMode,
                                             IMachineObservationState machineObservationState)
    {
      Debug.Assert (null != machineMode);
      Debug.Assert (null != machineObservationState);
      
      // In the request below, Id is used to get profit of the query cache
      // without having to unproxy some entities (which may cause some LazyInitialization problems)
      IList<IReasonSelection> list = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonSelection> ()
        .Add (Restrictions.Eq ("MachineMode.Id", machineMode.Id))
        .Add (Restrictions.Eq ("MachineObservationState.Id",
                               machineObservationState.Id))
        .Fetch (SelectMode.Fetch, "MachineMode")
        .Fetch (SelectMode.Fetch, "Reason")
        .Fetch (SelectMode.Fetch, "Reason.ReasonGroup")
        .Fetch (SelectMode.Fetch, "MachineFilter")
        .SetCacheable (true)
        // Note: Although SetCacheable may not behave so well with the FetchMode, consider here
        //       this is used only by the Web+Analysis service, and inside a session,
        //       so there will be no problem of LazyInitialization even if the cache is used
        .List<IReasonSelection> ();
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
    /// Get all the items sorted for a specified MonitoredMachine, MachineMode and MachineObservationState
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
    public IEnumerable<IReasonSelection> FindWith (IMachine machine,
                                                   IMachineMode machineMode,
                                                   IMachineObservationState machineObservationState)
    {
      Debug.Assert (null != machine);
      
      IEnumerable<IReasonSelection> reasonSelections = FindWith (machineMode,
                                                                 machineObservationState)
        .Where (reasonSelection
                =>
                (null == reasonSelection.MachineFilter)
                || (reasonSelection.MachineFilter.IsMatch (machine)));
      if (!reasonSelections.Any ()) {
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
          return reasonSelections;
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
      return reasonSelections;
    }
    
    /// <summary>
    /// Find all the possible reasons that are set in ReasonSelection
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IReason> FindReasons ()
    {
      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<ReasonSelection>()
        .Fetch (SelectMode.Fetch, "Reason")
        .Fetch (SelectMode.Fetch, "Reason.ReasonGroup")
        .List<IReasonSelection>()
        .Where (s => null != s.Reason)
        .Select (s => s.Reason)
        .Distinct ();
    }
  }
}
